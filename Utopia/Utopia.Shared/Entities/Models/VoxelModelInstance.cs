using System;
using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents instance data container of the voxel model.
    /// </summary>
    public class VoxelModelInstance : IBinaryStorable
    {
        // cached intermediate state of the model
        private VoxelModelState _internalState;

        // storable fields
        private int _animationIndex;
        private int _animationStepIndex;
        private int _elapsed;
        private bool _repeat;
        private Md5Hash _modelHash;
        private Quaternion _rotation;

        #region Properties
        /// <summary>
        /// Gets a parent voxel model
        /// </summary>
        public VoxelModel VoxelModel { get; private set; }

        /// <summary>
        /// Gets a storable md5 hash value of the model
        /// </summary>
        public Md5Hash ModelHash
        {
            get { return VoxelModel == null ? _modelHash : VoxelModel.Hash; }
        }

        /// <summary>
        /// Gets or sets model instance rotation
        /// </summary>
        public Quaternion Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        /// <summary>
        /// Animation index, -1 if no animation is performed
        /// </summary>
        public int AnimationIndex
        {
            get { return _animationIndex; }
            set { _animationIndex = value; }
        }
        
        /// <summary>
        /// Current animation step
        /// </summary>
        public int AnimationStepIndex
        {
            get { return _animationStepIndex; }
            set { _animationStepIndex = value; }
        }
        
        /// <summary>
        /// Time passed from start of current animation step
        /// </summary>
        public int Elapsed
        {
            get { return _elapsed; }
            set { _elapsed = value; }
        }
        
        /// <summary>
        /// Indicates if current animation should start again after the end
        /// </summary>
        public bool Repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }

        /// <summary>
        /// Gets a value that indicates if some animation is playing now
        /// </summary>
        public bool Playing
        {
            get { return _animationIndex != -1; }
        }

        /// <summary>
        /// Gets current voxel model state
        /// </summary>
        public VoxelModelState State
        {
            get
            {
                return Playing ? _internalState : VoxelModel.States[0];
            }
        }
        #endregion
        
        public VoxelModelInstance(VoxelModel model = null)
        {
            _animationIndex = -1;
            SetParentModel(model);
        }

        /// <summary>
        /// This method should be used only on deserialization step to restore parent model relationship
        /// </summary>
        /// <param name="model"></param>
        public void SetParentModel(VoxelModel model)
        {
            VoxelModel = model;
            // init the cached state, cached state should have the same structure as parent model states
            if(model != null)
                _internalState = new VoxelModelState(model.States[0]);
        }

        /// <summary>
        /// Call this method when the parent model has changed parts count (Editor use case)
        /// </summary>
        public void UpdateStates()
        {
            _internalState = new VoxelModelState(VoxelModel.States[0]);
        }

        /// <summary>
        /// Starts first animation with name specified
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="repeat"> </param>
        public void Play(string animationName, bool repeat = false)
        {
            if (string.IsNullOrEmpty(animationName)) 
                throw new ArgumentNullException("animationName");

            var animation = VoxelModel.Animations.FindIndex(a => a.Name == animationName);

            if (animation == -1)
                throw new ArgumentOutOfRangeException("animationName", "Model have not animation called " + animationName);

            _animationIndex = animation;
            _animationStepIndex = 0;
            _elapsed = 0;
            _repeat = repeat;
        }

        /// <summary>
        /// Starts animation with index specified
        /// </summary>
        /// <param name="index"></param>
        /// <param name="repeat"> </param>
        public void Play(int index, bool repeat = false)
        {
            if (index < 0 || index >= VoxelModel.Animations.Count)
                throw new ArgumentOutOfRangeException("index", "Model have not animation with index " + index);

            _animationIndex = index;
            _animationStepIndex = 0;
            _elapsed = 0;
            _repeat = repeat;
        }

        /// <summary>
        /// Updates current animation
        /// </summary>
        /// <param name="timePassed"></param>
        public void Update(ref long timePassed)
        {
            if (!Playing) return;
            var ms = (int)timePassed;

            _elapsed += ms;

            var animation = VoxelModel.Animations[_animationIndex];

            int duration = animation.Steps[_animationStepIndex].Duration;

            if (_elapsed > duration)
            {
                _elapsed -= duration;
                AnimationStepIndex++;

                if (_animationStepIndex == animation.Steps.Count)
                {
                    if (_repeat)
                        _animationStepIndex = 0;
                    else
                        Stop();
                }
            }

            if (Playing)
            {
                var state0 = VoxelModel.States[animation.Steps[_animationStepIndex].StateIndex];
                VoxelModelState state1;
                if (_animationStepIndex == animation.Steps.Count - 1)
                {
                    state1 = VoxelModel.States[animation.Steps[0].StateIndex];
                }
                else state1 = VoxelModel.States[animation.Steps[_animationStepIndex + 1].StateIndex];

                for (int i = 0; i < _internalState.PartsStates.Count; i++)
                {
                    _internalState.PartsStates[i].ActiveFrame = state0.PartsStates[i].ActiveFrame;
                    var step = (float)Elapsed / duration;
                    Matrix.SmoothStep(ref state0.PartsStates[i].Transform, ref state1.PartsStates[i].Transform, step, out _internalState.PartsStates[i].Transform);
                }
            }
        }

        /// <summary>
        /// Stops current animation
        /// </summary>
        public void Stop()
        {
            AnimationIndex = -1;
        }

        /// <summary>
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer)
        {
            writer.Write(VoxelModel.Hash);
            writer.Write(_rotation);
            writer.Write(_repeat);
            writer.Write(_elapsed);
            writer.Write((byte)_animationIndex);
            writer.Write((byte)_animationStepIndex);
        }

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            _modelHash = reader.ReadMd5Hash();
            _rotation = reader.ReadQuaternion();
            _repeat = reader.ReadBoolean();
            _elapsed = reader.ReadInt32();
            _animationIndex = reader.ReadByte();
            _animationStepIndex = reader.ReadByte();
        }
    }
}
