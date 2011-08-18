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
        public ShaderResourceView _terra_View;
        #endregion

        private void InitDrawComponents()
        {
            ArrayTexture.CreateTexture2DFromFiles(_game.GraphicDevice, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, out _terra_View);

            _terraEffect = new HLSLTerran(_game, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);
            _liquidEffect = new HLSLLiquid(_game, @"Effects/Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration);

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

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Update(ref GameTime TimeSpend)
        {
            throw new NotImplementedException();
        }

        public void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            throw new NotImplementedException();
        }
    }
}
