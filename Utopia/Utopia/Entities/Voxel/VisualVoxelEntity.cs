using System;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// VisualEntiy is a pure client side class that wraps a VoxelEntity.
    /// it has visualVoxelModel instance  
    /// </summary>
    public class VisualVoxelEntity : VisualEntity, IDisposable
    {
        private readonly IVoxelEntity _voxelEntity;
        private readonly VoxelModelManager _manager;
        private VisualVoxelModel _visualVoxelModel;
        public Matrix World;

        /// <summary>
        /// Gets wrapped VoxelEntity
        /// </summary>
        public IVoxelEntity VoxelEntity { get { return _voxelEntity; } }

        /// <summary>
        /// Gets or sets wrapped entity position
        /// </summary>
        public Vector3D Position
        {
            get { return _voxelEntity.Position; }
            set { _voxelEntity.Position = value; }
        }
        
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

            World = Matrix.Translation(wrapped.Position.AsVector3());

            if (wrapped.ModelInstance == null)
                return;

            var model = manager.GetModel(wrapped.ModelName);

            // set the model or wait for it
            if (model == null)
                _manager.VoxelModelAvailable += ManagerVoxelModelReceived;
            else
            {
                _visualVoxelModel = model;
                //Set local BB from the model
                if (_voxelEntity.DefaultSize == Vector3.Zero && _visualVoxelModel.VoxelModel.States[0].BoundingBox != null)
                {
                    SetEntityVoxelBB(_visualVoxelModel.VoxelModel.States[0].BoundingBox);
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