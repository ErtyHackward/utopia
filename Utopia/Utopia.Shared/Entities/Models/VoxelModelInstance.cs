using System;
using System.IO;
using System.Linq;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents instance data container of the voxel model.
    /// Head and Body rotation, animation state
    /// </summary>
    public class VoxelModelInstance : IBinaryStorable
    {
        // cached intermediate state of the model
        private VoxelModelState _internalState;

        // storable fields
        private int _animationIndex;
        private int _animationStepIndexFrom;
        private int _animationStepIndexTo;
        private int _elapsed;
        private bool _repeat;
        private Md5Hash _modelHash;
        private Quaternion _rotation;
        private Quaternion _headRotation;
        private bool _stopping;
        
        #region Properties

        /// <summary>
        /// Instance light color
        /// </summary>
        public Color3 LightColor { get; set; }

        /// <summary>
        /// Instance world position
        /// </summary>
        public Matrix World { get; set; }

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
        /// Gets or sets model instance head rotation
        /// </summary>
        public Quaternion HeadRotation
        {
            get { return _headRotation; }
            set { _headRotation = value; }
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
            get { return _animationStepIndexTo; }
            set { _animationStepIndexTo = value; }
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
                return _internalState;
            }
        }
        #endregion
        
        public VoxelModelInstance(VoxelModel model = null)
        {
            _rotation = Quaternion.Identity;
            _headRotation = Quaternion.Identity;
            _animationIndex = -1;
            LightColor = new Color3(1, 1, 1);
            World = Matrix.Identity;
            SetParentModel(model);
        }

        /// <summary>
        /// This method should be used only on deserialization step to restore parent model relationship
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetParentModel(VoxelModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            VoxelModel = model;

            // init the cached state, cached state should have the same structure as parent model states
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
        /// Determines whether the animation can be played
        /// </summary>
        /// <param name="animationName"></param>
        public bool CanPlay(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
                throw new ArgumentNullException("animationName");

            return VoxelModel.Animations.Any(a => a.Name == animationName);
        }

        /// <summary>
        /// Starts first animation with name specified
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="repeat"></param>
        public void Play(string animationName, bool repeat = false)
        {
            if (string.IsNullOrEmpty(animationName))
                throw new ArgumentNullException("animationName");

            var animation = VoxelModel.Animations.FindIndex(a => a.Name == animationName);

            if (animation == -1)
                throw new ArgumentOutOfRangeException("animationName", "Model have not animation called " + animationName);

            _animationIndex = animation;
            _animationStepIndexFrom = -1;
            _animationStepIndexTo = 0;
            _elapsed = 0;
            _repeat = repeat;
        }

        /// <summary>
        /// Starts animation with index specified
        /// </summary>
        /// <param name="index"></param>
        /// <param name="repeat"></param>
        public void Play(int index, bool repeat = false)
        {
            if (index < 0 || index >= VoxelModel.Animations.Count)
                throw new ArgumentOutOfRangeException("index", "Model have not animation with index " + index);

            _animationIndex = index;
            _animationStepIndexFrom = -1;
            _animationStepIndexTo = 0;
            _elapsed = 0;
            _repeat = repeat;
        }

        /// <summary>
        /// Updates current animation, should be done before each draw
        /// </summary>
        /// <param name="timePassed"></param>
        public void Interpolation(long timePassed)
        {
            if (!Playing) return;
            var ms = (int)timePassed;

            _elapsed += ms;

            var animation = VoxelModel.Animations[_animationIndex];

            var duration = _animationStepIndexTo == -1 ? animation.Steps[0].Duration : animation.Steps[_animationStepIndexTo].Duration;

            if (_elapsed > duration)
            {
                _elapsed -= duration;
                
                if (_animationStepIndexTo == -1)
                    AnimationIndex = -1;
                else
                {
                    _animationStepIndexFrom = _animationStepIndexTo;

                    if (_stopping)
                    {
                        _stopping = false;
                        _animationStepIndexTo = -1;
                    }
                    else
                    {
                        _animationStepIndexTo++;

                        if (_animationStepIndexTo == animation.Steps.Count)
                        {
                            if (_repeat)
                                _animationStepIndexTo = 0;
                            else
                                _animationStepIndexTo = -1;
                        }
                    }
                }
            }

            if (Playing)
            {
                // take previous state
                var state0 = _animationStepIndexFrom == -1 ? VoxelModel.States[0] : VoxelModel.States[animation.Steps[_animationStepIndexFrom].StateIndex];

                VoxelModelState state1;
                if (_animationStepIndexTo == -1)
                {
                    state1 = VoxelModel.States[0];
                }
                else if (_animationStepIndexTo == animation.Steps.Count)
                {
                    state1 = VoxelModel.States[animation.Steps[0].StateIndex];
                }
                else state1 = VoxelModel.States[animation.Steps[_animationStepIndexTo].StateIndex];

                for (var i = 0; i < _internalState.PartsStates.Count; i++)
                {
                    _internalState.PartsStates[i].ActiveFrame = state0.PartsStates[i].ActiveFrame;
                    var step = (float)Elapsed / duration;

                    var psFrom = state0.PartsStates[i];
                    var psTo = state1.PartsStates[i];
                    var psResult = _internalState.PartsStates[i];

                    psResult.Interpolation(psFrom, psTo, step);
                }
            }
        }

        /// <summary>
        /// Stops current animation
        /// </summary>
        public void Stop()
        {
            if (_animationIndex != -1)
            {
                _stopping = true;
                _repeat = false;
            }
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
            writer.Write((byte)_animationStepIndexFrom);
            writer.Write((byte)_animationStepIndexTo);   
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
            _animationStepIndexFrom = reader.ReadByte();
            _animationStepIndexTo = reader.ReadByte();
        }
    }
}

