﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Entities.Voxel;
using Utopia.Entities.Renderer;
using Ninject;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Shared.Entities.Interfaces;

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
        private IEntitiesRenderer _dynamicEntityRenderer;
        private readonly VoxelModelManager _voxelModelManager;
        private VoxelMeshFactory _voxelMeshFactory;
        #endregion

        #region Public variables/properties
        public List<IVisualEntityContainer> DynamicEntities { get; set; }
        #endregion

        public DynamicEntityManager([Named("DefaultEntityRenderer")] IEntitiesRenderer dynamicEntityRenderer, VoxelModelManager voxelModelManager, VoxelMeshFactory voxelMeshFactory)
        {
            DynamicEntities = new List<IVisualEntityContainer>();
            _voxelMeshFactory = voxelMeshFactory;
            _dynamicEntityRenderer = dynamicEntityRenderer;
            _voxelModelManager = voxelModelManager;

            _dynamicEntityRenderer.VisualEntities = DynamicEntities;
        }

        public override void Dispose()
        {
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
            base.Dispose();
        }

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            VisualDynamicEntity vEntity;

            vEntity = new VisualDynamicEntity(entity, new Voxel.VisualVoxelEntity(entity, _voxelModelManager, _voxelMeshFactory));

            return vEntity;
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            _dynamicEntityRenderer.Initialize();
        }

        public override void LoadContent()
        {
            _dynamicEntityRenderer.LoadContent();
        }

        public override void Update(ref GameTime timeSpend)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Update(ref timeSpend);
            }
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Interpolation(ref interpolationHd, ref interpolationLd);
            }
        }

        public override void Draw(int index)
        {
            //Only Draw the Entities that are in View Client scope !
            _dynamicEntityRenderer.Draw(index);
        }

        public void AddEntity(IDynamicEntity entity)
        {
            if (!_dynamicEntitiesDico.ContainsKey(entity.DynamicId))
            {
                VisualDynamicEntity newEntity = CreateVisualEntity(entity);
                _dynamicEntitiesDico.Add(entity.DynamicId, newEntity);
                DynamicEntities.Add(newEntity);
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

        public IEnumerator<VisualVoxelEntity> EnumerateVisualEntities()
        {
            foreach (var visualEntityContainer in DynamicEntities)
            {
                yield return visualEntityContainer.VisualEntity;    
            }
            
        }


    }
}
