using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Entities.Voxel;
using S33M3Engines.Shared.Math;
using S33M3Engines.Maths;

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
        #endregion

        #region public variables
        #endregion

        public EntityPickingManager(IDynamicEntityManager dynamicEntityManager)                                    
        {
            _dynamicEntityManager = dynamicEntityManager;
        }

        public void Dispose()
        {
        }

        #region private methods
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
