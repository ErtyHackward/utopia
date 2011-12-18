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
        public VoxelModelPartState[] PartsStates;

        /// <summary>
        /// Gets or sets current state bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        public VoxelModelState()
        {
            
        }

        public VoxelModelState(VoxelModel parentModel)
        {
            _parentModel = parentModel;
            PartsStates = new VoxelModelPartState[_parentModel.Parts.Count];

            for (int i = 0; i < PartsStates.Length; i++)
            {
                PartsStates[i] = new VoxelModelPartState();
            }

        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)PartsStates.Length);
            foreach (var voxelModelPartState in PartsStates)
            {
                voxelModelPartState.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            var count = reader.ReadByte();
            PartsStates = new VoxelModelPartState[count];

            for (int i = 0; i < count; i++)
            {
                var partState = new VoxelModelPartState();
                partState.Load(reader);
                PartsStates[i] = partState;
            }
        }
    }
}