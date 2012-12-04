using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.Particules
{
    public class ParticuleEngine : DrawableGameComponent
    {
        #region Private Variables
        public List<IEmitter> _liveEmitter;
        #endregion

        #region Public Properties
        #endregion

        public ParticuleEngine()
        {
            Initialize();
        }

        #region Public Methods
        public override void Initialize()
        {
            _liveEmitter = new List<IEmitter>();
        }

        public override void Update(GameTime timeSpent)
        {
            foreach (var emitter in _liveEmitter) emitter.Update(timeSpent);
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            foreach (var emitter in _liveEmitter) emitter.Interpolation(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            foreach (var emitter in _liveEmitter) emitter.Draw(context, index);
        }


        public void InsertEmitter(IEmitter emitter)
        {
            _liveEmitter.Add(emitter);
        }
        #endregion

        #region Private Methods

        #endregion

    }
}
