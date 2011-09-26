using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Entities.Voxel;
using S33M3Engines.Shared.Math;
using S33M3Engines.Maths;
using S33M3Engines.Timers;

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
        #endregion

        #region public variables
        #endregion

        public EntityPickingManager(IDynamicEntityManager dynamicEntityManager, TimerManager timerManager)                                    
        {
            _dynamicEntityManager = dynamicEntityManager;
            _timer = timerManager.AddTimer(1, 2000);
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
            IVisualEntityContainer entity;
            for (int i = 0; i < _dynamicEntityManager.DynamicEntities.Count; i++)
            {
                entity = _dynamicEntityManager.DynamicEntities[i];
                //On Player Range OR not ?
                //If yes ==> Store it, else skip it !
            }
        }
        #endregion

        #region public methods
        public bool CheckEntityPicking(ref Vector3D pickingPoint, out IVisualEntityContainer pickedEntity)
        {
            IVisualEntityContainer entity;

            for (int i = 0; i < _dynamicEntityManager.DynamicEntities.Count; i++)
            {
                entity = _dynamicEntityManager.DynamicEntities[i];
                if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref pickingPoint) == ContainmentType.Contains)
                {
                    pickedEntity = entity;
                    return true;
                }
            }
            pickedEntity = null;
            return false;
        }
        #endregion
    }
}
