using System.Collections.Generic;
using System.IO;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a model parts layout
    /// </summary>
    public class VoxelModelState : IBinaryStorable
    {
        private readonly VoxelModel _parentModel;

        /// <summary>
        /// Gets or sets the state name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Corresponding array for voxel model parts
        /// </summary>
        public List<VoxelModelPartState> PartsStates { get; private set; }

        /// <summary>
        /// Gets or sets current state bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Initialize a copy of the state
        /// </summary>
        /// <param name="copyFrom"></param>
        public VoxelModelState(VoxelModelState copyFrom)
        {
            PartsStates = new List<VoxelModelPartState>();
            _parentModel = copyFrom._parentModel;
            Name = copyFrom.Name;
            BoundingBox = copyFrom.BoundingBox;
            for (int i = 0; i < _parentModel.Parts.Count; i++)
            {
                PartsStates.Add(new VoxelModelPartState(copyFrom.PartsStates[i]));
            }

        }

        public VoxelModelState(VoxelModel parentModel)
        {
            PartsStates = new List<VoxelModelPartState>();

            _parentModel = parentModel;
            
            for (int i = 0; i < _parentModel.Parts.Count; i++)
            {
                PartsStates.Add(new VoxelModelPartState() );
            }

        }

        public void Save(BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Name))
                Name = "unnamed";
            writer.Write(Name);
            writer.Write((byte)PartsStates.Count);
            foreach (var voxelModelPartState in PartsStates)
            {
                voxelModelPartState.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            var count = reader.ReadByte();
            PartsStates.Clear();

            for (int i = 0; i < count; i++)
            {
                var partState = new VoxelModelPartState();
                partState.Load(reader);
                PartsStates.Add(partState);
            }
        }
        
        //Update internal BoundingBox
        public void UpdateBoundingBox()
        {
            if (PartsStates.Count == 0) 
                return;
            
            // calculate bounding boxes for each part state
            for (int i = 0; i < PartsStates.Count; i++)
            {
                var partState = PartsStates[i];

                if (partState.ActiveFrame == byte.MaxValue)
                    continue;

                var bb = new BoundingBox(new Vector3(), _parentModel.Parts[i].Frames[partState.ActiveFrame].BlockData.ChunkSize);
                
                bb = bb.Transform(partState.GetTransformation());

                if (i == 0) BoundingBox = bb;

                partState.BoundingBox = bb;

                BoundingBox = BoundingBox.Merge(BoundingBox, bb);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}