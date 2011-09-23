using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Interfaces;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Voxel;

namespace Utopia.Entities
{
    public class VisualDynamicEntity : IVisualEntityContainer, IDisposable
    {
        #region Private variables
        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private BoundingBox _playerBoundingBox;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;                         //Use to recompute the bounding box in world coordinate

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
        public VisualEntity VisualEntity { get; set; }

        public FTSValue<Vector3D> WorldPosition = new FTSValue<Vector3D>();         //World Position
        public FTSValue<Quaternion> LookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        public FTSValue<Quaternion> MoveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        #endregion

        public VisualDynamicEntity(IDynamicEntity dynamicEntity, VisualEntity visualEntity)
        {
            this.DynamicEntity = dynamicEntity;
            this.VisualEntity = visualEntity;

            Initialize();
        }

        public void Dispose()
        {
            VisualEntity.Dispose();
        }

        #region Private Methods
        private void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(DynamicEntity.Size.X / 2.0f), 0, -(DynamicEntity.Size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(DynamicEntity.Size.X / 2.0f), DynamicEntity.Size.Y, +(DynamicEntity.Size.Z / 2.0f));

            //Compute the initial Player world bounding box
            RefreshBoundingBox(ref WorldPosition.Value, out _playerBoundingBox);

            //Set Position
            //Set the entity world position following the position received from server
            WorldPosition.Value = DynamicEntity.Position;
            WorldPosition.ValuePrev = DynamicEntity.Position;

            //Set LookAt
            LookAtDirection.Value = DynamicEntity.Rotation;
            LookAtDirection.ValuePrev = LookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            MoveDirection.Value = LookAtDirection.Value;

            //Change the default value when Player => The player message arrive much more faster !
            if (DynamicEntity.ClassId == Shared.Chunks.Entities.EntityClassId.PlayerCharacter)
            {
                _interpolationRate = 0.1;
                _distanceLimit = 5;
            }

            _netLocation = new NetworkValue<Vector3D>() { Value = WorldPosition.Value, Interpolated = WorldPosition.Value };
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        private void RefreshBoundingBox(ref Vector3D worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

        private void RefreshEntityMovementAndRotation()
        {
            LookAtDirection.BackUpValue();

            _netLocation.Value = DynamicEntity.Position;
            LookAtDirection.Value = DynamicEntity.Rotation;

            Networkinterpolation();
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
            }
            else
                if (_netLocation.Distance > 0.1)
                {
                    _netLocation.Interpolated += _netLocation.DeltaValue * _interpolationRate;
                }

            WorldPosition.Value = _netLocation.Interpolated;
        }

        #endregion

        #region Public Methods
        public void Update(ref GameTime timeSpent)
        {
            RefreshEntityMovementAndRotation(); 
        }

        //Draw interpolation (Before each Drawing)
        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref LookAtDirection.ValuePrev, ref LookAtDirection.Value, interpolationLd, out LookAtDirection.ValueInterp);
            Vector3D.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolationHd, out WorldPosition.ValueInterp);

            //Refresh the VisualEntity World matrix based on the latest interpolated values
            Vector3 entityCenteredPosition = WorldPosition.ValueInterp.AsVector3(); //currentLocation.AsVector3();
            //entityCenteredPosition.X -= DynamicEntity.Size.X / 2;
            //entityCenteredPosition.Z -= DynamicEntity.Size.Z / 2;
            VisualEntity.World = Matrix.RotationQuaternion(LookAtDirection.ValueInterp) * Matrix.Translation(entityCenteredPosition);  //Matrix.Scaling(DynamicEntity.Size) * Matrix.Translation(entityCenteredPosition);
            //===================================================================================================================================
        }
        #endregion
    }
}
