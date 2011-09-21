using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents entites which has a voxel nature
    /// </summary>
    public abstract class VoxelEntity : Entity, IVoxelEntity
    {
        private VoxelModel _model;

        /// <summary>
        /// Occurs when entity voxel model was changed
        /// </summary>
        public event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        protected void OnVoxelModelChanged(VoxelModelEventArgs e)
        {
            var handler = VoxelModelChanged;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Gets or sets voxel entity model
        /// </summary>
        public VoxelModel Model
        {
            get { return _model; }
            set {
                if (_model != value)
                {
                    _model = value;
                    OnVoxelModelChanged(new VoxelModelEventArgs { Model = _model });
                }
            }
        }

        public VoxelEntity()
        {
            Model = new VoxelModel();
        }

        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            Model.Load(reader);
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            Model.Save(writer);
        }
    }
}
