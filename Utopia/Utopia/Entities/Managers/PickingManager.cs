using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine.Debug.Interfaces;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Provides entity and block picking by the player
    /// </summary>
    public class PickingManager : GameComponent, IDebugInfo, IPickingManager
    {
        private readonly IWorldChunks _worldChunks;
        private readonly ICameraManager _cameraManager;
        private readonly InputsManager _inputsManager;
        private IPickingRenderer _pickingRenderer;
        private readonly SingleArrayChunkContainer _cubesHolder;
        private readonly WorldConfiguration _worldConfiguration;
        private IVisualDynamicEntityManager _dynamicEntityManager;
        private IPlayerManager _playerManager;

        private VisualEntity _pickedUpEntity;
        private Vector3D _pickedUpEntityPosition;

        private TerraCubeWithPosition _prevCube;

        private TerraCubeWithPosition _pickedCube;
        private TerraCubeWithPosition _newCube;

        public bool ShowDebugInfo { get; set; }

        #region DI
        [Inject]
        public IPickingRenderer PickingRenderer
        {
            get { return _pickingRenderer; }
            set { _pickingRenderer = value; }
        }


        [Inject]
        public IPlayerManager PlayerManager { 
            get { return _playerManager; }
            set { _playerManager = value; }
        }
        
        [Inject]
        public IVisualDynamicEntityManager DynamicEntityManager
        {
            get { return _dynamicEntityManager; }
            set { _dynamicEntityManager = value; }
        }
        
        #endregion

        public PickingManager(IWorldChunks worldChunks,
                              ICameraManager cameraManager,
                              InputsManager inputsManager,
                              SingleArrayChunkContainer cubesHolder,
                              WorldConfiguration worldConfiguration)
        {
            if (worldChunks == null) throw new ArgumentNullException("worldChunks");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (inputsManager == null) throw new ArgumentNullException("inputsManager");
            if (cubesHolder == null) throw new ArgumentNullException("cubesHolder");
            if (worldConfiguration == null) throw new ArgumentNullException("worldConfiguration");

            _worldChunks = worldChunks;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _worldConfiguration = worldConfiguration;

            ShowDebugInfo = true;
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            GetSelectedEntity();

            base.FTSUpdate(timeSpent);
        }
        
        /// <summary>
        /// Checks nearby entities intersection with the pickingRay
        /// </summary>
        /// <param name="pickingRay">Ray to check intersection</param>
        /// <returns></returns>
        public EntityPickResult CheckEntityPicking(Ray pickingRay)
        {
            var result = new EntityPickResult { Distance = float.MaxValue };

            var tool = _playerManager.ActiveTool;

            if (tool == null)
                return result;

            var pickedEntityDistance = tool.PickRange == 0f ? float.MaxValue : tool.PickRange;
            foreach (var entity in PossibleEntities())
            {
                if (tool.CanPickEntity(entity.Entity) == PickType.Pick)
                {
                    //Refresh entity bounding box world
                    if (entity.Entity.CollisionType == Entity.EntityCollisionType.Model) // ==> Find better interface, for all state swtiching static entities
                    {
                        if (entity.VoxelEntity.ModelInstance == null)
                            continue; 

                        var localStaticEntityBb = entity.VoxelEntity.ModelInstance.State.BoundingBox;

                        var staticEntity = entity.Entity as IStaticEntity;
                        var dynamicEntity = entity.Entity as IDynamicEntity;

                        var rotation = Quaternion.Identity;

                        if (staticEntity != null)
                        {
                            rotation = staticEntity.Rotation;
                        }
                        if (dynamicEntity != null)
                        {
                            rotation = dynamicEntity.BodyRotation;
                            rotation.Invert();
                        }


                        localStaticEntityBb = localStaticEntityBb.Transform(Matrix.RotationQuaternion(rotation));          //Rotate the BoundingBox
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

        /// <summary>
        /// Checks each part of the model for intersection
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pickRay"></param>
        /// <param name="intersectionPoint"></param>
        /// <param name="normal"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static bool ModelRayIntersection(VisualEntity entity, Ray pickRay, out Vector3 intersectionPoint, out Vector3I normal, out float dist)
        {
            intersectionPoint = new Vector3();
            normal = new Vector3I();
            dist = float.MaxValue;

            var visualVoxelEntity = entity as VisualVoxelEntity;
            if (visualVoxelEntity == null)
                return false;

            var instance = visualVoxelEntity.VoxelEntity.ModelInstance;

            bool collisionDetected = false;
            //Check Against all existing "Sub-Cube" model

            //Get current Active state = A model can have multiple "State" (Like open, close, mid open, ...)
            var activeModelState = instance.State;

            var visualModel = visualVoxelEntity.VisualVoxelModel;
            if (visualModel == null || visualModel.VoxelModel != instance.VoxelModel)
                return false;

            //For each Part in the model (A model can be composed of several parts)
            for (int partId = 0; partId < visualModel.VoxelModel.Parts.Count; partId++)
            {
                VoxelModelPartState partState = activeModelState.PartsStates[partId];

                // it is possible that there is no frame, so no need to check it
                if (partState.ActiveFrame == byte.MaxValue)
                    continue;

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

                int index = -1;
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

        /// <summary>
        /// Enumerates all possible entities for picking
        /// </summary>
        /// <returns></returns>
        private IEnumerable<VisualVoxelEntity> PossibleEntities()
        {
            foreach (var containers in DynamicEntityManager.DynamicEntities)
            {
                if (_worldChunks.IsEntityVisible(containers.Entity.Position))
                    yield return containers;
            }

            foreach (var visibleChunk in _worldChunks.VisibleChunks().Where(c => c.DistanceFromPlayer < 32))
            {
                foreach (var visualEntity in visibleChunk.AllEntities())
                {
                    if (_worldChunks.IsEntityVisible(visualEntity.Entity.Position))
                        yield return visualEntity;
                }
            }
        }

        private void GetSelectedEntity()
        {
            if (_playerManager.ActiveTool == null)
                return;

            bool newpicking;

            Vector3D mouseWorldPosition;
            Vector3D mouseLookAtPosition;

            var distance = 10f;

            var thirdPersonCamera = _cameraManager.ActiveBaseCamera as ThirdPersonCameraWithFocus;

            if (thirdPersonCamera != null)
            {
                distance = thirdPersonCamera.Distance * 2;
            }

            if (_inputsManager.MouseManager.MouseCapture)
            {
                _inputsManager.MouseManager.UnprojectMouseCursor(_cameraManager.ActiveBaseCamera, out mouseWorldPosition, out mouseLookAtPosition, true);
                newpicking = RefreshPicking(ref mouseWorldPosition, mouseLookAtPosition.AsVector3(), distance);
            }
            else
            {
                _inputsManager.MouseManager.UnprojectMouseCursor(_cameraManager.ActiveBaseCamera, out mouseWorldPosition, out mouseLookAtPosition);
                
                newpicking = RefreshPicking(ref mouseWorldPosition, mouseLookAtPosition.AsVector3(), distance);

                if (newpicking)
                {
                    if (PlayerManager.Player.EntityState.IsBlockPicked)
                    {
                        if (Vector3D.Distance(mouseWorldPosition, new Vector3D(PlayerManager.Player.EntityState.PickedBlockPosition)) > 10.0)
                        {
                            PlayerManager.Player.EntityState.IsBlockPicked = false;
                        }
                    }
                }
            }

            if (newpicking)
            {
                //A new Block has been pickedup
                if (PlayerManager.Player.EntityState.IsEntityPicked == false)
                {
                    PickingRenderer.SetPickedBlock(ref PlayerManager.Player.EntityState.PickedBlockPosition, _worldConfiguration.BlockProfiles[_pickedCube.Cube.Id].YBlockOffset);
                }
                else
                {
                    if (_cameraManager.ActiveBaseCamera.CameraType == CameraType.ThirdPerson && _pickedUpEntity.Entity == PlayerManager.Player)
                        return;
                    PickingRenderer.SetPickedEntity(_pickedUpEntity);
                }
            }
        }

        /// <summary>
        /// Update player picking
        /// </summary>
        /// <param name="pickingWorldPosition"></param>
        /// <param name="pickingLookAt"></param>
        /// <param name="blockPickingDistance"></param>
        /// <returns>return true if a new Item has been picked up !</returns>
        private bool RefreshPicking(ref Vector3D pickingWorldPosition, Vector3 pickingLookAt, float blockPickingDistance)
        {
            // first we will check entities
            // after that we will check blocks because they can be closer than the entity

            PlayerManager.Player.EntityState.IsEntityPicked = false;
            PlayerManager.Player.EntityState.IsBlockPicked = false;

            //Check the Ray against all entity first
            var pickingRay = new Ray(pickingWorldPosition.AsVector3(), pickingLookAt);

            var epr = CheckEntityPicking(pickingRay);

            if (epr.Found && epr.Distance > blockPickingDistance)
                epr.Found = false;

            var tool = PlayerManager.ActiveTool;

            if (tool.PickRange != 0f)
                blockPickingDistance = Math.Min(tool.PickRange, blockPickingDistance);

            var nbrPointToSample = (int)(Math.Min(blockPickingDistance, epr.Distance) / 0.02);

            float sliceLimitSquared = float.PositiveInfinity;

            if (_worldChunks.SliceValue != -1)
            {
                var topPlane = new Plane(new Vector3(0, _worldChunks.SliceValue, 0), Vector3.UnitY);
                var bottomPlane = new Plane(new Vector3(0, _worldChunks.SliceValue - 5, 0), Vector3.UnitY);

                Vector3 topIntersectionPoint;
                Vector3 bottomIntersectionPoint;

                var topIntersection = pickingRay.Intersects(ref topPlane, out topIntersectionPoint);
                var bottomIntersection = pickingRay.Intersects(ref bottomPlane, out bottomIntersectionPoint);
                
                if (!topIntersection && !bottomIntersection)
                    return false;

                if (topIntersection && bottomIntersection)
                {
                    // find the closest one to the camera

                    var topIsCloser = Vector3D.DistanceSquared(pickingWorldPosition, new Vector3D(topIntersectionPoint)) < Vector3D.DistanceSquared(pickingWorldPosition, new Vector3D(bottomIntersectionPoint));
                    
                    if (topIsCloser)
                    {
                        pickingWorldPosition = new Vector3D(topIntersectionPoint);
                    }
                    else
                    {
                        pickingWorldPosition = new Vector3D(bottomIntersectionPoint);
                    }

                    sliceLimitSquared = Vector3.DistanceSquared(topIntersectionPoint, bottomIntersectionPoint);
                }
                else if (topIntersection)
                {
                    sliceLimitSquared = (float)Vector3D.DistanceSquared(pickingWorldPosition, new Vector3D(topIntersectionPoint));
                }
                else
                {
                    sliceLimitSquared = (float)Vector3D.DistanceSquared(pickingWorldPosition, new Vector3D(bottomIntersectionPoint));
                }

                if (epr.Found)
                {
                    sliceLimitSquared = (float)Vector3D.DistanceSquared(pickingWorldPosition, new Vector3D(epr.PickPoint));
                    if (epr.PickPoint.Y > topIntersectionPoint.Y)
                        nbrPointToSample = 0;
                }

                //if (Vector3D.DistanceSquared(prevPosition, prevPosition) > Vector3D.DistanceSquared(prevPosition, new Vector3D(bottomPoint)))
                //    pickingWorldPosition = new Vector3D(intersectPoint);
            }

            var startPos = pickingWorldPosition;

            //Check for Cube Picking
            //Sample points in the view direction vector
            for (var ptNbr = 0; ptNbr < nbrPointToSample; ptNbr++)
            {
                pickingWorldPosition += new Vector3D(pickingLookAt * 0.02f);

                if (Vector3D.DistanceSquared(pickingWorldPosition, startPos) >= sliceLimitSquared)
                    break;

                //Check if a block is picked up !
                var result = _cubesHolder.GetCube(pickingWorldPosition);
                if (!result.IsValid)
                    break;

                var blockProfile = _cubesHolder.Config.BlockProfiles[result.Cube.Id];
                var yOffset = blockProfile.YBlockOffset;

                var pickType = tool.CanPickBlock(blockProfile);

                if (pickType == PickType.Stop)
                {
                    // we found a block that is closer than entity (if any)
                    // don't allow to pick the entity in this case
                    epr.Found = false;
                    break;
                }

                if (pickType == PickType.Pick)
                {
                    var blockPos = pickingWorldPosition.ToCubePosition();
                    PlayerManager.Player.EntityState.PickedBlockPosition = blockPos;

                    var cubeBB = new BoundingBox(blockPos, blockPos + new Vector3(1, 1f - (float)yOffset, 1));
                    Vector3 faceInteresection;
                    if (cubeBB.Intersects(ref pickingRay, out faceInteresection))
                    {
                        _prevCube = _pickedCube;
                        _pickedCube = new TerraCubeWithPosition { Position = blockPos, BlockProfile = blockProfile, Cube = result.Cube };

                        PlayerManager.Player.EntityState.PickedBlockFaceOffset = Vector3.One - (_pickedCube.Position - faceInteresection);
                        PlayerManager.Player.EntityState.PickPoint = faceInteresection;
                        PlayerManager.Player.EntityState.PickPointNormal = cubeBB.GetPointNormal(faceInteresection);
                    }

                    bool newPlacechanged = false;

                    //Find the potential new block place, by rolling back !
                    while (ptNbr > 0)
                    {
                        pickingWorldPosition -= new Vector3D(pickingLookAt * 0.02f);

                        if (_cubesHolder.isPickable(ref pickingWorldPosition, out _newCube) == false)
                        {
                            PlayerManager.Player.EntityState.NewBlockPosition = _newCube.Position;
                            newPlacechanged = true;
                            break;
                        }
                        ptNbr--;
                    }

                    PlayerManager.Player.EntityState.IsEntityPicked = false;
                    PlayerManager.Player.EntityState.IsBlockPicked = true;
                    if (_prevCube.Position == PlayerManager.Player.EntityState.PickedBlockPosition)
                    {
                        if (!newPlacechanged)
                            return false;
                    }

                    break;
                }

            }

            // we need to decide what we have picked (block or entity)
            // if we found the block this means that it is closer than entity
            // (because we used limit as the closest picked entity)

            if (!PlayerManager.Player.EntityState.IsBlockPicked && epr.Found)
            {
                _pickedUpEntity = epr.PickedEntity;
                _pickedUpEntityPosition = _pickedUpEntity.Entity.Position;

                PlayerManager.Player.EntityState.PickedEntityPosition = _pickedUpEntity.Entity.Position;
                PlayerManager.Player.EntityState.PickedEntityLink = _pickedUpEntity.Entity.GetLink();
                PlayerManager.Player.EntityState.PickPoint = epr.PickPoint;
                PlayerManager.Player.EntityState.PickPointNormal = epr.PickNormal;
                PlayerManager.Player.EntityState.IsEntityPicked = true;
                PlayerManager.Player.EntityState.IsBlockPicked = false;
            }

            return PlayerManager.Player.EntityState.IsBlockPicked || PlayerManager.Player.EntityState.IsEntityPicked;
        }

        public string GetDebugInfo()
        {
            if (PlayerManager.Player.EntityState.IsBlockPicked)
            {
                return string.Format("Block picked {0} light : {1}; New Block position {2} light : {3}", 
                                      PlayerManager.Player.EntityState.PickedBlockPosition,
                                      _cubesHolder.GetCube(PlayerManager.Player.EntityState.PickedBlockPosition).Cube.EmissiveColor.ToString(),
                                      PlayerManager.Player.EntityState.NewBlockPosition,
                                      _cubesHolder.GetCube(PlayerManager.Player.EntityState.NewBlockPosition).Cube.EmissiveColor.ToString()
                                      );
                
            }
            else if (PlayerManager.Player.EntityState.IsEntityPicked)
            {
                return string.Format("Entity picked {0}", PlayerManager.Player.EntityState.PickedEntityPosition);
            }
            else
            {
                return "";    
            }
        }
    }
}
