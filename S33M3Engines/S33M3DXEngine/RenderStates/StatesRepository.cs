using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.RenderStates
{
    //This static class will hold a collection of predefined and various states, ready to be used !
    public static class RenderStatesRepo
    {
        private static RasterizerState[] _rasterStates;
        private static BlendState[] _blendStates;
        private static DepthStencilState[] _depthStencilStates;
        private static SamplerState[] _samplerStates;
        private static D3DEngine _engine;

        public static void Initialize(D3DEngine engine)
        {
            _engine = engine;
            _rasterStates = new RasterizerState[0];
            _blendStates = new BlendState[0];
            _depthStencilStates = new DepthStencilState[0];
            _samplerStates = new SamplerState[0];
        }

        public static void Dispose()
        {
            _engine = null;
            if (_rasterStates != null) foreach (RasterizerState obj in _rasterStates.Where(x => x != null)) obj.Dispose();
            if (_blendStates != null) foreach (BlendState obj in _blendStates.Where(x => x != null)) obj.Dispose();
            if (_depthStencilStates != null) foreach (DepthStencilState obj in _depthStencilStates.Where(x => x != null)) obj.Dispose();
            if (_samplerStates != null) foreach (SamplerState obj in _samplerStates.Where(x => x != null)) obj.Dispose();
        }

        //Raster States Management ======================================================================
        private static int _rasterApplied = -1;
        public static int AddRasterStates(RasterizerStateDescription RasterDescr)
        {
            Array.Resize<RasterizerState>(ref _rasterStates, _rasterStates.Length + 1);
            _rasterStates[_rasterStates.Length - 1] = new RasterizerState(_engine.Device, RasterDescr);
            return _rasterStates.Length - 1;
        }

        public static void ApplyRaster(int id, DeviceContext context)
        {
            if (id == _rasterApplied) return;
            _rasterApplied = id;
            context.Rasterizer.State = _rasterStates[id];
        }

        //Blend States Management ======================================================================
        private static int _blendApplied = -1;
        public static int AddBlendStates(BlendStateDescription BlendStateDescr)
        {
            Array.Resize<BlendState>(ref _blendStates, _blendStates.Length + 1);
            _blendStates[_blendStates.Length - 1] = new BlendState(_engine.Device, BlendStateDescr);
            return _blendStates.Length - 1;
        }

        public static void ApplyBlend(int id, DeviceContext context)
        {
            if (id == _blendApplied) return;
            _blendApplied = id;
            context.OutputMerger.BlendState = _blendStates[id];
        }

        //Blend States Management ==========================================================================
        private static int _depthStencilApplied = -1;
        public static int AddDepthStencilStates(DepthStencilStateDescription DepthStencilStateDescr)
        {
            Array.Resize<DepthStencilState>(ref _depthStencilStates, _depthStencilStates.Length + 1);
            _depthStencilStates[_depthStencilStates.Length - 1] = new DepthStencilState(_engine.Device, DepthStencilStateDescr);
            return _depthStencilStates.Length - 1;
        }

        public static void ApplyDepthStencil(int id, DeviceContext context)
        {
            if (id == _depthStencilApplied) return;
            _depthStencilApplied = id;
            context.OutputMerger.DepthStencilState = _depthStencilStates[id];
        }

        public static int AddSamplerStates(SamplerStateDescription SamplerDescr)
        {
            Array.Resize<SamplerState>(ref _samplerStates, _samplerStates.Length + 1);
            _samplerStates[_samplerStates.Length - 1] = new SamplerState(_engine.Device, SamplerDescr);
            return _samplerStates.Length - 1;
        }

        public static SamplerState GetSamplerState(int id)
        {
            return _samplerStates[id];
        }

        public static void ApplyStates(DeviceContext context, int RasterId = -1, int BlendId = -1, int DepthId = -1 )
        {
            if (RasterId != -1) ApplyRaster(RasterId, context);
            if (BlendId != -1) ApplyBlend(BlendId, context);
            if (DepthId != -1) ApplyDepthStencil(DepthId, context);
        }

    }
}
