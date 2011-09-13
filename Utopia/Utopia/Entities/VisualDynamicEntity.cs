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
    public class VisualDynamicEntity : IVisualEntityContainer
    {
        #region Private variables
        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private BoundingBox _playerBoundingBox;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;                         //Use to recompute the bounding box in world coordinate
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

        public FTSValue<DVector3> WorldPosition = new FTSValue<DVector3>();         //World Position
        public FTSValue<Quaternion> LookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        public FTSValue<Quaternion> MoveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        #endregion

        public VisualDynamicEntity(IDynamicEntity dynamicEntity, VisualEntity visualEntity)
        {
            this.DynamicEntity = dynamicEntity;
            this.VisualEntity = visualEntity;

            Initialize();
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
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        private void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

        private void RefreshEntityMovementAndRotation()
        {
            LookAtDirection.BackUpValue();
            WorldPosition.BackUpValue();

            WorldPosition.Value = DynamicEntity.Position;
            LookAtDirection.Value = DynamicEntity.Rotation;
        }

        #endregion

        #region Public Methods
        public void Update(ref GameTime timeSpent)
        {
            RefreshEntityMovementAndRotation(); 
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref LookAtDirection.ValuePrev, ref LookAtDirection.Value, interpolationLd, out LookAtDirection.ValueInterp);
            DVector3.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolationHd, out WorldPosition.ValueInterp);

            //Refresh the VisualEntity World matrix based on the latest interpolated values
            Vector3 entityCenteredPosition = WorldPosition.ValueInterp.AsVector3();
            entityCenteredPosition.X -= DynamicEntity.Size.X / 2;
            entityCenteredPosition.Z -= DynamicEntity.Size.Z / 2;
            VisualEntity.World = Matrix.Scaling(DynamicEntity.Size) * Matrix.Translation(entityCenteredPosition);
            //===================================================================================================================================
        }
        #endregion
    }
}
