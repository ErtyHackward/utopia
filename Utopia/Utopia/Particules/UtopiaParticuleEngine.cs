using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Particules;
using S33M3DXEngine;
using Utopia.Components;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine.Textures;
using Utopia.Shared.Settings;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;

namespace Utopia.Particules
{
    public class UtopiaParticuleEngine : ParticuleEngine
    {
        private SharedFrameCB _sharedFrameCB;
        private CameraManager<ICameraFocused> _cameraManager;
        private InputsManager _inputsManager;

        private ShaderResourceView _particules;
        private IEmitter _testEmitter;

        public override S33M3Resources.Structs.Vector3D CameraPosition
        {
            get { return _cameraManager.ActiveCamera.WorldPosition.ValueInterp; }
        }

        public UtopiaParticuleEngine(D3DEngine d3dEngine, 
                                     SharedFrameCB sharedFrameCB,
                                     CameraManager<ICameraFocused> cameraManager,
                                     InputsManager inputsManager)
            :base(d3dEngine, sharedFrameCB.CBPerFrame)
        {
            _sharedFrameCB = sharedFrameCB;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
        }

        public override void LoadContent(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(base._d3dEngine.Device, base._d3dEngine.ImmediateContext, ClientSettings.TexturePack + @"Particules/", @"*.png", FilterFlags.Point, "ArrayTexture_Particules", out _particules);
            ToDispose(_particules);
        }

        bool emitterCreated = false;
        public override void Update(S33M3DXEngine.Main.GameTime timeSpent)
        {
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Right, false))
            {
                if (!emitterCreated)
                {
                    _testEmitter = new Emitter(this._cameraManager.ActiveCamera.WorldPosition.Value,
                       new Vector3(0, 8, 0),
                       new Vector2(10, 10),
                       5.0f,
                       Emitter.ParticuleGenerationMode.Manual,
                       new Vector3(5, 3, 5),
                       new Vector3D(0, -9.8, 0),
                       RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear),
                       _particules,
                       DXStates.Rasters.Default,
                       DXStates.Blenders.Enabled,
                       DXStates.DepthStencils.DepthReadEnabled);

                    AddEmitter(_testEmitter);

                    emitterCreated = true;
                }
                _testEmitter.EmmitParticule(10);
            }

            base.Update(timeSpent);
        }


    }
}
