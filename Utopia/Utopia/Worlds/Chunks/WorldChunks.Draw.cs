using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        #region Public methods

        private void ChunkVisibilityTest()
        {
            foreach (VisualChunk chunk in SortedChunks)
            {
                chunk.isFrustumCulled = !_camManager.ActiveCamera.Frustum.IntersectsWithoutFar(ref chunk.ChunkWorldBoundingBox);
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
                if (!_playerManager.IsHeadInsideWater)
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
                if (chunk.isExistingMesh4Drawing && !chunk.isFrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
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

                if (chunk.isExistingMesh4Drawing)
                {
                    if (!chunk.isFrustumCulled)
                    {
                        _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                        _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _terraEffect.CBPerDraw.Values.PopUpValue = chunk.PopUpValue.ValueInterp;
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

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.isExistingMesh4Drawing && !chunk.isFrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    //Only If I have something to draw !
                    if (chunk.LiquidCubeVB != null)
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
                        chunk.DrawLiquidFaces(context);
                    }
                }
            }
        }

        private void DrawStaticEntities(DeviceContext context)
        {
            VisualChunk chunk;

            _staticEntityDrawTime = 0;
            _staticEntityDrawCalls = 0;

            if (DrawStaticInstanced)
            {
                _voxelModelInstancedEffect.Begin(context);
                _voxelModelInstancedEffect.CBPerFrame.Values.LightDirection = _skydome.LightDirection;
                _voxelModelInstancedEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelModelInstancedEffect.CBPerFrame.IsDirty = true;
            }
            else
            {
                _voxelModelEffect.Begin(context);
                _voxelModelEffect.CBPerFrame.Values.LightDirection = _skydome.LightDirection;
                _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _voxelModelEffect.CBPerFrame.IsDirty = true;
            }

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.DistanceFromPlayer > StaticEntityViewRange) continue;

                if (chunk.isExistingMesh4Drawing)
                {
                    if (!chunk.isFrustumCulled)
                    {
                        //For each Voxel Items in the chunk
                        foreach (var pair in chunk.VisualVoxelEntities)
                        {
                            // update instances data
                            foreach (var staticEntity in pair.Value)
                            {
                                //The staticEntity.Color is affected at entity creation time in the LightingManager.PropagateLightInsideStaticEntities(...)
                                var sunPart = (float)staticEntity.BlockLight.A / 255;
                                var sunColor = _skydome.SunColor * sunPart;
                                var resultColor = Color3.Max(staticEntity.BlockLight.ToColor3(), sunColor);
                                staticEntity.VoxelEntity.ModelInstance.LightColor = resultColor;

                                if (!DrawStaticInstanced)
                                {
                                    var sw = Stopwatch.StartNew();
                                    staticEntity.VisualVoxelModel.Draw(context, _voxelModelEffect, staticEntity.VoxelEntity.ModelInstance);
                                    sw.Stop();
                                    _staticEntityDrawTime += sw.Elapsed.TotalMilliseconds;
                                    _staticEntityDrawCalls++;
                                }
                            }

                            if (DrawStaticInstanced)
                            {
                                if (pair.Value.Count == 0) continue;
                                var entity = pair.Value.First();
                                var sw = Stopwatch.StartNew();
                                entity.VisualVoxelModel.DrawInstanced(context, _voxelModelInstancedEffect, pair.Value.Select(ve => ve.VoxelEntity.ModelInstance).ToList());
                                sw.Stop();
                                _staticEntityDrawTime += sw.Elapsed.TotalMilliseconds;
                                _staticEntityDrawCalls++;
                            }
                        }
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

            _terraEffect = new HLSLTerran(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration, _sharedFrameCB.CBPerFrame);
            _terraEffect.TerraTexture.Value = _terra_View;
            _terraEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(TexturePackConfig.Current.Settings.enuTexMipCreationFilteringId);
            _terraEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
            _terraEffect.BiomesColors.Value = _biomesColors_View;
            _terraEffect.SkyBackBuffer.Value = _skyBackBuffer.BackBuffer;

            _liquidEffect = new HLSLLiquid(_d3dEngine.Device, ClientSettings.EffectPack + @"Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration, _sharedFrameCB.CBPerFrame);
            _liquidEffect.TerraTexture.Value = _terra_View;
            _liquidEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(TexturePackConfig.Current.Settings.enuTexMipCreationFilteringId);
            _liquidEffect.SamplerOverlay.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);
            _liquidEffect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
            _liquidEffect.BiomesColors.Value = _biomesColors_View;
            _liquidEffect.AnimatedTextures.Value = _textureAnimation_View;
            _liquidEffect.SkyBackBuffer.Value = _skyBackBuffer.BackBuffer;

            _voxelModelEffect = ToDispose(new HLSLVoxelModel(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
            _voxelModelInstancedEffect = ToDispose(new HLSLVoxelModelInstanced(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VertexVoxelInstanced.VertexDeclaration));
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
            //_spriteTexture_View.Dispose();
            //_staticSpriteEffect.Dispose();
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
