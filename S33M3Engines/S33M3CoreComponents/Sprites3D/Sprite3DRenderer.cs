using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites3D
{

    /// <summary>
    /// Class that will handle rendering of sprite in 3 dimension
    /// Those rendering sprite can be billboard type
    /// </summary>
    public class Sprite3DRenderer<T> : BaseComponent where T : ISprite3DProcessor
    {
        #region Private Variables
        private T _processor;
        private int _rasterStateId;
        private int _blendStateId; 
        private int _depthStateId;
        #endregion

        #region Public Properties
        public T Processor { get { return _processor; } }
        #endregion

        public Sprite3DRenderer(T processor,
                                int RasterStateId,
                                int BlendStateId,
                                int DepthStateId,
                                DeviceContext context
                                )
        {
            _rasterStateId = RasterStateId;
            _blendStateId = BlendStateId;
            _depthStateId = DepthStateId;
            _processor = processor;

            Initialize(context);
        }

        #region Public Methods
        public void Begin(DeviceContext context, bool ApplyRenderStates = true)
        {
            _processor.Begin();   //Call processor Begin();
            if (ApplyRenderStates) SetRenderStates(context);
        }

        public void ReplayLast(DeviceContext context, 
                               bool ApplyRenderStates = true)
        {
            if (ApplyRenderStates) SetRenderStates(context);
            End(context);
        }

        public void End(DeviceContext context)
        {
            _processor.SetData(context); //Send the accumulated buffer to the GC ==> "Only" if Collection are "dirty"
            _processor.Set2DeviceAndDraw(context);
        }

        #endregion

        #region Private Methods
        private void Initialize(DeviceContext context)
        {
            _processor.Init(context, ResourceUsage.Dynamic);
        }

        private void SetRenderStates(DeviceContext context)
        {
            RenderStatesRepo.ApplyStates(context, _rasterStateId, _blendStateId, _depthStateId);
        }
        #endregion
    }
}
