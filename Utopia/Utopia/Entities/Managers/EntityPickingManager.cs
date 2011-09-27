﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Entities.Voxel;
using S33M3Engines.Shared.Math;
using S33M3Engines.Maths;
using S33M3Engines.Timers;
using Utopia.Shared.Chunks;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// The Aim of this class is to help the player entity picking
    /// It will need a collection of entities that are "Near" the player, in order to test the collision against as less entities as possible !
    /// </summary>
    public class EntityPickingManager : IEntityPickingManager, IDisposable
    {
        #region private variables
        private IDynamicEntityManager _dynamicEntityManager;
        private S33M3Engines.Timers.TimerManager.GameTimer _timer;
        private List<IVisualEntityContainer> _entitiesNearPlayer = new List<IVisualEntityContainer>(1000);
        private PlayerEntityManager _player;
        private int _entityDistance = AbstractChunk.ChunkSize.X * 2;
        #endregion

        #region public variables
        public PlayerEntityManager Player
        {
            get { return _player; }
            set { _player = value; }
        }
        #endregion

        public EntityPickingManager(IDynamicEntityManager dynamicEntityManager, TimerManager timerManager)                                    
        {
            _dynamicEntityManager = dynamicEntityManager;
            _timer = timerManager.AddTimer(1, 1000);
            _timer.OnTimerRaised += _timer_OnTimerRaised;
        }

        public void Dispose()
        {
            _timer.OnTimerRaised -= _timer_OnTimerRaised;
        }

        #region private methods
        private void _timer_OnTimerRaised()
        {
            CollectSurrendingPlayerEntities();
        }

        private void CollectSurrendingPlayerEntities()
        {
            //Clear the list
            _entitiesNearPlayer.Clear();

            IVisualEntityContainer entity;
            for (int i = 0; i < _dynamicEntityManager.DynamicEntities.Count; i++)
            {
                entity = _dynamicEntityManager.DynamicEntities[i];

                if (Vector3D.Distance(entity.VisualEntity.Position, _player.Player.Position) <= _entityDistance)
                {
                    _entitiesNearPlayer.Add(entity);
                }
            }

        }
        #endregion

        #region public methods
        public bool CheckEntityPicking(ref Vector3D pickingPoint, out IVisualEntityContainer pickedEntity)
        {
            IVisualEntityContainer entity;

            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                entity = _entitiesNearPlayer[i];
                if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref pickingPoint) == ContainmentType.Contains)
                {
                    pickedEntity = entity;
                    return true;
                }
            }
            pickedEntity = null;
            return false;
        }

        public void isCollidingWithEntity(ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            IVisualEntityContainer entity;
            //If new Position "inside" entity, then go back to previous Position !
            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                entity = _entitiesNearPlayer[i];
                if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref newPosition2Evaluate) == ContainmentType.Contains)
                {
                    newPosition2Evaluate = previousPosition;
                }
            }

        }
        #endregion
    }
}
