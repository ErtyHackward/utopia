using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;

namespace S33M3Physics
{
    public class Impulse
    {
        private DVector3 _forceApplied;
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

        public DVector3 ForceApplied
        {
            get
            {
                return _amountOfTime != 0.00001 ? _forceApplied : _forceApplied * _timeStep.ElapsedGameTimeInS_HD;
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
