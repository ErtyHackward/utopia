using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;

namespace Utopia.Components
{
    public class StaggingBackBuffer : DrawableGameComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        #endregion

        #region Public variables/properties
        public ShaderResourceView SolidStaggingBackBuffer
        {
            get
            {
                return _engine.StaggingBackBuffer;
            }
        }
        #endregion

        public StaggingBackBuffer(D3DEngine engine)
        {
            _engine = engine;
            this.DrawOrders.UpdateIndex(0, 999, "SolidBackBuffer"); //This should be call After all SOLID object have been draw on screen.
        }

        #region Public Methods
        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            _engine.SetSingleRenderTargets();
        }
        #endregion

        #region Private methods
        #endregion
    }
}
