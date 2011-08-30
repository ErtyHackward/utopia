using System;
using System.Collections.Generic;
using SharpDX;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Management;
using Utopia.Shared.ClassExt;

namespace Utopia.Tests.EntityManagement
{
    /// <summary>
    /// Represents an entity that moves using some vector and says "Hello" to entities around it
    /// </summary>
    public class WalkingTestEntity : LivingEntity, IDynamicEntity
    {
        public static Rectangle WalkRange = new Rectangle(-5000, -5000, 5000, 5000);

        public Vector2 MoveVector { get; set; }

        public HashSet<IEntity> Entities { get; set; }

        public override EntityClassId ClassId
        {
            get { throw new NotImplementedException(); }
        }

        public override string DisplayName
        {
            get { return "Moving entity "+EntityId; }
        }

        public WalkingTestEntity()
        {
            Entities = new HashSet<IEntity>();
        }

        public event EventHandler<EntityMoveEventArgs> PositionChanged;

        protected void OnPositionChanged(EntityMoveEventArgs e)
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Perform actions when getting into the area
        /// </summary>
        /// <param name="area"></param>
        public void AddArea(MapArea area)
        {
            area.EntityMoved += AreaEntityMoved;
        }

        /// <summary>
        /// Perform actions when leaving the area
        /// </summary>
        /// <param name="area"></param>
        public void RemoveArea(MapArea area)
        {
            area.EntityMoved -= AreaEntityMoved;
        }

        void AreaEntityMoved(object sender, EntityMoveEventArgs e)
        {
            if (Vector3.Distance(Position, e.Entity.Position) < 10)
            {
                if (!Entities.Contains(e.Entity))
                {
                    Entities.Add(e.Entity);
                    //Trace.WriteLine(DisplayName + ": Hello "+e.Entity.DisplayName);
                }
            }
        }

        private DateTime _lastUpdate;

        public void Update(DateTime gameTime)
        {
            // we need to skip duplicate updates calls
            if(_lastUpdate == gameTime)
                return;

            _lastUpdate = gameTime;

            var previousPosition = Position;
            Position = new Vector3(Position.X + MoveVector.X, Position.Y, Position.Z + MoveVector.Y);

            if (!WalkRange.Contains(Position))
            {
                // reverse the vector
                MoveVector = new Vector2(-MoveVector.X, -MoveVector.Y);
                Position = new Vector3(Position.X + MoveVector.X, Position.Y, Position.Z + MoveVector.Y);
            }
            else OnPositionChanged(new EntityMoveEventArgs { Entity = this, PreviousPosition = previousPosition });
        }
    }

}
