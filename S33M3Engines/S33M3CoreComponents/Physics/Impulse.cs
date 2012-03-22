using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Physics
{
    public class Impulse
    {
        private Vector3D _forceApplied;
        private float _amountOfTime = 0.00001f;
        private GameTime _timeStep;

        public bool IsActive
        {
            get { return _amountOfTime > 0; }
        }

        public Impulse(ref GameTime timeStep)
        {
            _timeStep = timeStep;
        }

        public Vector3D ForceApplied
        {
            get
            {
                return _amountOfTime != 0.00001 ? _forceApplied / (_timeStep.ElapsedGameTimeInS_HD) : _forceApplied;
            }
            set { _forceApplied = value; }
        }

        public float AmountOfTime
        {
            get { return _amountOfTime; }
            set { _amountOfTime = value > 0 ? value : 0; }
        }
    }
}
