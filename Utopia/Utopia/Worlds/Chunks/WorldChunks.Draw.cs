﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Resources.Effects.Terran;
using Utopia.Resources.Effects.Entities;
using S33M3DXEngine.RenderStates;
using S33M3DXEngine.Textures;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Maths;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Resources.VertexFormats;
using Utopia.Entities.Voxel;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Will contains world block landscape stored as Chunks.
    /// Concentrate to the rendering of the chunks !
    /// </summary>
    public partial class WorldChunks : IWorldChunks
    {
        #region Private variables
        private HLSLTerran _terraEffect;
        private HLSLLiquid _liquidEffect;

        private HLSLVoxelModel _voxelModelEffect;
        private HLSLVoxelModelInstanced _voxelModelInstancedEffect;

        private int _chunkDrawByFrame;
        private ShaderResourceView _terra_View;
        //private ShaderResourceView _spriteTexture_View;
        private ShaderResourceView _biomesColors_View;
        private ShaderResourceView _textureAnimation_View;

        private int _staticEntityDrawCalls;
        private double _staticEntityDrawTime;

        #endregion

        public ShaderResourceView Terra_View { get { return _terra_View; } }

        #region Public methods

        private void ChunkVisibilityTest()
        {
            foreach (var chunk in SortedChunks)
            {
                chunk.Graphics.IsFrustumCulled = !_camManager.ActiveCamera.Frustum.IntersectsWithoutFar(ref chunk.ChunkWorldBoundingBox);
            }
        }
        
        public override void Draw(DeviceContext context, int index)
        {

            if (index == SOLID_DRAW)
            {

                _chunkDrawByFrame = 0;

                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
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
                if (!PlayerManager.IsHeadInsideWater)
                {
                    //Head not inside Water => Draw water front Faces
                    RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);
                }
                else
                {
                    //Head inside Water block, draw back faces only
                    RenderStatesRepo.ApplyStates(context, DXStates.Rasters.CullFront, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);
                }

                DefaultDrawLiquid(context);
                return;
            }

            if (index == ENTITIES_DRAW)
            {
                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
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
                if (chunk.Graphics.NeedToRender) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    chunk.DrawDebugBoundingBox(context);
                }
            }
        }
