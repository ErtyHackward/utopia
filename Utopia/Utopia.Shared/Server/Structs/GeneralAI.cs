using System;
using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Base class for different AI classes
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(AnimalAI))]
    [ProtoInclude(101, typeof(HumanAI))]
    public abstract class GeneralAI
    {
        private int _randomRadius = 16;

        [Browsable(false)]
        public virtual ServerNpc Npc { get; set; }

        [Browsable(false)]
        public CharacterEntity Character { get { return Npc.Character; } }

        [Browsable(false)]
        public MoveAI Movement { get { return Npc.Movement; } }

        [Browsable(false)]
        public Random Random { get { return Npc.Random; } }

        [Browsable(false)]
        public FocusAI Focus { get { return Npc.Focus; } }

        [Browsable(false)]
        public ServerCore Server { get { return Npc.Server; } }

        [Browsable(false)]
        public Faction Faction { get { return Npc.Faction; } }

        /// <summary>
        /// Choose what to do next
        /// </summary>
        public abstract void AISelect();

        /// <summary>
        /// Perform choosen action
        /// </summary>
        public abstract void DoAction();

        protected void GoToRandomPoint()
        {
            var moveVector = Random.NextVector2IOnRadius(16);
            var curPos = BlockHelper.EntityToBlock(Character.Position);
            var movePos = curPos + new Vector3I(moveVector.X, 0, moveVector.Y);

            var cursor = Server.LandscapeManager.GetCursor(movePos);

            for (int m = 0; m < 4; m++)
            {
                var checkVectorUp = movePos + new Vector3I(0, m, 0);
                var checkVectorDown = movePos + new Vector3I(0, -m, 0);

                if (checkVectorUp.Y < AbstractChunk.ChunkSize.Y - 1)
                {
                    if (CanStandThere(cursor, checkVectorUp))
                    {
                        Movement.Goto(checkVectorUp);

                        if (_randomRadius < 16)
                            _randomRadius++;

                        return;
                    }
                }

                if (checkVectorDown.Y > 1)
                {
                    if (CanStandThere(cursor, checkVectorDown))
                    {
                        Movement.Goto(checkVectorDown);

                        if (_randomRadius < 16)
                            _randomRadius++;

                        return;
                    }
                }
            }

            if (_randomRadius > 2)
                _randomRadius--;
        }

        private bool CanStandThere(ILandscapeCursor cursor, Vector3I pos)
        {
            cursor.GlobalPosition = pos;

            return cursor.Read() == WorldConfiguration.CubeId.Air &&
                   cursor.PeekValue(Vector3I.Up) == WorldConfiguration.CubeId.Air &&
                   cursor.PeekValue(Vector3I.Down) != WorldConfiguration.CubeId.Air;
        }
    }
}