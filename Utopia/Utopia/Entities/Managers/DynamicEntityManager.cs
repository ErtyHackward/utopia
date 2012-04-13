using System;
using System.Collections.Generic;
using Utopia.Entities.Voxel;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Will keep a collection of IDynamicEntity received from server.
    /// Will be responsible mainly to Draw them, they will also be used to check collision detection with Player
    /// </summary>
    public class DynamicEntityManager : DrawableGameComponent, IDynamicEntityManager
    {
        #region Private variables
        private readonly Dictionary<uint, VisualDynamicEntity> _dynamicEntitiesDico = new Dictionary<uint, VisualDynamicEntity>();
        private readonly IEntitiesRenderer _dynamicEntityRenderer;
        private readonly VoxelModelManager _voxelModelManager;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> DynamicEntities { get; set; }
        public event EventHandler<DynamicEntityEventArgs> EntityAdded;
        public event EventHandler<DynamicEntityEventArgs> EntityRemoved;
        #endregion

        public DynamicEntityManager([Named("DefaultEntityRenderer")] IEntitiesRenderer dynamicEntityRenderer, VoxelModelManager voxelModelManager)
        {
            _dynamicEntityRenderer = dynamicEntityRenderer;
            _voxelModelManager = voxelModelManager;
        }

        public override void BeforeDispose()
        {
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
        }

        public override void Initialize()
        {
            DynamicEntities = new List<IVisualEntityContainer>();
            _dynamicEntityRenderer.VisualEntities = DynamicEntities;
            _dynamicEntityRenderer.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {
            _dynamicEntityRenderer.LoadContent(context);
        }

        public override void UnloadContent()
        {
            this.DisableComponent();
            _dynamicEntityRenderer.UnloadContent();
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
            _dynamicEntitiesDico.Clear();
            this.IsInitialized = false;
        }

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            return new VisualDynamicEntity(entity, new VisualVoxelEntity(entity, _voxelModelManager));
        }

        private void OnEntityAdded(DynamicEntityEventArgs e)
        {
            if (EntityAdded != null) EntityAdded(this, e);
        }

        private void OnEntityRemoved(DynamicEntityEventArgs e)
        {
            if (EntityRemoved != null) EntityRemoved(this, e);
        }

        #endregion

        #region Public Methods
        public override void Update(GameTime timeSpent)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Update(timeSpent);
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Interpolation(ref interpolationHd, ref interpolationLd);
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            //Only Draw the Entities that are in View Client scope !
            _dynamicEntityRenderer.Draw(context, index);
        }

        public void AddEntity(IDynamicEntity entity)
        {
            if (!_dynamicEntitiesDico.ContainsKey(entity.DynamicId))
            {
                VisualDynamicEntity newEntity = CreateVisualEntity(entity);
                _dynamicEntitiesDico.Add(entity.DynamicId, newEntity);
                DynamicEntities.Add(newEntity);

                OnEntityAdded(new DynamicEntityEventArgs { Entity = entity });
            }
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            if (_dynamicEntitiesDico.ContainsKey(entity.DynamicId))
            {
                VisualDynamicEntity visualEntity = _dynamicEntitiesDico[entity.DynamicId];
                DynamicEntities.Remove(visualEntity);
                _dynamicEntitiesDico.Remove(entity.DynamicId);
                visualEntity.Dispose();

                OnEntityRemoved(new DynamicEntityEventArgs { Entity = entity });
            }
        }

        public void RemoveEntityById(uint entityId,bool dispose=true)
        {
            if (_dynamicEntitiesDico.ContainsKey(entityId))
            {
                VisualDynamicEntity visualEntity = _dynamicEntitiesDico[entityId];
                DynamicEntities.Remove(_dynamicEntitiesDico[entityId]);
                _dynamicEntitiesDico.Remove(entityId);
                if (dispose) visualEntity.Dispose();
                OnEntityRemoved(new DynamicEntityEventArgs { Entity = visualEntity.DynamicEntity });
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

        public IEnumerator<VisualVoxelEntity> EnumerateVisualEntities()
        {
            foreach (var visualEntityContainer in DynamicEntities)
            {
                yield return visualEntityContainer.VisualEntity;
            }

        }
        #endregion

    }
}
