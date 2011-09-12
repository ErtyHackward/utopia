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
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using Utopia.Shared.Chunks.Entities.Voxel;

namespace Utopia.Entities
{
    public class EntityManager : DrawableGameComponent, IEntityManager, IDisposable 
    {
        #region Private variables
        // enumerate this entities to draw
        private Dictionary<uint, VisualDynamicEntity> _dynamicEntities = new Dictionary<uint, VisualDynamicEntity>();

        private D3DEngine _d3dEngine;
        private ActionsManager _action;
        private InputsManager _inputManager;
        private SingleArrayChunkContainer _chunkContainer;
        private VoxelMeshFactory _voxelMeshFactory;
        private CameraManager _cameraManager;
        private WorldFocusManager _worldFocusManager;
        #endregion

        #region Public Variables/Properties
        #endregion

        public EntityManager(D3DEngine d3dEngine,
                             CameraManager cameraManager,
                             WorldFocusManager worldFocusManager,
                             ActionsManager action,
                             InputsManager inputManager,
                             SingleArrayChunkContainer chunkContainer,
                             VoxelMeshFactory voxelMeshFactory
                             )
        {
            _d3dEngine = d3dEngine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _action = action;
            _inputManager = inputManager;
            _chunkContainer = chunkContainer;
            _voxelMeshFactory = voxelMeshFactory;
        }

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IEntity entity)
        {
            VisualDynamicEntity vEntity;

            switch (entity.ClassId)
            {
                case EntityClassId.PlayerCharacter: vEntity = new VisualPlayerCharacter(_d3dEngine, _cameraManager, _worldFocusManager, _action, _inputManager, _chunkContainer, _voxelMeshFactory, (PlayerCharacter)entity, new PlayerCharacterBody()); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException("classId");
            }

            vEntity.IsPlayerConstroled = false;

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
            //if (_dynamicEntities.ContainsKey(entity.EntityId))
            //{
            //    Console.WriteLine("SERVER try to insert an already existing entity !! : " + entity.EntityId.ToString());
            //        return;
            //}
            _dynamicEntities.Add(entity.EntityId, CreateVisualEntity(entity));
        }

        public void RemoveEntity(Shared.Chunks.Entities.Interfaces.IDynamicEntity entity)
        {
            _dynamicEntities.Remove(entity.EntityId);
        }

        public void RemoveEntityById(uint entityId)
        {
            //Console.WriteLine("Entity removed : " + entityId);
            //return;
            _dynamicEntities.Remove(entityId);
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            return _dynamicEntities[p].DynamicEntity;
        }

        public override void Dispose()
        {
        }
        #endregion
    }
}
