using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Physics
{
    public class Impulse
    {
        private static FastRandom _fstRnd = new FastRandom();

        private Vector3 _forceApplied;
        private float _amountOfTime = 0.00001f;
        private float _elapsedTime;
        private int privateHashCode;

        public bool IsActive
        {
            get { return _amountOfTime > 0; }
        }

        public Impulse(float elapsedTime)
        {
            _elapsedTime = elapsedTime;
        }

        public bool ApplyOnlyIfOnGround { get; set; }
        /// <summary>
        /// When used, the system will remove any other impulse with the same ID added during a physic cycle. (This impulse with be only processed once)
        /// </summary>
        private string _impulseId;
        public string ImpulseId
        {
            get { return _impulseId; }
            set { _impulseId = value; privateHashCode = value.GetHashCode(); }
        }


        public Impulse()
        {
            privateHashCode = _fstRnd.NextInt();
        }

        public Vector3 ForceApplied
        {
            get
            {
                if (_elapsedTime != 0f)
                {
                    return _amountOfTime != 0.00001 ? _forceApplied / (_elapsedTime) : _forceApplied;
                }
                else
                {
                    return _amountOfTime != 0.00001 ? _forceApplied / 0.025f : _forceApplied;
                }
            }
            set { _forceApplied = value; }
        }

        public float AmountOfTime
        {
            get { return _amountOfTime; }
            set { _amountOfTime = value > 0 ? value : 0; }
        }

        public override int GetHashCode()
        {
            return privateHashCode;
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
    }
}
