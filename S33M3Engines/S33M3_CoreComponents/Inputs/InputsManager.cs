using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_DXEngine;
using SharpDX.Direct3D11;
using S33M3_Resources.Structs;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Inputs.MouseHandler;
using S33M3_CoreComponents.Inputs.KeyboardHandler;
using S33M3_CoreComponents.Cameras.Interfaces;

namespace S33M3_CoreComponents.Inputs
{
    /// <summary>
    /// Class responsible to give acces to data forwarded by various Inputs.
    /// Mouse handling
    /// Keyboard Event model (Events) => Ideal for use text input
    /// Keyboard & Mouse Action model (Pooling) => Ideal to check the state of a check and react accordingly
    /// </summary>
    public class InputsManager : GameComponent
    {
        #region Private variables
        private D3DEngine _engine;
        #endregion

        #region Public variables/Properties
        public readonly ActionsManager ActionsManager;
        public readonly MouseManager MouseManager;
        public readonly KeyboardManager KeyboardManager;
        #endregion

        /// <summary>
        /// Input Managing
        /// </summary>
        /// <param name="engine">The Main d3d engine</param>
        /// <param name="cameraManager">The Camera Manager</param>
        /// <param name="actionType">The type presenting the collection of Actions at disposal via Const variables</param>
        public InputsManager(D3DEngine engine, 
                            Type actionType)
        {
            _engine = engine;

            MouseManager = ToDispose(new MouseManager(_engine));
            ActionsManager = ToDispose(new ActionsManager(_engine, MouseManager, actionType));
            KeyboardManager = ToDispose(new KeyboardManager(_engine));

            this.UpdateOrder = 0;

            this.EnableComponent();
        }

        #region Public Methods

        public override void Update(GameTime timeSpent)
        {
            ActionsManager.FetchInputs();
            ActionsManager.Update();
            MouseManager.Update();
            KeyboardManager.Update();
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            ActionsManager.FetchInputs();
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
