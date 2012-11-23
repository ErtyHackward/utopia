using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Models;

namespace Utopia.Entities
{
    public class VisualDynamicEntity : IVisualVoxelEntityContainer, IDisposable
    {
        #region Private variables
        //Server interpolated variables
        private NetworkValue<Vector3D> _netLocation;
        private double _interpolationRate = 0.035;
        private int _distanceLimit = 3;
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public readonly IDynamicEntity DynamicEntity;
        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public VisualVoxelEntity VisualVoxelEntity { get; set; }

        public FTSValue<Vector3D> WorldPosition = new FTSValue<Vector3D>();         //World Position
        public FTSValue<Quaternion> LookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        public FTSValue<Quaternion> MoveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)

        public FTSValue<Color3> ModelLight = new FTSValue<Color3>();

        public VoxelModelInstance ModelInstance { get; set; }

        public bool WithNetworkInterpolation { get; set; }

        private bool _walking = false;

        #endregion

        public VisualDynamicEntity(IDynamicEntity dynamicEntity, VisualVoxelEntity visualEntity)
        {
            this.DynamicEntity = dynamicEntity;
            this.VisualVoxelEntity = visualEntity;
            
            Initialize();
        }

        public void Dispose()
        {
            VisualVoxelEntity.Dispose();
        }

        #region Private Methods
        private void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            VisualVoxelEntity.LocalBBox.Minimum = new Vector3(-(DynamicEntity.DefaultSize.X / 2.0f), 0, -(DynamicEntity.DefaultSize.Z / 2.0f));
            VisualVoxelEntity.LocalBBox.Maximum = new Vector3(+(DynamicEntity.DefaultSize.X / 2.0f), DynamicEntity.DefaultSize.Y, +(DynamicEntity.DefaultSize.Z / 2.0f));

            //Set Position
            //Set the entity world position following the position received from server
            WorldPosition.Value = DynamicEntity.Position;
            WorldPosition.ValuePrev = DynamicEntity.Position;

            //Compute the initial Player world bounding box
            VisualVoxelEntity.RefreshWorldBoundingBox(ref WorldPosition.Value);

            //Set LookAt
            LookAtDirection.Value = DynamicEntity.HeadRotation;
            LookAtDirection.ValuePrev = LookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            MoveDirection.Value = LookAtDirection.Value;

            //Change the default value when Player => The player message arrive much more faster !
            if (DynamicEntity.ClassId == EntityClassId.PlayerCharacter)
            {
                _interpolationRate = 0.1;
                _distanceLimit = 5;
            }

            _netLocation = new NetworkValue<Vector3D>() { Value = WorldPosition.Value, Interpolated = WorldPosition.Value };

            WithNetworkInterpolation = true;
        }

        private void RefreshEntityMovementAndRotation()
        {

            var moveDirection = DynamicEntity.Position - _netLocation.Value;
            moveDirection.Normalize();

            var moveQuaternion = Quaternion.RotationMatrix(Matrix.LookAtLH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + moveDirection.AsVector3(), Vector3D.Up.AsVector3()));

            // leave only Y-rotation
            moveQuaternion.X = 0;
            moveQuaternion.Z = 0;
            moveQuaternion.Normalize();
            
            //Derived BodyRotation from HeadRotation
            DynamicEntity.BodyRotation = Quaternion.Lerp(DynamicEntity.BodyRotation, moveQuaternion, (float)Vector3D.Distance(DynamicEntity.Position, _netLocation.Value));

            _netLocation.Value = DynamicEntity.Position;

            //CheckWalkingAnimation(ref _netLocation.Value, ref _netLocation.Interpolated, 0.06);

            Networkinterpolation();
        }

        private void CheckWalkingAnimation(ref Vector3D previousPosition, ref Vector3D currentPosition, double threshold)
        {
            var distanceSquared = Vector3D.DistanceSquared(previousPosition, currentPosition);

            //Activate / deactivate Model Playing animation
            if (_walking && distanceSquared < threshold)
            {
                if (ModelInstance != null) 
                    ModelInstance.Stop();
                _walking = false;
            }

            if (!_walking && distanceSquared >= threshold)
            {
                if (ModelInstance != null) 
                    ModelInstance.TryPlay("Walk", true);
                _walking = true;
            }
        }

        private void Networkinterpolation()
        {
            //Interpolate received world position.
            WorldPosition.BackUpValue();

            _netLocation.DeltaValue = _netLocation.Value - _netLocation.Interpolated;
            _netLocation.Distance = _netLocation.DeltaValue.Length();
            if (_netLocation.Distance > _distanceLimit)
            {
                    _netLocation.Interpolated = _netLocation.Value;

                    //Refresh World Entity bounding box - only if entity did move !
                    VisualVoxelEntity.RefreshWorldBoundingBox(ref _netLocation.Interpolated);
            }
            else
                if (_netLocation.Distance > 0.1)
                {
                    _netLocation.Interpolated += _netLocation.DeltaValue * _interpolationRate;

                    //Refresh World Entity bounding box - only if entity did move !
                    VisualVoxelEntity.RefreshWorldBoundingBox(ref _netLocation.Interpolated);
                }

            WorldPosition.Value = _netLocation.Interpolated;
        }
        #endregion

        #region Public Methods
        public void Update(GameTime timeSpent)
        {
            LookAtDirection.BackUpValue();
            WorldPosition.BackUpValue();
            MoveDirection.BackUpValue();

            LookAtDirection.Value = DynamicEntity.HeadRotation;

            if (WithNetworkInterpolation)
            {
                RefreshEntityMovementAndRotation();
                MoveDirection.Value = DynamicEntity.BodyRotation;
            }
            else
            {
                WorldPosition.Value = DynamicEntity.Position;
                MoveDirection.Value = DynamicEntity.BodyRotation;
            }

            CheckWalkingAnimation(ref WorldPosition.ValuePrev, ref WorldPosition.Value, 0.004);
        }

        //Draw interpolation (Before each Drawing)
        public void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            Quaternion.Slerp(ref LookAtDirection.ValuePrev, ref LookAtDirection.Value, interpolationLd, out LookAtDirection.ValueInterp);
            Quaternion.Slerp(ref MoveDirection.ValuePrev, ref MoveDirection.Value, interpolationLd, out MoveDirection.ValueInterp);
            Vector3D.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolationHd, out WorldPosition.ValueInterp);

            if (ModelInstance != null)
            {
                ModelInstance.HeadRotation = Quaternion.Invert(LookAtDirection.ValueInterp);
                ModelInstance.Rotation = Quaternion.Invert(MoveDirection.ValueInterp);
                ModelInstance.Interpolation(timePassed);
            }
        }
        #endregion
    }
}
