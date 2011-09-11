using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using S33M3Engines.D3D;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Shared.Chunks;

namespace Utopia.Entities
{
    public class EntityManager : DrawableGameComponent, IEntityManager, IDisposable 
    {
        #region Private variables
        // enumerate this entities to draw
        private Dictionary<uint, VisualDynamicEntity> _dynamicEntities = new Dictionary<uint, VisualDynamicEntity>();

        D3DEngine _d3dEngine;
        ActionsManager _action;
        InputsManager _inputManager;
        SingleArrayChunkContainer _chunkContainer;
        VoxelMeshFactory _voxelMeshFactory;
        #endregion

        #region Public Variables/Properties
        #endregion
        public EntityManager(D3DEngine d3dEngine,
                             ActionsManager action,
                             InputsManager inputManager,
                             SingleArrayChunkContainer chunkContainer,
                             VoxelMeshFactory voxelMeshFactory,
                             VisualPlayerCharacter player
                             )
        {
            _d3dEngine = d3dEngine;
            _action = action;
            _inputManager = inputManager;
            _chunkContainer = chunkContainer;
            _voxelMeshFactory = voxelMeshFactory;
            _dynamicEntities.Add(0, player);
        }

        #region Private Methods
        public VisualDynamicEntity CreateVisualEntity(IEntity entity)
        {
            VisualDynamicEntity vEntity;

            switch (entity.ClassId)
            {
                case EntityClassId.PlayerCharacter: vEntity = new VisualPlayerCharacter(_d3dEngine, new SharpDX.Vector3(0.5f, 1.9f, 0.5f), _action, _inputManager, _chunkContainer, _voxelMeshFactory, (PlayerCharacter)entity); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException("classId");
            }

            vEntity.Initialize();

            return vEntity;
        }
        #endregion

        #region Public Methods
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
                entity.Draw();
            }
        }

        public void AddEntity(Shared.Chunks.Entities.Interfaces.IDynamicEntity entity)
        {
            _dynamicEntities.Add(entity.EntityId, CreateVisualEntity(entity));
        }

        public void RemoveEntity(Shared.Chunks.Entities.Interfaces.IDynamicEntity entity)
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

        public void Dispose()
        {
        }
        #endregion
    }
}
