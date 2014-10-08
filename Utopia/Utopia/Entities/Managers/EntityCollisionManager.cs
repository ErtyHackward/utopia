using System;
using System.Collections.Generic;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Network;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks;
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
    /// The Aim of this class is to check the player entity collissions
    /// It will need a collection of entities that are "Near" the player, in order to test the collision against as less entities as possible !
    /// </summary>
    public class EntityCollissonManager : IEntityCollisionManager, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private members
        private IVisualDynamicEntityManager _dynamicEntityManager;
        //private TimerManager.GameTimer _timer;
        private readonly List<VisualEntity> _entitiesNearPlayer = new List<VisualEntity>(1000);
        private PlayerEntityManager _player;
        private readonly int _entityDistance = AbstractChunk.ChunkSize.X * 2;
        private readonly ServerComponent _server;
        private readonly InputsManager _input;
        private IWorldChunks _worldChunks;
        private bool? _onEntityTop;
        #endregion

        #region Public properties

        public bool IsDirty { get; set; }

        #endregion

        #region DI

        [Inject]
        public PlayerEntityManager Player
        {
            get { return _player; }
            set { _player = value; }
        }

        [Inject]
        public IWorldChunks WorldChunks
        {
            get { return _worldChunks; }
            set { _worldChunks = value; }
        }
        
        [Inject]
        public IVisualDynamicEntityManager DynamicEntityManager
        {
            get { return _dynamicEntityManager; }
            set { _dynamicEntityManager = value; }
        }

        #endregion

        public EntityCollissonManager(ServerComponent server,
                                      InputsManager input)                                     
        {
            _input = input;
            _server = server;            
        }

        public void Dispose()
        {
        }

        #region private methods
        //Started everyseconds
        private void _timer_OnTimerRaised()
        {
            CollectsurroundingDynamicPlayerEntities(); //They have their own collection
            CollectsurroundingStaticEntities();  //They are stored inside chunks !
            IsDirty = false;
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
                    if (Vector3D.Distance(entity.VoxelEntity.Position, _player.Player.Position) <= _entityDistance)
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

        private void CollectsurroundingStaticEntities()
        {
            if (_worldChunks.SortedChunks == null) return;

            VisualChunk chunk;
            //Check inside the visible chunks (Not visible culled) the statics entities
            //Chunk are sorted around player, the 9 first are the chunk around the players.
            for (int i = 0; i < 9; i++)
            {
                chunk = _worldChunks.SortedChunks[i];
                foreach (var entity in chunk.AllEntities())
                {
                    //Add entity only if at <= 10 block distance !
                    if (Vector3D.Distance(entity.Entity.Position, _player.Player.Position) <= 10)
                    {
                        _entitiesNearPlayer.Add(entity);
                    }
                }
            }
        }
        #endregion

        #region public methods
        
        public void Update()
        {
            _timer_OnTimerRaised();
        }

        //Entity vs Player Collision detection
        //Used by physics engine
        public void IsCollidingWithEntity(VerletSimulator physicSimu, ref BoundingBox playerBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ref Vector3D originalPosition)
        {
            if (IsDirty) 
                _timer_OnTimerRaised();

            bool isSliding = false;
            for (int i = 0; i < _entitiesNearPlayer.Count; i++)
            {
                var entityTesting = _entitiesNearPlayer[i];

                if (entityTesting.Entity.IsPlayerCollidable)
                {
                    //Compute the New world located player bounding box, that will be use for collision detection
                    var playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPosition2Evaluate.AsVector3(), playerBoundingBox.Maximum + newPosition2Evaluate.AsVector3());
                    bool isEntityOnSliding;
                    CollisionCheck(physicSimu, entityTesting, ref playerBoundingBox, ref playerBoundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition, out isEntityOnSliding);
                    isSliding |= isEntityOnSliding;
                }
            }

            physicSimu.IsSliding = isSliding;
        }

        private void CollisionCheck(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox entityBoundingBox, ref BoundingBox boundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, out bool isSliding)
        {
            isSliding = false;

            if (entityTesting.WorldBBox.Intersects(ref boundingBox2Evaluate))
            {
                if (entityTesting.Entity is Ladder)
                {
                    physicSimu.isInContactWithLadder = true;
                    return;
                }

                //Special treatment in case of an IOrientedSlope entity
                if (entityTesting.Entity is IOrientedSlope)
                {
                    IOrientedSlope entity = (IOrientedSlope)entityTesting.Entity;
                    if (entity.IsOrientedSlope)
                    {
                        SlopeCollisionDetection(physicSimu, entityTesting, ref entityBoundingBox, ref boundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition, entity.Orientation, entity.IsSlidingSlope, out isSliding);
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
                Player.YForceApplying = entityTesting.Entity.YForceOnSideHit;

            }

            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            playerBoundingBox2Evaluate = new BoundingBox(playerBoundingBox.Minimum + newPositionWithColliding.AsVector3(), playerBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (IsCollidingWithModel(entityTesting, playerBoundingBox2Evaluate))
            {
                //logger.Debug("ModelCollisionDetection Z detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.Z, previousPosition.Z);

                newPositionWithColliding.Z = previousPosition.Z;
                _onEntityTop = false;
                Player.YForceApplying = entityTesting.Entity.YForceOnSideHit;
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

        private bool IsCollidingWithModel(VisualEntity entityTesting, BoundingBox playerBoundingBox2Evaluate)
        {
            var visualVoxelEntity = entityTesting as VisualVoxelEntity;
            if (visualVoxelEntity == null) 
                return false;

            var instance = visualVoxelEntity.VoxelEntity.ModelInstance;

            if (instance == null)
                return false;

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


        private void SlopeCollisionDetection(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox playerBoundingBox, ref BoundingBox playerBoundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ItemOrientation slopeOrientation, bool OnSlidingSlope, out bool isSliding)
        {
            if (IsSlopeCollisionDetection(physicSimu, entityTesting, ref playerBoundingBox, ref playerBoundingBox2Evaluate, ref newPosition2Evaluate, ref previousPosition, slopeOrientation, OnSlidingSlope, out isSliding))
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

        private bool IsSlopeCollisionDetection(VerletSimulator physicSimu, VisualEntity entityTesting, ref BoundingBox playerBoundingBox, ref BoundingBox playerBoundingBox2Evaluate, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ItemOrientation slopeOrientation, bool onSlidingSlope, out bool isSliding)
        {
            isSliding = false;

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

            float posiOriginal = posi;
            posi = posi / L;
            float Y = posi * H;
            Y = Math.Min(Math.Max(Y, 0), 1);

            if (onSlidingSlope)
            {
                return SlidingSlope(physicSimu, entityTesting, Y, ref newPosition2Evaluate, ref previousPosition, slopeOrientation, out isSliding);
            }

            //Apply only if new Y is >= Current Y
            if (entityTesting.WorldBBox.Minimum.Y + Y > newPosition2Evaluate.Y)
            {
                return NormalSlope(physicSimu, entityTesting, Y, ref newPosition2Evaluate, ref previousPosition);
            }
            else
            {
                physicSimu.AllowJumping = true;
                return false;
            }

        }

        private bool NormalSlope(VerletSimulator physicSimu, VisualEntity entityTesting, float Y, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            if (((entityTesting.WorldBBox.Minimum.Y + Y) - newPosition2Evaluate.Y) < 0.3f)
            {
                //Push up the player, and stabilize its physic simulation
                newPosition2Evaluate.Y = entityTesting.WorldBBox.Minimum.Y + Y;
                previousPosition.Y = newPosition2Evaluate.Y;

                physicSimu.OnGround = true;
                physicSimu.AllowJumping = true;
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool SlidingSlope(VerletSimulator physicSimu, VisualEntity entityTesting, float Y, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ItemOrientation slopeOrientation, out bool isSliding)
        {
            isSliding = false;

            if (entityTesting.WorldBBox.Minimum.Y + Y >= newPosition2Evaluate.Y)
            {
                if (((entityTesting.WorldBBox.Minimum.Y + Y) - newPosition2Evaluate.Y) < 0.3f)
                {                    
                    Y = Y - (float)(newPosition2Evaluate.Y - Math.Floor(newPosition2Evaluate.Y));

                    if (Y < 0.001 || Y >= 0.999)
                    {
                        Y = 0.02f;
                    }

                    newPosition2Evaluate.Y += entityTesting.Entity.Friction;

                    isSliding = true;

                    switch (slopeOrientation)
                    {
                        case ItemOrientation.North:
                            newPosition2Evaluate.Z = newPosition2Evaluate.Z - Y;
                            if (physicSimu.IsSliding == false)
                            {
                                physicSimu.SliddingForce = new Vector3(0,0,-1);
                            }
                            break;
                        case ItemOrientation.South:

                            newPosition2Evaluate.Z = newPosition2Evaluate.Z + Y;

                            if (physicSimu.IsSliding == false)
                            {
                                physicSimu.SliddingForce = new Vector3(0, 0, 1);
                            }

                            break;
                        case ItemOrientation.East:
                            newPosition2Evaluate.X = newPosition2Evaluate.X + Y;

                            if (physicSimu.IsSliding == false)
                            {
                                physicSimu.SliddingForce = new Vector3(1, 0, 0);
                            }

                            break;
                        case ItemOrientation.West:
                            newPosition2Evaluate.X = newPosition2Evaluate.X - Y;

                            if (physicSimu.IsSliding == false)
                            {
                                physicSimu.SliddingForce = new Vector3(-1, 0, 0);
                            }

                            break;
                        default:
                            break;
                    }

                    return false;
                }
                else
                {
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
