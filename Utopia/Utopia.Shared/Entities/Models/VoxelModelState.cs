using System;
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

        public void UpdateBoundingBox()
        {
            if (PartsStates.Count == 0) 
                return;

            var minPoint = new Vector3();
            var maxPoint = new Vector3();
            // calculate bounding boxes for each part state
            for (int i = 0; i < PartsStates.Count; i++)
            {
                var partState = PartsStates[i];
                var bb = new BoundingBox(new Vector3(), _parentModel.Parts[i].Frames[partState.ActiveFrame].BlockData.ChunkSize);

                bb.Minimum = Vector3.TransformCoordinate(bb.Minimum, partState.Transform);
                bb.Maximum = Vector3.TransformCoordinate(bb.Maximum, partState.Transform);

                partState.BoundingBox = bb;

                if (i == 0)
                {
                    minPoint = bb.Minimum;
                    maxPoint = bb.Maximum;
                }

                minPoint = Vector3.Min(minPoint, bb.Minimum);
                minPoint = Vector3.Min(minPoint, bb.Maximum);
                maxPoint = Vector3.Max(maxPoint, bb.Maximum);
                maxPoint = Vector3.Max(maxPoint, bb.Minimum);
            }

            BoundingBox = new BoundingBox(minPoint, maxPoint);
        }
    }
}