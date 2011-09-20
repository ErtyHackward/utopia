using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.Entities.Renderer;
using Ninject;
using SharpDX;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Will keep a collection of IDynamicEntity received from server.
    /// Will be responsible mainly to Draw them, they will also be used to check collision detection with Player
    /// </summary>
    public class DynamicEntityManager : DrawableGameComponent, IDynamicEntityManager
    {
        #region Private variables
        private Dictionary<uint, VisualDynamicEntity> _dynamicEntitiesDico = new Dictionary<uint, VisualDynamicEntity>();
        private List<IVisualEntityContainer> _dynamicEntities = new List<IVisualEntityContainer>();
        private IEntitiesRenderer _dynamicEntityRenderer;
        private VoxelMeshFactory _voxelMeshFactory;
        #endregion

        #region Public variables/properties
        #endregion

        public DynamicEntityManager([Named("DefaultEntityRenderer")] IEntitiesRenderer dynamicEntityRenderer, VoxelMeshFactory voxelMeshFactory)
        {
            _voxelMeshFactory = voxelMeshFactory;
            _dynamicEntityRenderer = dynamicEntityRenderer;

            _dynamicEntityRenderer.VisualEntities = _dynamicEntities;
        }

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            VisualDynamicEntity vEntity;

            vEntity = new VisualDynamicEntity(entity, new Voxel.VisualEntity(_voxelMeshFactory, entity));

            return vEntity;
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
        }

        public override void Dispose()
        {
        }

        public override void Update(ref GameTime timeSpent)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Update(ref timeSpent);
            }
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Interpolation(ref interpolationHd, ref interpolationLd);
            }
        }

        public override void Draw(int Index)
        {
            _dynamicEntityRenderer.Draw(Index);
        }

        public void AddEntity(IDynamicEntity entity)
        {
            if (!_dynamicEntitiesDico.ContainsKey(entity.EntityId))
            {
                VisualDynamicEntity newEntity = CreateVisualEntity(entity);
                _dynamicEntitiesDico.Add(entity.EntityId, newEntity);
                _dynamicEntities.Add(newEntity);
            }
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            if (_dynamicEntitiesDico.ContainsKey(entity.EntityId))
            {
                _dynamicEntities.Remove(_dynamicEntitiesDico[entity.EntityId]);
                _dynamicEntitiesDico.Remove(entity.EntityId);
            }
        }

        public void RemoveEntityById(uint entityId)
        {
            if (_dynamicEntitiesDico.ContainsKey(entityId))
            {
                _dynamicEntities.Remove(_dynamicEntitiesDico[entityId]);
                _dynamicEntitiesDico.Remove(entityId);
            }
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            VisualDynamicEntity e;
            if (_dynamicEntitiesDico.TryGetValue(p,out e))
            {
                return e.DynamicEntity;
            }
            return null;
        }

        
        #endregion

        public IEnumerator<VisualEntity> EnumerateVisualEntities()
        {
            foreach (var visualEntityContainer in _dynamicEntities)
            {
                yield return visualEntityContainer.VisualEntity;    
            }
            
        }


    }
}
