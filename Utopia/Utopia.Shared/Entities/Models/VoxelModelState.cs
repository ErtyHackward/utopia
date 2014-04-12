using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a model parts layout
    /// </summary>
    [ProtoContract]
    public class VoxelModelState
    {
        private VoxelModel _parentModel;
        private BoundingBox _boundingBox;

        /// <summary>
        /// Gets or sets the state name
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Corresponding array for voxel model parts
        /// </summary>
        [ProtoMember(2)]
        public List<VoxelModelPartState> PartsStates { get; set; }

        /// <summary>
        /// Gets or sets current state bounding box
        /// </summary>
        [ProtoMember(3)]
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        public bool IsMainState
        {
            get { return _parentModel.GetMainState() == this; }
        }

        public bool IsIconState
        {
            get { return _parentModel.GetIconState() == this; }
        }

        public VoxelModel ParentModel
        {
            get { return _parentModel; }
            set { 
                _parentModel = value;

                if (PartsStates.Count != _parentModel.Parts.Count)
                {
                    for (int i = 0; i < _parentModel.Parts.Count; i++)
                    {
                        PartsStates.Add(new VoxelModelPartState());
                    }
                }
            }
        }

        public VoxelModelState()
        {
            PartsStates = new List<VoxelModelPartState>();
        }

        /// <summary>
        /// Initialize a copy of the state
        /// </summary>
        /// <param name="copyFrom"></param>
        public VoxelModelState(VoxelModelState copyFrom) : this()
        {
            _parentModel = copyFrom._parentModel;
            Name = copyFrom.Name;
            BoundingBox = copyFrom.BoundingBox;
            for (int i = 0; i < _parentModel.Parts.Count; i++)
            {
                PartsStates.Add(new VoxelModelPartState(copyFrom.PartsStates[i]));
            }

        }

        public VoxelModelState(VoxelModel parentModel) : this()
        {
            ParentModel = parentModel;
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

                var frame = _parentModel.Frames[partState.ActiveFrame];

                var bb = new BoundingBox(new Vector3(), frame.BlockData.ChunkSize);
                
                bb = bb.Transform(partState.GetTransformation());

                if (i == 0) BoundingBox = bb;

                partState.BoundingBox = bb;

                BoundingBox.Merge(ref _boundingBox, ref bb, out _boundingBox);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}