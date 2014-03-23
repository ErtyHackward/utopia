using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        protected List<IEmitter> _liveEmitter;
        protected iCBuffer _sharedFrameBuffer;
        protected D3DEngine _d3dEngine;
        #endregion

        #region Public Properties
        public abstract Vector3D CameraPosition { get; }
        #endregion

        public ParticuleEngine(D3DEngine d3dEngine, 
                               iCBuffer sharedFrameBuffer)
        {
            _d3dEngine = d3dEngine;
            _sharedFrameBuffer = sharedFrameBuffer;
            _liveEmitter = new List<IEmitter>();

            DrawOrders.UpdateIndex(0, 1059, "ParticuleEngine");
        }

        #region Public Methods
        public override void Initialize()
        {
        }

        public override void LoadContent(DeviceContext context)
        {
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            //Remove stopped Emitters
            _liveEmitter.RemoveAll(x => x.isStopped);

            //Update live emitters
            foreach (var emitter in _liveEmitter) emitter.FTSUpdate(timeSpent);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            foreach (var emitter in _liveEmitter) emitter.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            foreach (var emitter in _liveEmitter) emitter.Draw(context, index);
        }

        protected void AddEmitter(DeviceContext context, IEmitter emitter)
        {
            //bind the Emitter with this PArticuleEngine for rendering
            emitter.ParentParticuleEngine = this;
            emitter.Initialize(context, _sharedFrameBuffer);
            
            _liveEmitter.Add(emitter);
        }
        #endregion

        #region Private Methods

        #endregion

    }
}
