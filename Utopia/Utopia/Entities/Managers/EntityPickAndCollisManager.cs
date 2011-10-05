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
using Utopia.Shared.Chunks;
using Utopia.Network;
using Utopia.Action;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// The Aim of this class is to help the player entity picking
    /// It will need a collection of entities that are "Near" the player, in order to test the collision against as less entities as possible !
    /// </summary>
    public class EntityPickAndCollisManager : IEntityPickingManager, IDisposable
    {
        #region private variables
        private IDynamicEntityManager _dynamicEntityManager;
        private S33M3Engines.Timers.TimerManager.GameTimer _timer;
        private List<IVisualEntityContainer> _entitiesNearPlayer = new List<IVisualEntityContainer>(1000);
        private PlayerEntityManager _player;
        private int _entityDistance = AbstractChunk.ChunkSize.X * 2;
        private Server _server;
        private ActionsManager _action;
        private IWorldChunks _worldChunks;
        #endregion

        #region public variables
        public PlayerEntityManager Player
        {
            get { return _player; }
            set { _player = value; }
        }

        public IWorldChunks WorldChunks
        {
            get { return _worldChunks; }
            set { _worldChunks = value; }
        }
        #endregion

        public EntityPickAndCollisManager(IDynamicEntityManager dynamicEntityManager, 
                                          TimerManager timerManager,
                                          Server server,
                                          ActionsManager action)                                     
        {
            _dynamicEntityManager = dynamicEntityManager;
            _timer = timerManager.AddTimer(1, 1000);
            _timer.OnTimerRaised += _timer_OnTimerRaised;
            _action = action;
            _server = server;
        }

        public void Dispose()
        {
            _timer.OnTimerRaised -= _timer_OnTimerRaised;
        }

        #region private methods
        //Started everyseconds
        private void _timer_OnTimerRaised()
        {
            CollectSurrendingDynamicPlayerEntities(); //They have their own collection
            CollectSurrendingStaticPlayerEntities(); //They are stored inside chunks !
        }

        private void CollectSurrendingStaticPlayerEntities()
        {
            VisualChunk chunk;
            //Check inside the visible chunks (Not visible culled) the statics entities
            for (int i = 0; i < _worldChunks.Chunks.Length; i++)
            {
                chunk = _worldChunks.Chunks[i];
                if (chunk.isFrustumCulled == false)
                {
                    foreach (var entity in chunk.Entities.EnumerateFast())
                    {
                        //Add Static entities HERE for Picking and collision test !
                    }
                }
            }
        }

        private void CollectSurrendingDynamicPlayerEntities()
        {
            //Clear the list
            _entitiesNearPlayer.Clear();

            VisualDynamicEntity entity;
            for (int i = 0; i < _dynamicEntityManager.DynamicEntities.Count; i++)
            {
                entity = _dynamicEntityManager.DynamicEntities[i] as VisualDynamicEntity;

                if (entity != null)
                {
                    if (Vector3D.Distance(entity.VisualEntity.Position, _player.Player.Position) <= _entityDistance)
                    {
                        _entitiesNearPlayer.Add(entity);
                    }
                }
                else
                {
                    throw new Exception("Entity type not handled");
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
                    //Player was moving ?
                    if (newPosition2Evaluate != previousPosition)
                    {
                        Vector3D newPositionWithColliding = previousPosition;

                        newPositionWithColliding.X = newPosition2Evaluate.X;
                        if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref newPositionWithColliding) == ContainmentType.Contains)
                        {
                            newPositionWithColliding.X = previousPosition.X;
                        }

                        newPositionWithColliding.Y = newPosition2Evaluate.Y;
                        if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref newPositionWithColliding) == ContainmentType.Contains)
                        {
                            newPositionWithColliding.Y = previousPosition.Y;
                        }

                        newPositionWithColliding.Z = newPosition2Evaluate.Z;
                        if (MCollision.BoxContainsPoint(ref entity.VisualEntity.WorldBBox, ref newPositionWithColliding) == ContainmentType.Contains)
                        {
                            newPositionWithColliding.Z = previousPosition.Z;
                        }

                        newPosition2Evaluate = newPositionWithColliding;

                        //Send an impulse message to the Entity, following my "LookAtVector" !
                        float impulsePower = 1;
                        if(_action.isTriggered(Actions.Move_Run)) impulsePower = 2;

                        _server.ServerConnection.SendAsync(new EntityImpulseMessage()
                        {
                            EntityId = entity.VisualEntity.VoxelEntity.EntityId,
                            Vector3 = MQuaternion.GetLookAtFromQuaternion(_player.Player.Rotation) * impulsePower
                        }
                        );
                    }
                    else
                    {
                        Vector3D lookAt = MQuaternion.GetLookAtFromQuaternion_V3D(entity.VisualEntity.VoxelEntity.Rotation);
                        newPosition2Evaluate += lookAt * 0.1;
                    }
                }
            }

        }
        #endregion
    }
}
