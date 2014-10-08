//  -------------------------------------------------------------
//  Utopia.Shared project 
//  written by Vladislav Pozdnyakov (hackward@gmail.com) 2014-2014
//  -------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    [ProtoContract]
    public class HumanAI : GeneralAI
    {
        private readonly Stopwatch _timer = new Stopwatch();
        private KeyPoint _currentPoint;


        [ProtoMember(1)]
        public List<Activity> Activities { get; private set; }

        public Activity CurrentActivity { get; private set; }
        
        /// <summary>
        /// Gets current NPC state
        /// </summary>
        public ServerNpcState State { get; private set; }

        public HumanAI()
        {
            Activities = new List<Activity>();
        }

        public override void AISelect()
        {
            if (State != ServerNpcState.Idle)
                return;

            // validate activity
            var activity = Activities.OrderByDescending(a => a.StartAt).FirstOrDefault(a => a.StartAt < Server.Clock.Now.TimeOfDay);

            if (activity == null)
                return;

            if (CurrentActivity != activity)
            {
                // new activity
                CurrentActivity = activity;
                
                if (activity.KeyPoints.Count > 0)
                {
                    _currentPoint = Random.Next(activity.KeyPoints);
                    Movement.Goto(_currentPoint.Position);
                    State = ServerNpcState.Walking;
                }
                return;
            }
            
            if (Random.NextDouble() < 0.3f)
            {
                // go to next waypoint

                if (CurrentActivity.KeyPoints.Count > 1)
                {
                    _currentPoint = Random.NextExcept(CurrentActivity.KeyPoints, _currentPoint);
                    Movement.Goto(_currentPoint.Position);
                    State = ServerNpcState.Walking;
                }

                return;
            }

            if (Random.NextDouble() < 0.3f)
            {
                _timer.Restart();
                State = ServerNpcState.LookingAround;
                Focus.LookAtRandomEntity();
                return;
            }

        }

        public override void DoAction()
        {
            switch (State)
            {
                case ServerNpcState.Walking:
                    if (!Movement.IsActive)
                    {
                        State = ServerNpcState.Idle;

                        if (_currentPoint.HeadRotation != Quaternion.Zero)
                            Character.HeadRotation = _currentPoint.HeadRotation;
                    }
                    break;
                case ServerNpcState.LookingAround:
                    if (_timer.Elapsed.TotalSeconds > 5)
                        State = ServerNpcState.Idle;
                    break;
            }


        }
    }

    /// <summary>
    /// AI activity
    /// </summary>
    [ProtoContract]
    public class Activity
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        
        /// <summary>
        /// Day time when the activity should work
        /// </summary>
        [ProtoMember(2)]
        public UtopiaTimeSpan StartAt { get; set; }

        [ProtoMember(3)]
        public List<KeyPoint> KeyPoints { get; set; }

        public Activity()
        {
            KeyPoints = new List<KeyPoint>();
        }
    }

    [ProtoContract]
    public struct KeyPoint
    {
        public Vector3I Position { get; set; }

        public Quaternion HeadRotation { get; set; }
    }
}