using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Voxel;
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

            vEntity = new VisualDynamicEntity(entity, new Voxel.VisualEntity(_voxelMeshFactory, new PlayerCharacterBody()));

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

                //TODO To remove when Voxel Entity erge will done with Entity
                //Update the position and World Matrix of the Voxel body of the Entity.
                entity.VisualEntity.Position = entity.WorldPosition.ValueInterp;
                Vector3 entityCenteredPosition = entity.WorldPosition.ValueInterp.AsVector3();
                entityCenteredPosition.X -= entity.DynamicEntity.Size.X / 2;
                entityCenteredPosition.Z -= entity.DynamicEntity.Size.Z / 2;
                entity.VisualEntity.World = Matrix.Scaling(entity.DynamicEntity.Size) * Matrix.Translation(entityCenteredPosition);
                //===================================================================================================================================
            }
        }

        public override void Draw()
        {
            _dynamicEntityRenderer.Draw();
        }

        public void AddEntity(IDynamicEntity entity)
        {
            VisualDynamicEntity newEntity = CreateVisualEntity(entity);
            _dynamicEntitiesDico.Add(entity.EntityId, newEntity);
            _dynamicEntities.Add(newEntity);
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            _dynamicEntities.Remove(_dynamicEntitiesDico[entity.EntityId]);
            _dynamicEntitiesDico.Remove(entity.EntityId);
        }

        public void RemoveEntityById(uint entityId)
        {
            _dynamicEntities.Remove(_dynamicEntitiesDico[entityId]);
            _dynamicEntitiesDico.Remove(entityId);
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            return _dynamicEntitiesDico[p].DynamicEntity;
        }
        #endregion
    }
}
