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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
        public bool ConstraintOnlyMode { get; set; }
        public bool OnGround { get { return _onGround; } set { _onGround = value; } }
        public bool AllowJumping { get; set; }

        public float OnOffsettedBlock { get; set; }
        public float OffsetBlockHitted { get; set; }
        public double GroundBelowEntity { get; set; }
        public bool isInContactWithLadder { get; set; }
        public bool StopMovementAction { get; set; }

        private bool _isSliding;
        private int _slidingCycles;
        private int _stopSliddingCounter;
        public Vector3 SliddingForce;
        public bool IsSliding
        {
            get { return _isSliding; }
            set
            {
                if (!_isSliding && value)
                {
                    //logger.Debug("Slide Start detected");
                    _stopSliddingCounter = 1;
                    _slidingCycles = 0;
                    _isSliding = true;
                }
                if (_isSliding && !value && _stopSliddingCounter <= 0)
                {
                    //logger.Debug("Slide Stop detected");
                    SliddingForce *= (_slidingCycles/4);
                    //logger.Debug("START Force {0} with {1} cycle", SliddingForce ,_slidingCycles );

                    Impulses.Add(new Impulse() { ForceApplied = SliddingForce });
                    StopMovementAction = true;

                    SliddingForce = default(Vector3);
                    _isSliding = false;
                }

                if (!value) _stopSliddingCounter--;

                if (_isSliding) _slidingCycles++;
            }
        }

        //If set to value other than 0, then the enviroment will emit a force that will absorbe all force being applied to the entity.
        public float Friction { get; set; }
        public float AirFriction { get; set; }

        public Vector3D CurPosition
        {
            get { return _curPosition; }
            set {
                PrevPosition = _curPosition;
                _curPosition = value; 
            }
        }
        public Vector3D PrevPosition { get { return _prevPosition; } set { _prevPosition = value; } }

        public delegate void CheckConstraintFct(VerletSimulator physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ref Vector3D originalPosition);
        public CheckConstraintFct ConstraintFct;

        public VerletSimulator(ref BoundingBox localBoundingBox)
        {
            _localBoundingBox = localBoundingBox;
            Friction = 1;
        }

        public void StartSimulation(Vector3D startingPosition)
        {
            _isRunning = true;
            _prevPosition = startingPosition;
            _curPosition = startingPosition;
        }

        public void StartSimulation(ref Vector3D startingPosition, ref Vector3D PreviousPosition)
        {
            _isRunning = true;
            _prevPosition = PreviousPosition;
            _curPosition = startingPosition;
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

            //_impulses.Clear();
        }

        public void Simulate(float elapsedTimeS, out Vector3D newPosition)
        {
            if (_isRunning)
            {
                AllowJumping = false;
                if (ConstraintOnlyMode == false)
                {
                    AccumulateForce(elapsedTimeS);                                //Add the force currently applied
                    Verlet(elapsedTimeS, out newPosition);                        //Compute the next location based taken into account the accumulated force, the time , ...
                }
                else
                {
                    newPosition = _curPosition;
                }
                isInContactWithLadder = false;
                SatisfyConstraints(ref newPosition, ref _prevPosition);          //Validate the new location based in constraint (Collision, ...)
            }
            else
            {
                //newPosition = Vector3D.Zero;
                newPosition = _curPosition;
                logger.Error("Simulate physic called while not running, please avoid !");
            }
        }

        private void AccumulateForce(float elapsedTimeS)
        {
            _forcesAccum.X = 0;
            _forcesAccum.Y = 0;
            _forcesAccum.Z = 0;

            //Vertical velocity if not on ground, to make the entity fall !
            if (_subjectToGravity && !_onGround && !isInContactWithLadder)
            {
                _forcesAccum.Y += -SimulatorCst.Gravity;
            }

            _impulses.RemoveAll(x => x.ApplyOnlyIfOnGround && OnGround == false);

            OnGround = false;

            foreach (var impulse in _impulses)
            {
                if (impulse.IsActive)
                {
                    _forcesAccum += (impulse.ForceApplied);
                    impulse.AmountOfTime -= elapsedTimeS;
                }
            }

            //CleanUp impulses
            _impulses.RemoveAll(x => x.IsActive == false);
        }

        private void Verlet(float elapsedTimeS, out Vector3D newPosition)
        {
            if (Friction > 0.0f)
            {
                //Create a viscosity force against what's in place !
                _curPosition += (_prevPosition - _curPosition) * Friction;
            }

            if (AirFriction > 0.0f)
            {
                Vector3D SideForces = new Vector3D(_prevPosition.X - _curPosition.X, 0, _prevPosition.Z - _curPosition.Z);
                _curPosition += SideForces * AirFriction;
            }

            newPosition = _curPosition + _curPosition - _prevPosition + (_forcesAccum * elapsedTimeS * elapsedTimeS);
            _prevPosition = _curPosition;
            _curPosition = newPosition;
        }

        private void SatisfyConstraints(ref Vector3D futurePosition, ref Vector3D OriginalPosition)
        {
            Vector3D OriginalPositionBackUp = OriginalPosition;
            foreach (CheckConstraintFct fct in ConstraintFct.GetInvocationList())
            {
                //This fct will be able to modify the newPosition if needed to satisfy its own constraint !
                fct(this, ref _localBoundingBox, ref futurePosition, ref OriginalPosition, ref OriginalPositionBackUp);
            }
            _curPosition = futurePosition;
        }
    }
}
