using System.Diagnostics;
using System.Linq;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Server.Structs
{
    [ProtoContract]
    public class AnimalAI : GeneralAI
    {
        private readonly Stopwatch _timer = new Stopwatch();
        
        /// <summary>
        /// Gets current NPC state
        /// </summary>
        public ServerNpcState State { get; private set; }

        public override ServerNpc Npc
        {
            get { return base.Npc; }
            set { 
                base.Npc = value;
                Character.HealthChanged += CharacterOnHealthChanged;
            }
        }

        /// <summary>
        /// Choose what to do next
        /// </summary>
        public override void AISelect()
        {
            if (State != ServerNpcState.RunAway)
            {
                if (Npc.DangerousEntities.Count > 0)
                {
                    var entity = Npc.DangerousEntities.OrderBy(e => Vector3D.DistanceSquared(Character.Position, e.Position)).First();

                    if (Vector3D.Distance(entity.Position, Character.Position) < 16)
                    {
                        Movement.RunAway();
                        _timer.Restart();
                        State = ServerNpcState.RunAway;
                    }
                }
            }

            if (State != ServerNpcState.Idle)
                return;

            if (Random.NextDouble() < 0.3)
            {
                State = ServerNpcState.Walking;
                GoToRandomPoint();
                _timer.Restart();
                return;
            }

            State = ServerNpcState.LookingAround;
            _timer.Restart();
        }

        /// <summary>
        /// Perform choosen action
        /// </summary>
        public override void DoAction()
        {
            if (State == ServerNpcState.Idle)
                return;

            switch (State)
            {
                case ServerNpcState.RunAway:
                case ServerNpcState.Walking:

                    if (!Movement.IsActive || _timer.Elapsed.TotalSeconds > 30)
                    {
                        State = ServerNpcState.Idle;
                    }

                    break;
                case ServerNpcState.LookingAround:

                    if (Focus.Target == null || _timer.Elapsed.TotalSeconds > 10)
                    {
                        if (Focus.Target != null)
                        {
                            if (Random.NextDouble() < 0.3)
                            {
                                State = ServerNpcState.Idle;
                                return;
                            }
                        }

                        Focus.LookAtRandomEntity();
                    }
                    break;
                case ServerNpcState.Following:
                    break;
            }
        }

        private void CharacterOnHealthChanged(object sender, EntityHealthChangeEventArgs e)
        {
            if (e.SourceEntity != null)
            {
                if (!Npc.DangerousEntities.Contains(e.SourceEntity))
                    Npc.DangerousEntities.Add(e.SourceEntity);
            }
        }
    }
}