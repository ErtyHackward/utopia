using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Models;

namespace Utopia.Entities
{
    /// <summary>
    /// Provides interpolation possibility to dynamicEntities
    /// </summary>
    public class VisualDynamicEntity : VisualVoxelEntity
    {
        #region Private variables
        //Server interpolated variables
        private NetworkValue<Vector3D> _netLocation;
        private double _interpolationRate = 0.035;
        private bool _moving;
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public ICharacterEntity DynamicEntity;
        
        public FTSValue<Vector3D>   WorldPosition   = new FTSValue<Vector3D>();     //World Position
        public FTSValue<Quaternion> LookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        public FTSValue<Quaternion> MoveDirection   = new FTSValue<Quaternion>();   //Real move direction (derived from LookAt, but will depend the mode !)

        public FTSValue<Color3> ModelLight = new FTSValue<Color3>();

        public VoxelModelInstance ModelInstance { get { return VoxelEntity.ModelInstance; } }

        public bool WithNetworkInterpolation { get; set; }
        
        #endregion

        public VisualDynamicEntity(ICharacterEntity dynEntity, VoxelModelManager manager)
            : base(dynEntity, manager)
        {
            DynamicEntity = dynEntity;

            //Will be used to update the bounding box with world coordinate when the entity is moving
            LocalBBox.Minimum = new Vector3(-(DynamicEntity.DefaultSize.X / 2.0f), 0,                           -(DynamicEntity.DefaultSize.Z / 2.0f));
            LocalBBox.Maximum = new Vector3(+(DynamicEntity.DefaultSize.X / 2.0f), DynamicEntity.DefaultSize.Y, +(DynamicEntity.DefaultSize.Z / 2.0f));

            //Set Position
            //Set the entity world position following the position received from server
            WorldPosition.Value     = DynamicEntity.Position;
            WorldPosition.ValuePrev = DynamicEntity.Position;

            //Compute the initial Player world bounding box
            RefreshWorldBoundingBox(ref WorldPosition.Value);

            //Set LookAt
            LookAtDirection.Value = DynamicEntity.HeadRotation;
            LookAtDirection.ValuePrev = LookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            MoveDirection.Value = LookAtDirection.Value;

            //Change the default value when Player => The player message arrive much more faster !
            if (DynamicEntity is PlayerCharacter)
            {
                _interpolationRate = 0.1;
            }

            _netLocation = new NetworkValue<Vector3D> { Value = WorldPosition.Value, Interpolated = WorldPosition.Value };

            WithNetworkInterpolation = true;
        }

        #region Private Methods

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

        private void CheckMovingAnimation(ref Vector3D previousPosition, ref Vector3D currentPosition, double threshold)
        {
            var distanceSquared = Vector3D.DistanceSquared(previousPosition, currentPosition);
            string animationFrameName;
            switch (this.DynamicEntity.DisplacementMode)
            {
                case EntityDisplacementModes.Flying:
                case EntityDisplacementModes.Dead:
                    animationFrameName = "Flying";
                    break;
                case EntityDisplacementModes.Walking:
                    animationFrameName = "Walk";
                    break;
                case EntityDisplacementModes.Swiming:
                    animationFrameName = "Walk";
                    break;
                case EntityDisplacementModes.FreeFlying:
                    animationFrameName = "Flying";
                    break;
                case EntityDisplacementModes.God:
                    animationFrameName = "Flying";
                    break;
                default:
                    animationFrameName = "Walk";
                    break;
            }

            //Activate / deactivate Model Playing animation
            if (_moving && distanceSquared < threshold)
            {
                if (ModelInstance != null)
                    ModelInstance.Stop(animationFrameName);
                _moving = false;
            }

            if (!_moving && distanceSquared >= threshold)
            {
                if (ModelInstance != null)
                    ModelInstance.TryPlay(animationFrameName, true);
                _moving = true;
            }
        }

        private void Networkinterpolation()
        {
            //Interpolate received world position.
            WorldPosition.BackUpValue();

            _netLocation.DeltaValue = _netLocation.Value - _netLocation.Interpolated;
            _netLocation.Distance = _netLocation.DeltaValue.Length();
            if (_netLocation.Distance > 0.1)
            {
                _netLocation.Interpolated += _netLocation.DeltaValue * _interpolationRate;

                //Refresh World Entity bounding box - only if entity did move !
                RefreshWorldBoundingBox(ref _netLocation.Interpolated);
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

            CheckMovingAnimation(ref WorldPosition.ValuePrev, ref WorldPosition.Value, 0.0001);
        }

        //Draw interpolation (Before each Drawing)
        public void Interpolation(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            Quaternion.Slerp(ref LookAtDirection.ValuePrev, ref LookAtDirection.Value, interpolationLd, out LookAtDirection.ValueInterp);
            Quaternion.Slerp(ref MoveDirection.ValuePrev, ref MoveDirection.Value, interpolationLd, out MoveDirection.ValueInterp);
            Vector3D.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolationHd, out WorldPosition.ValueInterp);

            if (ModelInstance != null)
            {
                ModelInstance.HeadRotation = Quaternion.Invert(LookAtDirection.ValueInterp);
                ModelInstance.Rotation = Quaternion.Invert(MoveDirection.ValueInterp);
                ModelInstance.Interpolation(elapsedTime);
            }
        }
        #endregion
    }
}
