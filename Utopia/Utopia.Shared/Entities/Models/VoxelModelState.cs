using System.Collections.Generic;
using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a model parts layout
    /// </summary>
    public class VoxelModelState : IBinaryStorable
    {
        private readonly VoxelModel _parentModel;

        /// <summary>
        /// Corresponding array for voxel model parts
        /// </summary>
        public List<VoxelModelPartState> PartsStates { get; private set; }

        /// <summary>
        /// Gets or sets current state bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        public VoxelModelState()
        {
            PartsStates = new List<VoxelModelPartState>();
        }

        public VoxelModelState(VoxelModel parentModel)
        {
            PartsStates = new List<VoxelModelPartState>();

            _parentModel = parentModel;
            
            for (int i = 0; i < _parentModel.Parts.Count; i++)
            {
                PartsStates.Add(new VoxelModelPartState());
            }

        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)PartsStates.Count);
            foreach (var voxelModelPartState in PartsStates)
            {
                voxelModelPartState.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            var count = reader.ReadByte();
            PartsStates.Clear();

            for (int i = 0; i < count; i++)
            {
                var partState = new VoxelModelPartState();
                partState.Load(reader);
                PartsStates.Add(partState);
            }
        }
    }
}