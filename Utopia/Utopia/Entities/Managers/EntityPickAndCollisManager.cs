﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Network;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks;
using S33M3CoreComponents.Timers;
using S33M3CoreComponents.Inputs.Actions;
using S33M3Resources.Structs;
using S33M3CoreComponents.Physics.Verlet;
using S33M3CoreComponents.Maths;
using Utopia.Action;
using S33M3CoreComponents.Inputs;

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
        //private TimerManager.GameTimer _timer;
        private List<VisualEntity> _entitiesNearPlayer = new List<VisualEntity>(1000);
        private PlayerEntityManager _player;
        private int _entityDistance = AbstractChunk.ChunkSize.X * 2;
        private ServerComponent _server;
        private InputsManager _input;
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

        public bool isDirty { get; set; }

        [Inject]
        public IDynamicEntityManager DynamicEntityManager
        {
            get { return _dynamicEntityManager; }
            set { _dynamicEntityManager = value; }
        }

        #endregion

        public EntityPickAndCollisManager(TimerManager timerManager,
                                          ServerComponent server,
                                          InputsManager input)                                     
        {
            //_timer = timerManager.AddTimer(1, 100);         //10 times/s
            //_timer.OnTimerRaised += _timer_OnTimerRaised;
            _input = input;
            _server = server;
        }

        public void Dispose()
        {
            //_timer.OnTimerRaised -= _timer_OnTimerRaised;
        }

        #region private methods
        //Started everyseconds
        private void _timer_OnTimerRaised()
        {
            CollectsurroundingDynamicPlayerEntities(); //They have their own collection
            CollectsurroundingStaticEntities();  //They are stored inside chunks !
            isDirty = false;
        }

        private void CollectsurroundingDynamicPlayerEntities()
        {
            //Clear the list
            _entitiesNearPlayer.Clear();

            VisualDynamicEntity entity;
            for (int i = 0; i < DynamicEntityManager.DynamicEntities.Count; i++)
            {
                entity = DynamicEntityManager.DynamicEntities[i] as VisualDynamicEntity;

                if (entity != null)
                {
                    if (Vector3D.Distance(entity.VisualVoxelEntity.VoxelEntity.Position, _player.Player.Position) <= _entityDistance)
                    {
                        _entitiesNearPlayer.Add(entity.VisualVoxelEntity);
                    }
                }
                else
                {
                    throw new Exception("Entity type not handled");
                }
            }
        }

        private void CollectsurroundingStaticEntities()
        {
            if (_worldChunks.SortedChunks == null) return;

            VisualChunk chunk;
            //Check inside the visible chunks (Not visible culled) the statics entities
            //Chunk are sorted around player, the 9 first are the chunk around the players.
            for (int i = 0; i < 9; i++)
            {
                chunk = _worldChunks.SortedChunks[i];
                foreach (var pair in chunk.VisualVoxelEntities)
                {
                    foreach (var entity in pair.Value)
                    {
                        //Add entity only if at <= 10 block distance !
                        if (Vector3D.Distance(entity.Entity.Position, _player.Player.Position) <= 10)
                        {
                            _entitiesNearPlayer.Add(entity);
                        }
                    }
                }
            }
        }
        #endregion

        #region public methods
        public bool CheckEntityPicking(ref Ray pickingRay, out VisualEntity pickedEntity)
        {
            if (isDirty) _timer_OnTimerRaised();

            VisualEntity entity;
            float currentDistance;
            float pickedEntityDistance = float.MaxValue;
            pickedEntity = null;
            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                entity = _entitiesNearPlayer[i];
                if (entity.Entity.IsPickable)
                {
                    Collision.RayIntersectsBox(ref pickingRay, ref entity.WorldBBox, out currentDistance);
                    if (currentDistance > 0)
                    {
                        if (currentDistance < pickedEntityDistance)
                        {
                            pickedEntityDistance = currentDistance;
                            pickedEntity = entity;
                        }
                    }
                }
            }
            if (pickedEntity == null) 
                return false;
            else 
                return true;
        }

        public void Update()
        {
            _timer_OnTimerRaised();
        }

        //Entity vs Player Collision detection
        //Use by physic engine
        public void isCollidingWithEntity(VerletSimulator physicSimu, ref BoundingBox entityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (isDirty) _timer_OnTimerRaised();

            VisualEntity entityTesting;
            BoundingBox _boundingBox2Evaluate;
            
            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                entityTesting = _entitiesNearPlayer[i];
                if (entityTesting.Entity.IsPlayerCollidable)
                {
                    //Compute the New world located player bounding box, that will be use for collision detection
                    _boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPosition2Evaluate.AsVector3(), entityBoundingBox.Maximum + newPosition2Evaluate.AsVector3());
                    boundingBoxCheck(physicSimu, entityTesting, ref entityBoundingBox, ref _boundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition);
                }
            }
        }

        bool OnEntityTop = false;

        private void boundingBoxCheck(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox entityBoundingBox, ref BoundingBox boundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (Collision.BoxContainsBox(ref entityTesting.WorldBBox, ref boundingBox2Evaluate) == ContainmentType.Intersects)
            {
                Vector3D newPositionWithColliding = previousPosition;

                newPositionWithColliding.Y = newPosition2Evaluate.Y;
                boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                if (Collision.BoxContainsBox(ref entityTesting.WorldBBox, ref boundingBox2Evaluate) == ContainmentType.Intersects)
                {
                    //Cannot go below, warp to the entity surface, and flag as "potentially" on ground
                    newPositionWithColliding.Y = entityTesting.WorldBBox.Maximum.Y; //previousPosition.Y;
                    newPositionWithColliding.Y += 0.001;
                    previousPosition.Y = newPositionWithColliding.Y;
                    OnEntityTop = true;
                }

                newPositionWithColliding.X = newPosition2Evaluate.X;
                boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                if (Collision.BoxContainsBox(ref entityTesting.WorldBBox, ref boundingBox2Evaluate) == ContainmentType.Intersects)
                {
                    newPositionWithColliding.X = previousPosition.X;
                    OnEntityTop = false;
                }

                newPositionWithColliding.Z = newPosition2Evaluate.Z;
                boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                if (Collision.BoxContainsBox(ref entityTesting.WorldBBox, ref boundingBox2Evaluate) == ContainmentType.Intersects)
                {
                    newPositionWithColliding.Z = previousPosition.Z;
                    OnEntityTop = false;
                }

                //Set the NEW player position after collision tests
                newPosition2Evaluate = newPositionWithColliding;

                // ? I'm on "TOP" of an object ???
                if (OnEntityTop)
                {
                    physicSimu.OnGround = true;
                }

                if (entityTesting.Entity is IDynamicEntity)
                {
                    //Send an impulse message to the Entity, following my "LookAtVector" !
                    float impulsePower = 1;
                    if (_input.ActionsManager.isTriggered(UtopiaActions.Move_Run)) impulsePower = 2;

                    _server.ServerConnection.SendAsync(new EntityImpulseMessage
                    {
                        DynamicEntityId = (entityTesting.Entity as IDynamicEntity).DynamicId,
                        Vector3 = MQuaternion.GetLookAtFromQuaternion(_player.Player.HeadRotation) * impulsePower
                    }
                    );
                }

            }
            else
            {
                //No collision with Y, is the block below me solid to entity ?
                if (OnEntityTop) //I was on ground previously, I'm still on ground ?
                {
                    boundingBox2Evaluate.Minimum.Y -= 0.01f; //Check entity below me
                    if (Collision.BoxContainsBox(ref entityTesting.WorldBBox, ref boundingBox2Evaluate) == ContainmentType.Intersects)
                    {
                        physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                        OnEntityTop = true;
                    }
                    else
                    {
                        OnEntityTop = false;
                    }
                }
            }
        }
        
        #endregion

    }
}
