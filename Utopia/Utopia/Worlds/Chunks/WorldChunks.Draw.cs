﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Settings;
using Utopia.Resources.Effects.Terran;
using Utopia.Resources.Effects.Entities;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.Textures;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Maths;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Will contains world block landscape stored as Chunks.
    /// Concentrate to the rendering of the chunks !
    /// </summary>
    public partial class WorldChunks : IWorldChunks
    {
        #region private variables

        private HLSLTerran _terraEffect;
        private HLSLLiquid _liquidEffect;
        private HLSLStaticEntitySprite _staticSpriteEffect;
        private int _chunkDrawByFrame;
        private ShaderResourceView _terra_View;
        private ShaderResourceView _spriteTexture_View;
        #endregion

        #region public variables
        #endregion

        #region Public methods

        public override void Draw(DeviceContext context, int index)
        {
            
            if (index == SOLID_DRAW)
            {
                _chunkDrawByFrame = 0;

                RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                DrawSolidFaces(context);

#if DEBUG
                if (ShowDebugInfo)
                {
                    DrawDebug(context);
                }
#endif

                return;
            }

            if (index == TRANSPARENT_DRAW)
            {
                //Only 2 index registered, no need to test the value of the index here it is for transparent one !
                if (!_playerManager.IsHeadInsideWater)
                {
                    //Head not inside Water => Draw water front Faces
                    RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                }
                else
                {
                    //Head inside Water block, draw back faces only
                    RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.CullFront, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                }
                DefaultDrawLiquid(context);
                return;
            }

            if (index == ENTITIES_DRAW)
            {
                RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                DrawStaticEntities(context);
                return;
            }

        }
        #endregion

        #region Private methods

#if DEBUG
        private void DrawDebug(DeviceContext context)
        {
            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (chunk.IsReady2Draw && !chunk.isFrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    chunk.DrawDebugBoundingBox(context);
                }
            }
        }
#endif

        private void DrawSolidFaces(DeviceContext context)
        {
            VisualChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            _terraEffect.Begin(context);

            //Foreach faces type
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.IsReady2Draw)
                {
                    if (!chunk.isFrustumCulled)
                    {
                        _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                        _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _terraEffect.CBPerDraw.Values.popUpYOffset = 0;
                        _terraEffect.CBPerDraw.Values.Opaque = chunk.Opaque;
                        _terraEffect.CBPerDraw.IsDirty = true;
                        _terraEffect.Apply(context);

                        chunk.DrawSolidFaces(context);

                        _chunkDrawByFrame++;
                    }
                }
            }
        }

        //Default Liquid Drawing
        private void DefaultDrawLiquid(DeviceContext context)
        {
            Matrix worldFocus = Matrix.Identity;

            VisualChunk chunk;

            _liquidEffect.Begin(context);

            _liquidEffect.SolidBackBuffer.Value = _solidBackBuffer.SolidStaggingBackBuffer;
            _liquidEffect.SolidBackBuffer.IsDirty = true;

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (chunk.IsReady2Draw && !chunk.isFrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    //Only If I have something to draw !
                    if (chunk.LiquidCubeVB != null)
                    {
                        _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                        _liquidEffect.CBPerDraw.Values.popUpYOffset = 0;
                        _liquidEffect.CBPerDraw.Values.Opaque = chunk.Opaque;
                        _liquidEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _liquidEffect.CBPerDraw.IsDirty = true;
                        _liquidEffect.Apply(context);
                        chunk.DrawLiquidFaces(context);
                    }
                }
            }
        }

        private void DrawStaticEntities(DeviceContext context)
        {
            VisualChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            _staticSpriteEffect.Begin(context);
            _staticSpriteEffect.CBPerFrameLocal.Values.WorldFocus = Matrix.Transpose(_worldFocusManager.CenterOnFocus(ref MMatrix.Identity));

            // Calculate the rotation that needs to be applied to the billboard model to face the current camera position using the arc tangent function.
            //    angle = atan2(modelPosition.x - cameraPosition.x, modelPosition.z - cameraPosition.z) * (180.0 / D3DX_PI);

            //    // Convert rotation into radians.
            //    rotation = (float)angle * 0.0174532925f;
            //Use the rotation to first rotate the world matrix accordingly, and then translate to the position of the billboard in the world.

            //    // Setup the rotation the billboard at the origin using the world matrix.
            //    D3DXMatrixRotationY(&worldMatrix, rotation);

            _staticSpriteEffect.CBPerFrameLocal.Values.View = Matrix.RotationQuaternion(Quaternion.Conjugate(_camManager.ActiveCamera.YAxisOrientation));
            _staticSpriteEffect.CBPerFrameLocal.Values.WindPower = _weather.Wind.FlatWindFlowNormalizedWithNoise;
            _staticSpriteEffect.CBPerFrameLocal.Values.KeyFrameAnimation = (float)_weather.Wind.KeyFrameAnimation;
            _staticSpriteEffect.CBPerFrameLocal.IsDirty = true;
            _staticSpriteEffect.Apply(context);

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.IsReady2Draw)
                {
                    if (!chunk.isFrustumCulled)
                    {
                        chunk.DrawStaticEntities(context);
                    }
                }
            }
        }

        private void InitDrawComponents(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _terra_View);

            _terraEffect = new HLSLTerran(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration, _sharedFrameCB.CBPerFrame);
            _terraEffect.TerraTexture.Value = _terra_View;
            _terraEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);

            _liquidEffect = new HLSLLiquid(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration, _sharedFrameCB.CBPerFrame);
            _liquidEffect.TerraTexture.Value = _terra_View;
            _liquidEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
            _liquidEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipPoint);

            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Sprites/", @"*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _spriteTexture_View);
            _staticSpriteEffect = new HLSLStaticEntitySprite(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities/StaticEntitySprite.hlsl", VertexSprite3D.VertexDeclaration, _sharedFrameCB.CBPerFrame);
            _staticSpriteEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
            _staticSpriteEffect.DiffuseTexture.Value = _spriteTexture_View;
        }

        private void DisposeDrawComponents()
        {
            _terra_View.Dispose();
            _liquidEffect.Dispose();
            _terraEffect.Dispose();
            _spriteTexture_View.Dispose();
            _staticSpriteEffect.Dispose();
        }
        #endregion

        #region GetInfo Interface
        public string GetInfo()
        {
            int BprimitiveCount = 0;
            int VprimitiveCount = 0;
            //Run over all chunks to see their status, and take action accordingly.
            for (int chunkIndice = 0; chunkIndice < VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y; chunkIndice++)
            {
                if (SortedChunks[chunkIndice].SolidCubeIB == null) continue;
                if (!SortedChunks[chunkIndice].isFrustumCulled)
                {
                    VprimitiveCount += SortedChunks[chunkIndice].SolidCubeIB.IndicesCount;
                    if (SortedChunks[chunkIndice].LiquidCubeIB != null) VprimitiveCount += (SortedChunks[chunkIndice].LiquidCubeIB.IndicesCount);
                }
                BprimitiveCount += SortedChunks[chunkIndice].SolidCubeIB.IndicesCount;
                if (SortedChunks[chunkIndice].LiquidCubeIB != null) BprimitiveCount += (SortedChunks[chunkIndice].LiquidCubeIB.IndicesCount);
            }
            return string.Concat("<TerraCube Mod> BChunks : ", VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y, "; BPrim : ", BprimitiveCount, " DChunks : ", _chunkDrawByFrame, " DPrim : ", VprimitiveCount);
        }
        #endregion
    }
}
