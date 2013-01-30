﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LtreeVisualizer.DataPipe;
using S33M3DXEngine.Main;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.LandscapeEntities.Trees;
using LtreeVisualizer.Components;
using S33M3CoreComponents.Cameras;
using SharpDX;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;

namespace LtreeVisualizer
{
    public class LtreeRender : Game
    {
        private GameComponent _gamecomp;
        private GameComponent _cameraMnger;
        private GameComponent _cameraEntity;

        private InputsManager _inputManager;
        private ICamera _camera;

        public LtreeRender(Size startingWindowsSize, string WindowsCaption, Size ResolutionSize = default(Size))
            : base(startingWindowsSize, WindowsCaption, new SharpDX.DXGI.SampleDescription(1, 0), ResolutionSize)
        {
            DXStates.CreateStates(Engine);

            _camera = new FirstPersonCamera(base.Engine, 0.5f, 100.0f);

            _inputManager = new InputsManager(base.Engine, typeof(Actions));
            _inputManager.MouseManager.IsRunning = true;
            _inputManager.EnableComponent();

            _cameraMnger = ToDispose(new CameraManager<ICamera>(_inputManager, null));
            ((CameraManager<ICamera>)_cameraMnger).RegisterNewCamera(_camera);
            _cameraMnger.EnableComponent();

            _cameraEntity = ToDispose(new Entity(new S33M3Resources.Structs.Vector3D(0, 0, -20), Quaternion.RotationAxis(Vector3.UnitY, 0.0f)));
            _cameraEntity.EnableComponent();

            _camera.CameraPlugin = (ICameraPlugin)_cameraEntity;

            _gamecomp = new LTreeVisu((CameraManager<ICamera>)_cameraMnger);
            _gamecomp.EnableComponent();

            //Register Here all components
            base.GameComponents.Add(_cameraMnger);
            base.GameComponents.Add(_gamecomp);
            base.GameComponents.Add(_cameraEntity);
        }

        Thread _dataPipeThread;
        Pipe _dataPipe = new Pipe();
        public override void Initialize()
        {
            _dataPipeThread = new Thread(_dataPipe.Start);
            _dataPipeThread.Start();

            this.Engine.GameWindow.KeyUp += new KeyEventHandler(GameWindow_KeyUp);
            base.Initialize();
        }

        void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                this.Engine.isFullScreen = !this.Engine.isFullScreen;
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            Pipe.StopThread = true;
            Pipe.PipeStream.Close();
            base.Dispose(disposeManagedResources);
        }
    }
}
