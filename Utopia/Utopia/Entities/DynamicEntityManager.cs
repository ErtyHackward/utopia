using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Voxel;

namespace Utopia.Entities
{
    /// <summary>
    /// Will keep a collection of IDynamicEntity received from server.
    /// Will be responsible mainly to Draw them, they will also be used to check collision detection with Player
    /// </summary>
    public class DynamicEntityManager : DrawableGameComponent, IDynamicEntityManager
    {
        #region Private variables
        private Dictionary<uint, VisualDynamicEntity> _dynamicEntities;
        #endregion

        #region Public variables/properties
        #endregion

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            VisualDynamicEntity vEntity;

            vEntity = new VisualDynamicEntity(entity, new PlayerCharacterBody());

            return vEntity;
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            _dynamicEntities = new Dictionary<uint, VisualDynamicEntity>();
        }

        public override void LoadContent()
        {
        }

        public override void Dispose()
        {
        }

        public override void Update(ref GameTime timeSpent)
        {
            foreach (var entity in _dynamicEntities.Values)
            {
                entity.Update(ref timeSpent);
            }
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            foreach (var entity in _dynamicEntities.Values)
            {
                entity.Interpolation(ref interpolationHd, ref interpolationLd);
            }
        }

        public override void Draw()
        {
            foreach (var entity in _dynamicEntities.Values)
            {
                //entity.Draw();
            }
        }

        public void AddEntity(IDynamicEntity entity)
        {
            _dynamicEntities.Add(entity.EntityId, CreateVisualEntity(entity));
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            _dynamicEntities.Remove(entity.EntityId);
        }

        public void RemoveEntityById(uint entityId)
        {
            _dynamicEntities.Remove(entityId);
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            return _dynamicEntities[p].DynamicEntity;
        }
        #endregion
    }
}
