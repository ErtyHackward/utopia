using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.Physics.Verlet
{
    public class VerletSimulator
    {
        private Vector3D _curPosition;

        private Vector3D _prevPosition;

        private Vector3D _forcesAccum;

        private BoundingBox _localBoundingBox;
        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        bool _subjectToGravity = true;
        bool _withCollisionBounsing = false;
        bool _onGround;

        private List<Impulse> _impulses = new List<Impulse>();

        public List<Impulse> Impulses { get { return _impulses; } }
        public bool WithCollisionBouncing { get { return _withCollisionBounsing; } set { _withCollisionBounsing = value; } }
        public bool SubjectToGravity { get { return _subjectToGravity; } set { _subjectToGravity = value; } }
        public bool OnGround { get { return _onGround; } set { _onGround = value; } }

        public Vector3D CurPosition { get { return _curPosition; } set { _curPosition = value; } }
        public Vector3D PrevPosition { get { return _prevPosition; } set { _prevPosition = value; } }

        public delegate void CheckConstraintFct(VerletSimulator physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);
        public CheckConstraintFct ConstraintFct;

        public VerletSimulator(ref BoundingBox localBoundingBox)
        {
            _localBoundingBox = localBoundingBox;
        }

        public void StartSimulation(ref Vector3D StartingPosition, ref Vector3D PreviousPosition)
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

        public void Simulate(ref GameTime dt, out Vector3D newPosition)
        {
            if (_isRunning)
            {
                AccumulateForce(ref dt);                                //Add the force currently applied
                Verlet(ref dt, out newPosition);                        //Compute the next location based taken into account the accumulated force, the time , ...
                SatisfyConstraints(ref newPosition, ref _prevPosition); //Validate the new location based in constraint (Collision, ...)
            }
            else
            {
                newPosition = Vector3D.Zero;
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

        private void Verlet(ref GameTime dt, out Vector3D newPosition)
        {
            newPosition = _curPosition + _curPosition - _prevPosition + (_forcesAccum * dt.ElapsedGameTimeInS_HD * dt.ElapsedGameTimeInS_HD);
            _prevPosition = _curPosition;
            _curPosition = newPosition;
        }

        private void SatisfyConstraints(ref Vector3D futurePosition, ref Vector3D OriginalPosition)
        {
            foreach (CheckConstraintFct fct in ConstraintFct.GetInvocationList())
            {
                //This fct will be able to modify the newPosition if needed to satisfy its own constraint !
                fct(this, ref _localBoundingBox, ref futurePosition, ref OriginalPosition);
            }
            _curPosition = futurePosition;
        }
    }
}
