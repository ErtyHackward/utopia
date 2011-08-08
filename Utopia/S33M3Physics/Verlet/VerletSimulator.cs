using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.D3D;
using S33M3Engines.Maths;

namespace S33M3Physics.Verlet
{
    public class VerletSimulator
    {
        private DVector3 _curPosition;

        private DVector3 _prevPosition;

        private DVector3 _forcesAccum;

        private BoundingBox _boundingBox;
        private bool _isRunning;

        bool _subjectToGravity = true;
        bool _withCollisionBounsing = false;
        bool _onGround;


        private List<Impulse> _impulses = new List<Impulse>();

        public List<Impulse> Impulses { get { return _impulses; } }
        public bool WithCollisionBounsing { get { return _withCollisionBounsing; } set { _withCollisionBounsing = value; } }
        public bool SubjectToGravity { get { return _subjectToGravity; } set { _subjectToGravity = value; } }
        public bool OnGround { get { return _onGround; } set { _onGround = value; } }

        public DVector3 CurPosition { get { return _curPosition; } set { _curPosition = value; } }
        public DVector3 PrevPosition { get { return _prevPosition; } set { _prevPosition = value; } }

        public delegate void CheckConstraintFct(ref DVector3 newPosition2Evaluate, ref DVector3 previousPosition);
        public CheckConstraintFct ConstraintFct;

        public VerletSimulator(ref BoundingBox boundingBox)
        {
            _boundingBox = boundingBox;
        }

        public void StartSimulation(ref DVector3 StartingPosition, ref DVector3 PreviousPosition)
        {
            _isRunning = true;
            _prevPosition = PreviousPosition;
            _curPosition = StartingPosition;
        }

        public void StopSimulation()
        {
            _isRunning = false;
        }

        public void Freeze(bool X, bool Y, bool Z)
        {
            if (X) _prevPosition.X = _curPosition.X;
            if (Y) _prevPosition.Y = _curPosition.Y;
            if (Z) _prevPosition.Z = _curPosition.Z;
        }

        public void Simulate(ref GameTime dt, out DVector3 newPosition)
        {
            if (_isRunning)
            {
                AccumulateForce(ref dt);
                Verlet(ref dt, out newPosition);
                SatisfyConstraints(ref newPosition, ref _prevPosition);
            }
            else
            {
                newPosition = DVector3.Zero;
            }
        }

        private void AccumulateForce(ref GameTime dt)
        {
            _forcesAccum.X = 0;
            _forcesAccum.Y = 0;
            _forcesAccum.Z = 0;

            if (_subjectToGravity && !_onGround) 
                _forcesAccum.Y += -SimulatorCst.Gravity;

            for (int ImpulseIndex = 0; ImpulseIndex < _impulses.Count; ImpulseIndex++)
            {
                if (_impulses[ImpulseIndex].IsActive)
                {
                    _forcesAccum += _impulses[ImpulseIndex].ForceApplied;
                    _impulses[ImpulseIndex].AmountOfTime -= dt.ElapsedGameTimeInS_LD;
                }
            }

            //CleanUp impulses
            _impulses.RemoveAll(x => x.IsActive == false);

        }

        private void Verlet(ref GameTime dt, out DVector3 newPosition)
        {
            newPosition = _curPosition + _curPosition - _prevPosition + (_forcesAccum * dt.ElapsedGameTimeInS_HD * dt.ElapsedGameTimeInS_HD);
            _prevPosition = _curPosition;
            _curPosition = newPosition;
        }

        private void SatisfyConstraints(ref DVector3 newPosition, ref DVector3 previousPosition)
        {
            foreach (CheckConstraintFct fct in ConstraintFct.GetInvocationList())
            {
                //This fct will be able to modify the newPosition if needed to satisfy its own constraint !
                fct(ref newPosition, ref previousPosition);
            }

            _curPosition = newPosition;
           
        }
    }
}
