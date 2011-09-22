﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using UtopiaContent.Effects.Terran;
using S33M3Engines.Struct.Vertex;
using SharpDX.Direct3D11;
using S33M3Engines.Textures;
using S33M3Engines.StatesManager;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Chunks;

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
        private int _chunkDrawByFrame;
        private float _sunColorBase;
        public ShaderResourceView _terra_View;
        #endregion

        #region public variables
        #endregion

        #region Public methods

        public override void Draw(int Index)
        {
            

#if DEBUG
            if (_gameStates.DebugDisplay == 2) StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Wired, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#endif
            _chunkDrawByFrame = 0;

            _terraEffect.Begin();
            _terraEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _terraEffect.CBPerFrame.Values.dayTime = _gameClock.ClockTime.ClockTimeNormalized2;
            _terraEffect.CBPerFrame.Values.fogdist = ((VisualWorldParameters.WorldVisibleSize.X) / 2) - 48;
            _terraEffect.CBPerFrame.IsDirty = true;
            _sunColorBase = GetSunColor();

            if (_playerManager.IsHeadInsideWater) _terraEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase / 3, _sunColorBase / 3, _sunColorBase);
            else _terraEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase, _sunColorBase, _sunColorBase);

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            if (Index == SOLID_DRAW)
            {
                DrawSolidFaces();
#if DEBUG
                DrawDebug();
#endif
            }
            else
            {
                //Only 2 index registered, no need to test the value of the index here it is for transparent one !
                if (!_playerManager.IsHeadInsideWater)
                {
                    //Head not inside Water => Draw water front Faces
                    StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                }
                else
                {
                    //Head inside Water block, draw back faces only
                    StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullFront, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                }

                DefaultDrawLiquid(); 
            }

        }
        #endregion

        #region Private methods
        private void DrawSolidFaces()
        {
            VisualChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            //Foreach faces type
            for (int chunkIndice = 0; chunkIndice < VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.Ready2Draw)
                {
                    //Only checking Frustum with the faceID = 0
                    //chunk.isFrustumCulled = !_camManager.ActiveCamera.Frustum.Intersects(chunk.ChunkWorldBoundingBox);

                    if (!chunk.isFrustumCulled)
                    {
                        _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                        _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _terraEffect.CBPerDraw.Values.popUpYOffset = 0;
                        _terraEffect.CBPerDraw.IsDirty = true;
                        _terraEffect.Apply();

                        chunk.DrawSolidFaces();

                        _chunkDrawByFrame++;
                    }
                }
            }
        }

        //Default Liquid Drawing
        private void DefaultDrawLiquid()
        {
            Matrix worldFocus = Matrix.Identity;

#if DEBUG
            if (_gameStates.DebugDisplay == 2) StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Wired, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#endif

            VisualChunk chunk;

            _liquidEffect.Begin();
            _liquidEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            if (_playerManager.IsHeadInsideWater) _liquidEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase / 3, _sunColorBase / 3, _sunColorBase);
            else _liquidEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase, _sunColorBase, _sunColorBase);

            _liquidEffect.CBPerFrame.IsDirty = true;

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (chunk.Ready2Draw && !chunk.isFrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    //Only If I have something to draw !
                    if (chunk.LiquidCubeVB != null)
                    {
                        _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                        _liquidEffect.CBPerDraw.Values.popUpYOffset = 0;
                        _liquidEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _liquidEffect.CBPerDraw.IsDirty = true;
                        _liquidEffect.Apply();
                        chunk.DrawLiquidFaces();
                    }
                }
            }
        }

        private float GetSunColor()
        {
            float SunColorBase;
            if (_gameClock.ClockTime.ClockTimeNormalized <= 0.2083944 || _gameClock.ClockTime.ClockTimeNormalized > 0.9583824) // Between 23h00 and 05h00 => Dark night
            {
                SunColorBase = 0.05f;
            }
            else
            {
                if (_gameClock.ClockTime.ClockTimeNormalized > 0.2083944 && _gameClock.ClockTime.ClockTimeNormalized <= 0.4166951) // Between 05h00 and 10h00 => Go to Full Day
                {
                    SunColorBase = MathHelper.FullLerp(0.05f, 1, 0.2083944, 0.4166951, _gameClock.ClockTime.ClockTimeNormalized);
                }
                else
                {
                    if (_gameClock.ClockTime.ClockTimeNormalized > 0.4166951 && _gameClock.ClockTime.ClockTimeNormalized <= 0.6666929) // Between 10h00 and 16h00 => Full Day
                    {
                        SunColorBase = 1f;
                    }
                    else
                    {
                        SunColorBase = MathHelper.FullLerp(1, 0.05f, 0.6666929, 0.9583824, _gameClock.ClockTime.ClockTimeNormalized); //Go to Full night
                    }
                }
            }

            return SunColorBase;
        }

#if DEBUG
        private void DrawDebug()
        {
            VisualChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            for (int chunkIndice = 0; chunkIndice < VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.Ready2Draw)
                {
                    if (!chunk.isFrustumCulled)
                    {
                        if (_gameStates.DebugDisplay == 1) chunk.ChunkBoundingBoxDisplay.Draw(_camManager.ActiveCamera, _worldFocusManager.WorldFocus);
                    }

                }
            }
        }
#endif

        private void InitDrawComponents()
        {
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_WorldChunk", out _terra_View);

            _terraEffect = new HLSLTerran(_d3dEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);
            _liquidEffect = new HLSLLiquid(_d3dEngine, @"Effects/Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration);

            _terraEffect.TerraTexture.Value = _terra_View;
            _terraEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);

            _liquidEffect.TerraTexture.Value = _terra_View;
            _liquidEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);
        }

        private void DisposeDrawComponents()
        {
            _terra_View.Dispose();
            _liquidEffect.Dispose();
            _terraEffect.Dispose();
        }
        #endregion

        #region GetInfo Interface
        public string GetInfo()
        {
            int BprimitiveCount = 0;
            int VprimitiveCount = 0;
            //Run over all chunks to see their status, and take action accordingly.
            for (int chunkIndice = 0; chunkIndice < VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z; chunkIndice++)
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
            return string.Concat("<TerraCube Mod> BChunks : ", VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z, "; BPrim : ", BprimitiveCount, " DChunks : ", _chunkDrawByFrame, " DPrim : ", VprimitiveCount);
        }
        #endregion
        


    }
}
