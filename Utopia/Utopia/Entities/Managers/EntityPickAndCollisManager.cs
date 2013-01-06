using System;
using System.Collections.Generic;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Network;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks;
using S33M3CoreComponents.Timers;
using S33M3Resources.Structs;
using S33M3CoreComponents.Physics.Verlet;
using S33M3CoreComponents.Maths;
using Utopia.Action;
using S33M3CoreComponents.Inputs;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Models;
using S33M3CoreComponents.Physics;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// The Aim of this class is to help the player entity picking
    /// It will need a collection of entities that are "Near" the player, in order to test the collision against as less entities as possible !
    /// </summary>
    public class EntityPickAndCollisManager : IEntityPickingManager, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region private variables
        private IDynamicEntityManager _dynamicEntityManager;
        //private TimerManager.GameTimer _timer;
        private List<VisualEntity> _entitiesNearPlayer = new List<VisualEntity>(1000);
        private PlayerEntityManager _player;
        private int _entityDistance = AbstractChunk.ChunkSize.X * 2;
        private ServerComponent _server;
        private InputsManager _input;
        private IWorldChunks _worldChunks;
        private PlayerEntityManager _playerManager;
        private bool? _onEntityTop = null;
        private HandTool _handTool = new HandTool();

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

        [Inject]
        public ILandscapeManager2D LandscapeManager
        {
            get { return _landscapeManager; }
            set { 
                _landscapeManager = value;
                _handTool.LandscapeManager = _landscapeManager;
            }
        }

        #endregion

        public EntityPickAndCollisManager(TimerManager timerManager,
                                          ServerComponent server,
                                          InputsManager input,
                                          PlayerEntityManager playerManager)                                     
        {
            //_timer = timerManager.AddTimer(1, 100);         //10 times/s
            //_timer.OnTimerRaised += _timer_OnTimerRaised;
            _input = input;
            _server = server;
            _playerManager = playerManager;
            
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
        
        public EntityPickResult CheckEntityPicking(Ray pickingRay)
        {
            if (isDirty) 
                _timer_OnTimerRaised();

            var result = new EntityPickResult();
            result.Distance = float.MaxValue;

            var tool = Player.Player.Equipment.RightTool;

            if (tool == null)
                tool = _handTool;

            var pickedEntityDistance = float.MaxValue;
            foreach (var entity in _entitiesNearPlayer)
            {
                if (tool.CanPickEntity(entity.Entity) == PickType.Pick)
                {
                    //Refresh entity bounding box world
                    if (entity.Entity.CollisionType == Entity.EntityCollisionType.Model) // ==> Find better interface, for all state swtiching static entities
                    {
                        var localStaticEntityBb = ((VisualVoxelEntity)entity).VoxelEntity.ModelInstance.State.BoundingBox;
                        localStaticEntityBb = localStaticEntityBb.Transform(Matrix.RotationQuaternion(((IStaticEntity)entity.Entity).Rotation));          //Rotate the BoundingBox
                        //Recompute the World bounding box of the entity based on a new Entity BoundingBox
                        entity.SetEntityVoxelBB(localStaticEntityBb); //Will automaticaly apply a 1/16 scaling on the boundingbox
                    }

                    float currentDistance;
                    if (Collision.RayIntersectsBox(ref pickingRay, ref entity.WorldBBox, out currentDistance))
                    {
                        if (currentDistance < pickedEntityDistance)
                        {
                            if (entity.Entity.CollisionType == Entity.EntityCollisionType.Model)
                            {
                                Vector3 tmpPickPoint;
                                Vector3I tmpPickNormal;

                                if (!ModelRayIntersection(entity, pickingRay, out tmpPickPoint, out tmpPickNormal, out currentDistance))
                                    continue;

                                result.PickPoint = tmpPickPoint;
                                result.PickNormal = tmpPickNormal;

                            }
                            else
                            {
                                Collision.RayIntersectsBox(ref pickingRay, ref entity.WorldBBox, out result.PickPoint);
                                result.PickNormal = entity.WorldBBox.GetPointNormal(result.PickPoint);
                            }


                            pickedEntityDistance = currentDistance;
                            result.PickedEntity = entity;
                            result.Found = true;
                        }
                    }
                }
            }

            result.Distance = pickedEntityDistance;

            return result;
        }

        public void Update()
        {
            _timer_OnTimerRaised();
        }

        //Entity vs Player Collision detection
        //Use by physic engine
        public void isCollidingWithEntity(VerletSimulator physicSimu, ref BoundingBox playerBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (isDirty) _timer_OnTimerRaised();

            VisualEntity entityTesting;
            BoundingBox _playerBoundingBox2Evaluate;
            
            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                entityTesting = _entitiesNearPlayer[i];

                if (entityTesting.Entity.IsPlayerCollidable)
                {
                    //Compute the New world located player bounding box, that will be use for collision detection
                    _playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPosition2Evaluate.AsVector3(), playerBoundingBox.Maximum + newPosition2Evaluate.AsVector3());
                    CollisionCheck(physicSimu, entityTesting, ref playerBoundingBox, ref _playerBoundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition);
                }
            }
        }

        private void CollisionCheck(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox entityBoundingBox, ref BoundingBox boundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (entityTesting.WorldBBox.Intersects(ref boundingBox2Evaluate))
            {
                if (entityTesting.Entity is IOrientedSlope)
                {
                    IOrientedSlope entity = (IOrientedSlope)entityTesting.Entity;
                    if (entity.IsOrientedSlope)
                    {
                        SlopeCollisionDetection(physicSimu, entityTesting, ref entityBoundingBox, ref boundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition, entity.Orientation);
                        return;
                    }
                }

                switch (entityTesting.Entity.CollisionType)
                {
                    case Utopia.Shared.Entities.Entity.EntityCollisionType.BoundingBox:
                        BoundingBoxCollision(physicSimu, entityTesting, ref entityBoundingBox, ref boundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition);
                        break;
                    case Utopia.Shared.Entities.Entity.EntityCollisionType.Model:
                        ModelCollisionDetection(physicSimu, entityTesting, ref entityBoundingBox, ref boundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition);
                        break;
                    default:
                        break;
                }
            }

        }

        bool _isOnGround;
        private ILandscapeManager2D _landscapeManager;

        private void ModelCollisionDetection(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox playerBoundingBox, ref BoundingBox playerBoundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (entityTesting.SkipOneCollisionTest)
            {
                entityTesting.SkipOneCollisionTest = false;
                return;
            }

            Vector3D newPositionWithColliding = previousPosition;
            _onEntityTop = null;

            newPositionWithColliding.X = newPosition2Evaluate.X;
            playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
            {
                //logger.Debug("ModelCollisionDetection X detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.X, previousPosition.X);

                newPositionWithColliding.X = previousPosition.X;
                _onEntityTop = false;
                _playerManager.YForceApplying = entityTesting.Entity.YForceOnSideHit;

            }

            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
            {
                //logger.Debug("ModelCollisionDetection Z detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.Z, previousPosition.Z);

                newPositionWithColliding.Z = previousPosition.Z;
                _onEntityTop = false;
                _playerManager.YForceApplying = entityTesting.Entity.YForceOnSideHit;
            }

            newPositionWithColliding.Y = newPosition2Evaluate.Y;
            playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
            {
                //logger.Debug("ModelCollisionDetection Y detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.Y, previousPosition.Y);

                newPositionWithColliding.Y = previousPosition.Y;
                if (_onEntityTop == null)
                {
                    _onEntityTop = true;
                }
            }
            else
            {
                if (_isOnGround)
                {
                    playerBoundingBox2Evaluate.Minimum.Y -= 0.01f;
                    if (IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
                    {
                        _onEntityTop = true;
                    }

                }
            }

            //Set the NEW player position after collision tests
            newPosition2Evaluate = newPositionWithColliding;

            if (_onEntityTop == true)
            {
                physicSimu.OnGround = true;
                _isOnGround = true;
                physicSimu.AllowJumping = true;
            }
            else
            {
                _isOnGround = false;
            }

            playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (_onEntityTop != true && IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
            {
                //I'm "Blocked" by this entity !
                //Testing, inject Force to unblock myself !

                var forceDirection = playerBoundingBox2Evaluate.GetCenter() - entityTesting.WorldBBox.GetCenter();
                forceDirection.Normalize();

                physicSimu.Impulses.Add(new Impulse { ForceApplied = forceDirection * 3 });
                entityTesting.SkipOneCollisionTest = true;
            }
        }

        private bool ModelRayIntersection(VisualEntity entity, Ray pickRay, out Vector3 intersectionPoint, out Vector3I normal, out float dist)
        {
            intersectionPoint = new Vector3();
            normal = new Vector3I();
            dist = float.MaxValue;

            var visualVoxelEntity = entity as VisualVoxelEntity;
            if (visualVoxelEntity == null) 
                return false;

            var instance = visualVoxelEntity.VoxelEntity.ModelInstance;

            int index;
            bool collisionDetected = false;
            //Check Against all existing "Sub-Cube" model

            //Get current Active state = A model can have multiple "State" (Like open, close, mid open, ...)
            var activeModelState = instance.State;

            var visualModel = visualVoxelEntity.VisualVoxelModel;

            //For each Part in the model (A model can be composed of several parts)
            for (int partId = 0; partId < visualModel.VoxelModel.Parts.Count; partId++)
            {
                VoxelModelPartState partState = activeModelState.PartsStates[partId];

                // it is possible that there is no frame, so no need to check it
                if (partState.ActiveFrame == byte.MaxValue)
                    continue;

                VoxelModelPart part = visualModel.VoxelModel.Parts[partId];
                BoundingBox frameBoundingBox = visualModel.VisualVoxelFrames[partState.ActiveFrame].BoundingBox;

                //Get Current Active part Frame = In animation case, the frame will be different when time passing by ... (Time depends)
                var activeframe = visualModel.VoxelModel.Frames[partState.ActiveFrame]; //one active at a time

                Matrix invertedEntityWorldMatrix = partState.GetTransformation() * Matrix.RotationQuaternion(instance.Rotation) * instance.World;
                Matrix entityWorldMatrix = invertedEntityWorldMatrix;
                invertedEntityWorldMatrix.Invert();

                // convert ray to entity space
                var ray = pickRay.Transform(invertedEntityWorldMatrix);

                float partDistance;
                // if we don't intersect part BB then there is no reason to check each block BB
                if (!Collision.RayIntersectsBox(ref ray, ref frameBoundingBox, out partDistance))
                    continue;
                
                // don't check part that is far than already found intersection
                if (partDistance >= dist)
                    continue;
                
                //Check each frame Body part
                Vector3I chunkSize = activeframe.BlockData.ChunkSize;
                byte[] data = activeframe.BlockData.BlockBytes;

                index = -1;
                //Get all sub block not empty
                for (var z = 0; z < chunkSize.Z; z++)
                {
                    for (var x = 0; x < chunkSize.X; x++)
                    {
                        for (var y = 0; y < chunkSize.Y; y++)
                        {
                            index++;

                            //Get cube
                            if (data[index] > 0)
                            {
                                //Collision checking against this ray
                                var box = new BoundingBox(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));

                                float blockDist;
                                if (Collision.RayIntersectsBox(ref ray, ref box, out blockDist) && blockDist < dist)
                                {
                                    dist = blockDist;
                                    Collision.RayIntersectsBox(ref ray, ref box, out intersectionPoint);
                                    normal = box.GetPointNormal(intersectionPoint);

                                    intersectionPoint = Vector3.TransformCoordinate(intersectionPoint, entityWorldMatrix);

                                    collisionDetected = true;
                                }

                                
                            }
                        }
                    }
                }
            }

            return collisionDetected;
        }

        private bool IsCollidingWithModel(VisualEntity entityTesting, BoundingBox playerBoundingBox2Evaluate)
        {
            var visualVoxelEntity = entityTesting as VisualVoxelEntity;
            if (visualVoxelEntity == null) 
                return false;

            var instance = visualVoxelEntity.VoxelEntity.ModelInstance;

            int index;
            bool collisionDetected = false;
            //Check Against all existing "Sub-Cube" model

            //Get current Active state = A model can have multiple "State" (Like open, close, mid open, ...)
            var activeModelState = instance.State;

            var visualModel = visualVoxelEntity.VisualVoxelModel;
            
            //For each Part in the model (A model can be composed of several parts)
            for (int partId = 0; partId < visualModel.VoxelModel.Parts.Count && !collisionDetected; partId++)
            {
                VoxelModelPartState partState = activeModelState.PartsStates[partId];

                // it is possible that there is no frame, so no need to check anything
                if (partState.ActiveFrame == byte.MaxValue)
                    continue;

                VoxelModelPart part = visualModel.VoxelModel.Parts[partId];
                BoundingBox frameBoundingBox = visualModel.VisualVoxelFrames[partState.ActiveFrame].BoundingBox;
               
                //Get Current Active part Frame = In animation case, the frame will be different when time passing by ... (Time depends)
                var activeframe = visualModel.VoxelModel.Frames[partState.ActiveFrame]; //one active at a time

                Matrix invertedEntityWorldMatrix = partState.GetTransformation() * Matrix.RotationQuaternion(instance.Rotation) * instance.World;
                invertedEntityWorldMatrix.Invert();

                BoundingBox PlayerBBInEntitySpace = playerBoundingBox2Evaluate.Transform(invertedEntityWorldMatrix);

                // if we don't intersect part BB then there is no reason to check each block BB
                if (!frameBoundingBox.Intersects(ref PlayerBBInEntitySpace))
                    continue;

                //Check each frame Body part
                Vector3I chunkSize = activeframe.BlockData.ChunkSize;
                byte[] data = activeframe.BlockData.BlockBytes;

                index = -1;
                //Get all sub block not empty
                for (int z = 0; z < chunkSize.Z && !collisionDetected; z++)
                {
                    for (int x = 0; x < chunkSize.X && !collisionDetected; x++)
                    {
                        for (int y = 0; y < chunkSize.Y && !collisionDetected; y++)
                        {
                            index++;

                            //Get cube
                            if (data[index] > 0)
                            {
                                //Collision checking against this point.

                                if (PlayerBBInEntitySpace.Minimum.X > x + 1 || x > PlayerBBInEntitySpace.Maximum.X)
                                    continue; //No collision

                                if (PlayerBBInEntitySpace.Minimum.Y > y + 1 || y > PlayerBBInEntitySpace.Maximum.Y)
                                    continue; //No collision

                                if (PlayerBBInEntitySpace.Minimum.Z > z + 1 || z > PlayerBBInEntitySpace.Maximum.Z)
                                    continue; //No collision

                                //Collision HERE !!!
                                collisionDetected = true;
                            }
                        }
                    }
                }
            }

            return collisionDetected;
        }


        private void SlopeCollisionDetection(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox playerBoundingBox, ref BoundingBox playerBoundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ItemOrientation slopeOrientation)
        {

            if (IsSlopeCollisionDetection(physicSimu, entityTesting, ref playerBoundingBox, ref playerBoundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition, slopeOrientation))
            {
                Vector3D newPositionWithColliding = previousPosition;

                newPositionWithColliding.X = newPosition2Evaluate.X;
                playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                if (entityTesting.WorldBBox.Intersects(ref playerBoundingBox2Evaluate))
                {
                    newPositionWithColliding.X = previousPosition.X;
                }

                newPositionWithColliding.Z = newPosition2Evaluate.Z;
                playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                if (entityTesting.WorldBBox.Intersects(ref playerBoundingBox2Evaluate))
                {
                    newPositionWithColliding.Z = previousPosition.Z;
                }

                newPosition2Evaluate = newPositionWithColliding;
            }

        }

        private bool IsSlopeCollisionDetection(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox playerBoundingBox, ref BoundingBox playerBoundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ItemOrientation slopeOrientation)
        {
            Vector3 entityPosition = newPosition2Evaluate.AsVector3();
            float posi = 0.0f;

            float L = 0.0f;
            float H = entityTesting.WorldBBox.Maximum.Y - entityTesting.WorldBBox.Minimum.Y;

            switch (slopeOrientation)
            {
                case ItemOrientation.North:
                    L = entityTesting.WorldBBox.Maximum.Z - entityTesting.WorldBBox.Minimum.Z;
                    posi = (entityPosition.Z + playerBoundingBox.Maximum.Z) - entityTesting.WorldBBox.Minimum.Z;
                    break;
                case ItemOrientation.South:
                    L = entityTesting.WorldBBox.Maximum.Z - entityTesting.WorldBBox.Minimum.Z;
                    posi = entityTesting.WorldBBox.Maximum.Z - (entityPosition.Z + playerBoundingBox.Minimum.Z);
                    break;
                case ItemOrientation.East:
                    L = entityTesting.WorldBBox.Maximum.X - entityTesting.WorldBBox.Minimum.X;
                    posi = entityTesting.WorldBBox.Maximum.X - (entityPosition.X + playerBoundingBox.Minimum.X);
                    break;
                case ItemOrientation.West:
                    L = entityTesting.WorldBBox.Maximum.X - entityTesting.WorldBBox.Minimum.X;
                    posi = (entityPosition.X + playerBoundingBox.Maximum.X) - entityTesting.WorldBBox.Minimum.X;
                    break;
                default:
                    break;
            }

            posi = posi / L;
            float Y = posi * H;
            Y = Math.Min(Math.Max(Y, 0), 1);

            //Apply only if new Y is >= Current Y
            if (entityTesting.WorldBBox.Minimum.Y + Y > newPosition2Evaluate.Y)
            {
                if (((entityTesting.WorldBBox.Minimum.Y + Y) - newPosition2Evaluate.Y) < 0.3f)
                {
                    newPosition2Evaluate.Y = entityTesting.WorldBBox.Minimum.Y + Y;
                    previousPosition.Y = newPosition2Evaluate.Y;

                    physicSimu.OnGround = true;
                    physicSimu.AllowJumping = true;

                    return false;
                }
                else
                {
                    //Colliding with a slope High slide face
                    //newPosition2Evaluate = previousPosition;
                    return true;
                }
            }
            else
            {
                physicSimu.AllowJumping = true;
                return false;
            }

        }

        private void BoundingBoxCollision(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox entityBoundingBox, ref BoundingBox boundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            Vector3D newPositionWithColliding = previousPosition;

            newPositionWithColliding.Y = newPosition2Evaluate.Y;
            boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (entityTesting.WorldBBox.Intersects(ref boundingBox2Evaluate))
            {
                //If falling
                if (newPositionWithColliding.Y <= previousPosition.Y)
                {
                    newPositionWithColliding.Y = entityTesting.WorldBBox.Maximum.Y; //previousPosition.Y;
                }
                else
                {
                    newPositionWithColliding.Y = previousPosition.Y;
                }
                previousPosition.Y = newPositionWithColliding.Y;
                _onEntityTop = true;
            }

            newPositionWithColliding.X = newPosition2Evaluate.X;
            boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (entityTesting.WorldBBox.Intersects(ref boundingBox2Evaluate, 0.001f))
            {
                newPositionWithColliding.X = previousPosition.X;
                _onEntityTop = false;
            }

            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            boundingBox2Evaluate = new BoundingBox(entityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), entityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (entityTesting.WorldBBox.Intersects(ref boundingBox2Evaluate, 0.001f))
            {
                newPositionWithColliding.Z = previousPosition.Z;
                _onEntityTop = false;
            }

            //Set the NEW player position after collision tests
            newPosition2Evaluate = newPositionWithColliding;

            // ? I'm on "TOP" of an object ???
            if (_onEntityTop == true)
            {
                physicSimu.OnGround = true;
            }

            if (entityTesting.Entity is IDynamicEntity)
            {
                //Send an impulse message to the Entity, following my "LookAtVector" !
                float impulsePower = 1;
                if (_input.ActionsManager.isTriggered(UtopiaActions.Move_Run)) impulsePower = 2;

                _server.ServerConnection.Send(new EntityImpulseMessage
                {
                    DynamicEntityId = (entityTesting.Entity as IDynamicEntity).DynamicId,
                    Vector3 = MQuaternion.GetLookAtFromQuaternion(_player.Player.HeadRotation) * impulsePower
                }
                );
            }
        }
        
        #endregion

    }
}