#endif

        public bool IsEntityVisible(Vector3D pos)
        {
            if (_sliceValue == -1)
                return true;

            return pos.Y < _sliceValue + 1 && pos.Y > _sliceValue - 6;
        }

        public IEnumerable<VisualChunk> ChunksToDraw(bool sameSlice = true)
        {
            //var chunksLimit = _sliceValue == -1 ? SortedChunks.Length : Math.Min(SortedChunks.Length, SliceViewChunks);
            
            //for (int chunkIndice = 0; chunkIndice < chunksLimit; chunkIndice++)
            //{
            //    var chunk = SortedChunks[chunkIndice];

            //    if (chunk.isExistingMesh4Drawing && !chunk.isFrustumCulled)
            //        yield return chunk;
            //}

            if (_sliceValue == -1)
            {
                for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                {
                    var chunk = SortedChunks[chunkIndice];

                    if (chunk.Graphics.NeedToRender)
                        yield return chunk;
                }
            }
            else
            {
                var pos = PlayerManager.Player.Position.ToCubePosition();
                var playerChunk = GetChunk(ref pos);

                if (SliceViewChunks <= 9)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        for (int z = -1; z < 2; z++)
                        {
                            var chunk = GetChunkFromChunkCoord(playerChunk.Position + new Vector3I(x, 0, z));

                            if (chunk.Graphics.NeedToRender && (!sameSlice || chunk.Graphics.SliceOfMesh == _sliceValue))
                                yield return chunk;
                        }
                    }
                }
                else if (SliceViewChunks <= 25)
                {
                    for (int x = -2; x < 3; x++)
                    {
                        for (int z = -2; z < 3; z++)
                        {
                            var chunk = GetChunkFromChunkCoord(playerChunk.Position + new Vector3I(x, 0, z));

                            if (chunk.Graphics.NeedToRender && (!sameSlice || chunk.Graphics.SliceOfMesh == _sliceValue))
                                yield return chunk;
                        }
                    }
                }
            }

        }

        private void DrawSolidFaces(DeviceContext context)
        {
            Matrix worldFocus = Matrix.Identity;

            _terraEffect.Begin(context);

            if (ShadowMap.ShadowMap != null)
            {
                //Depth Shadow Mapping !
                _terraEffect.ShadowMap.Value = ShadowMap.ShadowMap.DepthMap;
                _terraEffect.ShadowMap.IsDirty = true;
            }

            foreach (var chunk in ChunksToDraw())
            {
                _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                _terraEffect.CBPerDraw.Values.PopUpValue = chunk.PopUpValue.ValueInterp;
                _terraEffect.CBPerDraw.Values.LightViewProjection = Matrix.Transpose(ShadowMap.LightViewProjection);
                _terraEffect.CBPerDraw.Values.SunVector = ShadowMap.BackUpLightDirection;
                _terraEffect.CBPerDraw.Values.UseShadowMap = ClientSettings.Current.Settings.GraphicalParameters.ShadowMap;
                _terraEffect.CBPerDraw.IsDirty = true;
                _terraEffect.Apply(context);

                chunk.Graphics.DrawSolidFaces(context);

                _chunkDrawByFrame++;
            }
        }

        //Default Liquid Drawing
        private void DefaultDrawLiquid(DeviceContext context)
        {
            Matrix worldFocus = Matrix.Identity;

            _liquidEffect.Begin(context);

            foreach (var chunk in ChunksToDraw())
            {
                //Only If I have something to draw !
                if (chunk.Graphics.LiquidCubeVB != null)
                {
                    _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                    _liquidEffect.CBPerDraw.Values.PopUpValue = chunk.PopUpValue.ValueInterp;

                    switch (ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog)
                    {
                        case "SkyFog":
                            _liquidEffect.CBPerDraw.Values.FogType = 0.0f;
                            break;
                        case "SimpleFog":
                            _liquidEffect.CBPerDraw.Values.FogType = 1.0f;
                            break;
                        case "NoFog":
                        default:
                            _liquidEffect.CBPerDraw.Values.FogType = 2.0f;
                            break;
                    }

                    _liquidEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                    _liquidEffect.CBPerDraw.IsDirty = true;
                    _liquidEffect.Apply(context);
                    chunk.Graphics.DrawLiquidFaces(context);
                }   
            }
        }

        private void DrawStaticEntities(DeviceContext context)
        {
            _staticEntityDrawTime = 0;
            _staticEntityDrawCalls = 0;

            if (DrawStaticInstanced)
            {
                _voxelModelInstancedEffect.Begin(context);
                _voxelModelInstancedEffect.CBPerFrame.Values.LightDirection = Skydome.LightDirection;
                _voxelModelInstancedEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelModelInstancedEffect.CBPerFrame.IsDirty = true;
            }
            else
            {
                _voxelModelEffect.Begin(context);
                _voxelModelEffect.CBPerFrame.Values.LightDirection = Skydome.LightDirection;
                _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelModelEffect.CBPerFrame.IsDirty = true;
            }

            foreach (var chunk in ChunksToDraw())
            {
                if (chunk.DistanceFromPlayer > StaticEntityViewRange) 
                    continue;
                
                foreach (var pair in chunk.AllPairs())
                {
                    // update instances data
                    foreach (var staticEntity in pair.Value)
                    {
                        //The staticEntity.Color is affected at entity creation time in the LightingManager.PropagateLightInsideStaticEntities(...)
                        var sunPart = (float)staticEntity.BlockLight.A / 255;
                        var sunColor = Skydome.SunColor * sunPart;
                        var resultColor = Color3.Max(staticEntity.BlockLight.ToColor3(), sunColor);
                        staticEntity.VoxelEntity.ModelInstance.LightColor = resultColor;

                        if (!DrawStaticInstanced)
                        {
                            if (IsEntityVisible(staticEntity.Entity.Position))
                            {
                                var sw = Stopwatch.StartNew();
                                staticEntity.VisualVoxelModel.Draw(context, _voxelModelEffect, staticEntity.VoxelEntity.ModelInstance);
                                sw.Stop();
                                _staticEntityDrawTime += sw.Elapsed.TotalMilliseconds;
                                _staticEntityDrawCalls++;
                            }
                        }
                    }

                    if (DrawStaticInstanced)
                    {
                        if (pair.Value.Count == 0) continue;
                        var entity = pair.Value.First();
                        var sw = Stopwatch.StartNew();
                        entity.VisualVoxelModel.DrawInstanced(context, _voxelModelInstancedEffect, pair.Value.Where(ve => IsEntityVisible(ve.Entity.Position)).Select(ve => ve.VoxelEntity.ModelInstance).ToList());
                        sw.Stop();
                        _staticEntityDrawTime += sw.Elapsed.TotalMilliseconds;
                        _staticEntityDrawCalls++;
                    }
                }
            }
        }

        /// <summary>
        /// Allows to load world effects and textures or to reload textures
        /// </summary>
        /// <param name="context"></param>
        public void InitDrawComponents(DeviceContext context)
        {
            if (this.IsInitialized)
            {
                UnloadDrawComponents();
            }

            //Create Biomes Colors texture Array
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"BiomesColors/", @"*.png", FilterFlags.Point, "BiomesColors_WorldChunk", out _biomesColors_View, SharpDX.DXGI.Format.BC1_UNorm);
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", TexturePackConfig.Current.Settings.enuSamplingFilter, "ArrayTexture_WorldChunk", out _terra_View);
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"AnimatedTextures/", @"*.png", FilterFlags.Point, "ArrayTexture_AnimatedTextures", out _textureAnimation_View, SharpDX.DXGI.Format.BC4_UNorm);
            

            _terraEffect = new HLSLTerran(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration, SharedFrameCb.CBPerFrame);
            _terraEffect.TerraTexture.Value = _terra_View;
            _terraEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(TexturePackConfig.Current.Settings.enuTexMipCreationFilteringId);
            _terraEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
            _terraEffect.BiomesColors.Value = _biomesColors_View;
            _terraEffect.SkyBackBuffer.Value = _skyBackBuffer.BackBuffer;

            _liquidEffect = new HLSLLiquid(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration, SharedFrameCb.CBPerFrame);
            _liquidEffect.TerraTexture.Value = _terra_View;
            _liquidEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(TexturePackConfig.Current.Settings.enuTexMipCreationFilteringId);
            _liquidEffect.SamplerOverlay.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);
            _liquidEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
            _liquidEffect.BiomesColors.Value = _biomesColors_View;
            _liquidEffect.AnimatedTextures.Value = _textureAnimation_View;
            _liquidEffect.SkyBackBuffer.Value = _skyBackBuffer.BackBuffer;

            _voxelModelEffect = ToDispose(new HLSLVoxelModel(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
            _voxelModelInstancedEffect = ToDispose(new HLSLVoxelModelInstanced(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VoxelInstanceData.VertexDeclaration));
        }

        private void _skyBackBuffer_OnStaggingBackBufferChanged(ShaderResourceView newStaggingBackBuffer)
        {
            //Assign newly created StaggingBuffer
            _terraEffect.SkyBackBuffer.Value = newStaggingBackBuffer;
            _liquidEffect.SkyBackBuffer.Value = newStaggingBackBuffer;
        }

        private void UnloadDrawComponents()
        {
            DisposeDrawComponents();
        }

        private void DisposeDrawComponents()
        {
            _terra_View.Dispose();
            _liquidEffect.Dispose();
            _terraEffect.Dispose();
            _biomesColors_View.Dispose();
            _voxelModelEffect.Dispose();
            _voxelModelInstancedEffect.Dispose();
            _textureAnimation_View.Dispose();
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
                if (SortedChunks[chunkIndice].Graphics.SolidCubeIB == null) continue;
                if (SortedChunks[chunkIndice].Graphics.NeedToRender)
                {
                    VprimitiveCount += SortedChunks[chunkIndice].Graphics.SolidCubeIB.IndicesCount;
                    if (SortedChunks[chunkIndice].Graphics.LiquidCubeIB != null) VprimitiveCount += (SortedChunks[chunkIndice].Graphics.LiquidCubeIB.IndicesCount);
                }
                BprimitiveCount += SortedChunks[chunkIndice].Graphics.SolidCubeIB.IndicesCount;
                if (SortedChunks[chunkIndice].Graphics.LiquidCubeIB != null) BprimitiveCount += (SortedChunks[chunkIndice].Graphics.LiquidCubeIB.IndicesCount);
            }
            return string.Concat("<TerraCube Mod> BChunks : ", VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y, "; BPrim : ", BprimitiveCount, " DChunks : ", _chunkDrawByFrame, " DPrim : ", VprimitiveCount);
        }
        #endregion
    }
}
