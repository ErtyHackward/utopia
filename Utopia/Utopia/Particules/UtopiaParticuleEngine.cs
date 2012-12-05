using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Particules;
using S33M3DXEngine;
using Utopia.Components;

namespace Utopia.Particules
{
    public class UtopiaParticuleEngine : ParticuleEngine
    {
        private SharedFrameCB _sharedFrameCB;
        private CameraManager<ICameraFocused> _cameraManager;

        public override S33M3Resources.Structs.Vector3D CameraPosition
        {
            get { return _cameraManager.ActiveCamera.WorldPosition.ValueInterp; }
        }

        public UtopiaParticuleEngine(D3DEngine d3dEngine, 
                                     SharedFrameCB sharedFrameCB,
                                     CameraManager<ICameraFocused> cameraManager)
            :base(d3dEngine, sharedFrameCB.CBPerFrame)
        {
            _sharedFrameCB = sharedFrameCB;
            _cameraManager = cameraManager;
        }


    }
}
