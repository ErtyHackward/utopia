using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents instance data container of the voxel model.
    /// Head and Body rotation, animation state
    /// </summary>
    public class VoxelModelInstance
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        // cached intermediate state of the model
        private VoxelModelState _internalState;

        private VoxelModelState _currentState;

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
        private string _switchStateTarget;
        #endregion

        /// <summary>
        /// Occurs when model instance finished to change its state
        /// </summary>
        public event EventHandler StateChanged;

        protected virtual void OnStateChanged()
        {
            var handler = StateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region Public Properties
        /// <summary>
        /// Instance light color
        /// </summary>
        public Color3 LightColor { get; set; }

        /// <summary>
        /// Gets or sets instance alpha transparency
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// Instance world position (with scaling incorporated)
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

        /// <summary>
        /// Contains a list of particles emit data, null if the particles are not initialized
        /// </summary>
        public List<DateTime> ParticuleLastEmit { get; set; }

        #endregion

        public VoxelModelInstance(VoxelModel model = null)
        {
            _rotation = Quaternion.Identity;
            _headRotation = Quaternion.Identity;
            World = Matrix.Identity;

            _animationIndex = -1;
            Alpha = 1;
            LightColor = new Color3(1, 1, 1);
            SetParentModel(model);
        }

        #region Public Methods
        /// <summary>
        /// Call this method when the parent model has changed parts count (Editor use case)
        /// </summary>
        public void UpdateStates()
        {
            SetState(VoxelModel.GetMainState());
        }

        /// <summary>
        /// Changes current instance active state
        /// </summary>
        /// <param name="state"></param>
        public void SetState(VoxelModelState state)
        {
            _currentState = state;

            // create a copy because we can to change its values
            // inside the instance
            _internalState = new VoxelModelState(state);

            OnStateChanged();
        }

        /// <summary>
        /// Changes current instance active state instantly
        /// </summary>
        /// <param name="stateName"></param>
        public void SetState(string stateName)
        {
            SetState(VoxelModel.States.GetByName(stateName));
        }

        /// <summary>
        /// Changes current instance state using animation if possible
        /// </summary>
        /// <param name="newStateName"></param>
        public void SwitchState(string newStateName)
        {
            byte indexEnd = 0;

            for (byte i = 0; i < VoxelModel.States.Count; i++)
            {
                var state = VoxelModel.States[i];

                if (state.Name == newStateName)
                    indexEnd = i;
            }

            string animName = string.Empty;

            foreach (var anim in VoxelModel.Animations)
            {
                if (anim.Steps[anim.Steps.Count - 1].StateIndex == indexEnd)
                {
                    animName = anim.Name;
                }
            }

            if (!string.IsNullOrEmpty(animName))
            {
                _switchStateTarget = newStateName;
                Play(animName);
            }
            else
            {
                SetState(newStateName);
            }
        }

        /// <summary>
        /// Returns world matrix to draw the tool
        /// </summary>
        /// <returns></returns>
        public Matrix GetToolTransform()
        {
            var armIndex = VoxelModel.GetArmIndex();

            var arm = VoxelModel.GetArm();

            if (armIndex == -1 || !arm.PalmTransform.HasValue)
                return Matrix.Identity;

            // palmTransform value is stored only in the first state (which is returned by GetArm())
            // so we can't use current state palmTrasform value
            return arm.PalmTransform.Value * State.PartsStates[armIndex].GetTransformation() * Matrix.RotationQuaternion(Rotation) * World;
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
        /// Checks if the entity have an animation and no one animation is playing right now
        /// And if has plays it.
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns>true if animation is started to play otherwise false</returns>
        public bool TryPlay(string animationName, bool repeat = false)
        {
            if (_animationIndex != -1)
                return false;

            if (CanPlay(animationName))
            {
                Play(animationName, repeat);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        public bool TryPlayForced(string animationName, bool repeat)
        {
            if (CanPlay(animationName))
            {
                Play(animationName, repeat);
                return true;
            }
            
            return false;
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
            _animationStepIndexFrom = VoxelModel.Animations[animation].StartFrame;
            _animationStepIndexTo = _animationStepIndexFrom + 1;
            _elapsed = 0;
            _repeat = repeat;
            _stopping = false;
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
            _animationStepIndexFrom = VoxelModel.Animations[index].StartFrame;
            _animationStepIndexTo = _animationStepIndexFrom + 1;
            _elapsed = 0;
            _repeat = repeat;
            _stopping = false;
        }

        /// <summary>
        /// Updates current animation, should be done before each draw
        /// </summary>
        /// <param name="timePassed"></param>
        public void Interpolation(float elapsedTime)
        {
            if (!Playing) 
                return;

            var ms = (int)(elapsedTime * 1000.0f);

            _elapsed += ms;

            var animation = VoxelModel.Animations[_animationIndex];

            var duration = _animationStepIndexTo == -1 ? animation.Steps[0].Duration : animation.Steps[_animationStepIndexTo].Duration;

            if (_elapsed > duration)
            {
                _elapsed -= duration;

                if (_animationStepIndexTo == -1)
                {
                    AnimationIndex = -1;
                    OnStop();
                }
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
                            {
                                if (!string.IsNullOrEmpty(_switchStateTarget))
                                {
                                    AnimationIndex = -1;
                                    OnStop();
                                }
                                else
                                    // return to the main state
                                    _animationStepIndexTo = -1;
                            }
                        }
                    }
                }
            }

            if (Playing)
            {
                // take previous state
                var state0 = _animationStepIndexFrom == -1 ? _currentState : VoxelModel.States[animation.Steps[_animationStepIndexFrom].StateIndex];

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

                _internalState.UpdateBoundingBox();
            }
        }
        /// <summary>
        /// Stops specified animation smoothly
        /// </summary>
        public void Stop(string animationName = null)
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                var animation = VoxelModel.Animations.FindIndex(a => a.Name == animationName);
                if (animation == -1)
                    logger.Debug("{0}", "Model have not animation called " + animationName);

                if (_animationIndex != animation)
                    return;
            }
            
            if (_animationIndex != -1)
            {
                _stopping = true;
                _repeat = false;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method should be used only on deserialization step to restore parent model relationship
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void SetParentModel(VoxelModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            VoxelModel = model;

            // init the cached state, cached state should have the same structure as parent model states
            SetState(model.GetMainState());
        }

        private void OnStop()
        {
            if (!string.IsNullOrEmpty(_switchStateTarget))
            {
                SetState(_switchStateTarget);
                _switchStateTarget = string.Empty;
            }
        }
        #endregion

        
    }
}

