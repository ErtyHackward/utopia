﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Particules
{
    public abstract class ParticuleEngine : DrawableGameComponent
    {
        #region Private Variables
        private List<IEmitter> _liveEmitter;
        private iCBuffer _sharedFrameBuffer;
        private D3DEngine _d3dEngine;
        #endregion

        #region Public Properties
        public abstract Vector3D CameraPosition { get; }
        #endregion

        public ParticuleEngine(D3DEngine d3dEngine, 
                               iCBuffer sharedFrameBuffer)
        {
            _d3dEngine = d3dEngine;
            _sharedFrameBuffer = sharedFrameBuffer;

            DrawOrders.UpdateIndex(0, 1059, "ParticuleEngine");
        }

        #region Public Methods
        public override void Initialize()
        {
            _liveEmitter = new List<IEmitter>();
        }

        public override void LoadContent(DeviceContext context)
        {
        }

        public override void Update(GameTime timeSpent)
        {
            //Remove stopped Emitters
            _liveEmitter.RemoveAll(x => x.isStopped);

            //Update live emitters
            foreach (var emitter in _liveEmitter) emitter.Update(CameraPosition);
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            foreach (var emitter in _liveEmitter) emitter.Interpolation(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            foreach (var emitter in _liveEmitter) emitter.Draw(context, index);
        }

        public void AddEmitter(IEmitter emitter)
        {
            //bind the Emitter with this PArticuleEngine for rendering
            emitter.ParentParticuleEngine = this;
            emitter.Initialize(_d3dEngine.ImmediateContext, _sharedFrameBuffer);

            _liveEmitter.Add(emitter);
        }
        #endregion

        #region Private Methods

        #endregion

    }
}
