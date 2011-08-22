using System;
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

        public void Draw()
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _chunkDrawByFrame = 0;

            _terraEffect.Begin();
            _terraEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _terraEffect.CBPerFrame.Values.dayTime = _gameClock.ClockTime.ClockTimeNormalized2;
            _terraEffect.CBPerFrame.Values.fogdist = ((VisibleWorldSize.X) / 2) - 48;
            _terraEffect.CBPerFrame.IsDirty = true;
            _sunColorBase = GetSunColor();

            if (_player.HeadInsideWater) _terraEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase / 3, _sunColorBase / 3, _sunColorBase);
            else _terraEffect.CBPerFrame.Values.SunColor = new Vector3(_sunColorBase, _sunColorBase, _sunColorBase);

            DrawSolidFaces();

        }
        #endregion

        #region Private methods
        private void DrawSolidFaces()
        {
            VisualChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            //Foreach faces type
            for (int chunkIndice = 0; chunkIndice < _worldParameters.WorldSize.X * _worldParameters.WorldSize.Z; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.Ready2Draw)
                {
                    //Only checking Frustum with the faceID = 0
                    chunk.isFrustumCulled = !_camManager.ActiveCamera.Frustum.Intersects(chunk.ChunkWorldBoundingBox);

                    if (!chunk.isFrustumCulled)
                    {
                        _worldFocusManager.CenterOnFocus(ref chunk.World, ref worldFocus);
                        _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _terraEffect.CBPerDraw.Values.popUpYOffset = 0;
                        _terraEffect.CBPerDraw.IsDirty = true;
                        _terraEffect.Apply();

                        lock (chunk.Lock_DrawChunksSolidFaces)
                        {
                            if (chunk.SolidCubeVB != null)
                            {
                                chunk.SolidCubeVB.SetToDevice(0);
                                chunk.SolidCubeIB.SetToDevice(0);
                                _d3dEngine.Context.DrawIndexed(chunk.SolidCubeIB.IndicesCount, 0, 0);
                            }
                        }

                        _chunkDrawByFrame++;
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


        private void InitDrawComponents()
        {
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, out _terra_View);

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
        


    }
}
