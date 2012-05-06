using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3DXEngine.Threading;
using Samples.GameComp;
using System.Drawing;
using System.Windows.Forms;
using S33M3CoreComponents.Maths.Noises;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using Samples.Entities;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using Samples.RenderStates;
using S33M3CoreComponents.Noise;

namespace Samples
{
    public class GameRender : Game
    {
        private NoiseRender _gameComp;
        private InputsManager _inputManager;
        private GameComponent _sharedFrameComp;
        private INoise3 _noise;
        private GameComponent _cameraMnger;
        private GameComponent _myCameraEntity;
        private ICamera _camera;

        public GameRender(Size startingWindowsSize, string WindowsCaption, INoise3 noise, Size ResolutionSize = default(Size))
            : base(startingWindowsSize, WindowsCaption, new SharpDX.DXGI.SampleDescription(1,0), ResolutionSize)
        {
            _noise = noise;
        }

        public override void Initialize()
        {
            DXRenderStates.CreateStates(Engine);

            S33M3DXEngine.Threading.SmartThread.SetOptimumNbrThread(2, true);

            //Create the camera, and the camera manager
            _camera = ToDispose(new FirstPersonCamera(base.Engine, 0.5f, 1000.0f));
            _cameraMnger = ToDispose(new CameraManager<ICamera>(_camera));
            _cameraMnger.EnableComponent();

            _inputManager = new InputsManager(base.Engine, typeof(Actions));
            _inputManager.MouseManager.IsRunning = true;
            _inputManager.EnableComponent();

            _myCameraEntity = ToDispose(new Entity(this.Engine, _cameraMnger as CameraManager<ICamera>, _inputManager));
            _myCameraEntity.EnableComponent();

            _camera.CameraPlugin = (ICameraPlugin)_myCameraEntity;

            //Create the various components
            _sharedFrameComp = ToDispose(new SharedFrameCB(base.Engine, (CameraManager<ICamera>)_cameraMnger));
            _sharedFrameComp.EnableComponent();

            //Register Here all components here
            _gameComp = ToDispose(new NoiseRender(Engine, _noise, _sharedFrameComp as SharedFrameCB, _camera));
            _gameComp.EnableComponent();

            this.GameComponents.Add(_inputManager);
            this.GameComponents.Add(_myCameraEntity);
            this.GameComponents.Add(_cameraMnger);
            this.GameComponents.Add(_sharedFrameComp);
            this.GameComponents.Add(_gameComp);
            this.Engine.GameWindow.KeyUp += new KeyEventHandler(GameWindow_KeyUp);
            base.Initialize();
        }

        public override void AfterDispose()
        {
            DXRenderStates.Dispose();
        }

        void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                this.Engine.isFullScreen = !this.Engine.isFullScreen;
            }
        }
    }
}
