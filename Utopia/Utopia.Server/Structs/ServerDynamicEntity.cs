using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Pure-server class for server area management, wraps dynamic entity
    /// </summary>
    public abstract class ServerDynamicEntity
    {

        public event EventHandler<ServerDynamicEntityMoveEventArgs> PositionChanged;

        private void OnPositionChanged(ServerDynamicEntityMoveEventArgs e)
        {
            EventHandler<ServerDynamicEntityMoveEventArgs> handler = PositionChanged;
            if (handler != null) handler(this, e);
        }

        public IDynamicEntity DynamicEntity { get; private set; }

        /// <summary>
        /// Perform actions when getting closer to area. Entity should add all needed event handlers
        /// </summary>
        /// <param name="area"></param>
        public abstract void AddArea(MapArea area);

        /// <summary>
        /// Perform actions when area is far away, entity should remove any event hadler it has
        /// </summary>
        /// <param name="area"></param>
        public abstract void RemoveArea(MapArea area);

        private MapArea _currentArea;

        /// <summary>
        /// Gets or sets current entity area
        /// </summary>
        public MapArea CurrentArea
        {
            get
            {
                return _currentArea;
            }
            set
            {
                if (_currentArea != value)
                {
                    if (_currentArea != null)
                    {
                        _currentArea.EntityInViewRange -= AreaEntityInViewRange;
                        _currentArea.EntityOutOfViewRange -= AreaEntityOutOfViewRange;
                    }

                    _currentArea = value;

                    if (_currentArea != null)
                    {
                        _currentArea.EntityInViewRange += AreaEntityInViewRange;
                        _currentArea.EntityOutOfViewRange += AreaEntityOutOfViewRange;
                    }
                }
            }
        }


        protected ServerDynamicEntity(IDynamicEntity entity)
        {
            DynamicEntity = entity;
            entity.PositionChanged += entity_PositionChanged;
        }

        void entity_PositionChanged(object sender, EntityMoveEventArgs e)
        {
            OnPositionChanged(new ServerDynamicEntityMoveEventArgs { ServerDynamicEntity = this, PreviousPosition = e.PreviousPosition });
        }

        /// <summary>
        /// Called when some entity goes out of view range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityOutOfViewRange(object sender, ServerDynamicEntityEventArgs e)
        {

        }

        /// <summary>
        /// Called when some entity get closer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void AreaEntityInViewRange(object sender, ServerDynamicEntityEventArgs e)
        {

        }

        /// <summary>
        /// Perform dynamic update (AI logic)
        /// </summary>
        public abstract void Update(DynamicUpdateState gameTime);

        public override int GetHashCode()
        {
            return DynamicEntity.GetHashCode();
        }
    }

    public class ServerDynamicEntityMoveEventArgs : EventArgs
    {
        public ServerDynamicEntity ServerDynamicEntity { get; set; }
        public DVector3 PreviousPosition { get; set; } 
    }
}
