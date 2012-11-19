using System;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// VisualVoxelEntity is a pure client side class that wraps a IVoxelEntity.
    /// it has visualVoxelModel instance  
    /// </summary>
    public class VisualVoxelEntity : VisualEntity, IDisposable
    {
        private readonly IVoxelEntity _voxelEntity;
        private readonly VoxelModelManager _manager;
        private VisualVoxelModel _visualVoxelModel;

        /// <summary>
        /// Gets wrapped VoxelEntity
        /// </summary>
        public IVoxelEntity VoxelEntity { get { return _voxelEntity; } }

        /// <summary>
        /// Gets current visualmodel instance
        /// </summary>
        public VisualVoxelModel VisualVoxelModel
        {
            get { return _visualVoxelModel; }
        }

        /// <summary>
        /// Creates a VisualVoxelEntity ready to render
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="wrapped">wrapped VoxelEntity from server</param>
        public VisualVoxelEntity(IVoxelEntity wrapped, VoxelModelManager manager)
            : base(wrapped.DefaultSize, wrapped)
        {
            _voxelEntity = wrapped;
            _manager = manager;

            //Create the world position of the Voxel Entity, based on its initial position

            if (wrapped.ModelInstance == null)
                return;

            var model = manager.GetModel(wrapped.ModelName);

            // set the model or wait for it
            if (model == null)
                _manager.VoxelModelAvailable += ManagerVoxelModelReceived;
            else
            {
                _visualVoxelModel = model;
                if (wrapped.DefaultSize == Vector3.Zero) //No size define, use the default one
                {
                    BoundingBox voxelModelBB = _voxelEntity.ModelInstance.State.BoundingBox;

                    if (voxelModelBB != null)
                    {
                        LocalBBox = new BoundingBox(voxelModelBB.Minimum / 16, voxelModelBB.Maximum / 16);

                        //Add instance rotation, if existing
                        if (Entity is IStaticEntity)
                        {
                            LocalBBox = LocalBBox.Transform(Matrix.RotationQuaternion(((IStaticEntity)Entity).Rotation));
                        }

                        ComputeWorldBoundingBox(Entity.Position, out WorldBBox);
                    }
                }
            }
        }

        void ManagerVoxelModelReceived(object sender, VoxelModelReceivedEventArgs e)
        {
            // our model just downloaded, set it
            if (e.Model.Name == _voxelEntity.ModelName)
            {
                _visualVoxelModel = _manager.GetModel(e.Model.Name);
                _manager.VoxelModelAvailable -= ManagerVoxelModelReceived;
                if (_voxelEntity.DefaultSize == Vector3.Zero && _visualVoxelModel.VoxelModel.States[0].BoundingBox != null)
                {
                    SetEntityVoxelBB(_visualVoxelModel.VoxelModel.States[0].BoundingBox);
                }
            }
        }
        
        public void Dispose()
        {

        }    
    }
}