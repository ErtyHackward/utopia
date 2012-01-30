using System;
using S33M3Engines.D3D;
using SharpDX;
using Utopia.Shared.Entities.Models;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Represents instance data container of the voxel model
    /// </summary>
    public class VoxelModelInstance
    {
        private VoxelModelState _internalState;

        /// <summary>
        /// Gets a parent visual voxel model
        /// </summary>
        public VisualVoxelModel VisualVoxelModel { get; private set; }

        /// <summary>
        /// Animation index, -1 if no animation is performed
        /// </summary>
        public int AnimationIndex { get; set; }

        /// <summary>
        /// Current animation step
        /// </summary>
        public int AnimationStepIndex { get; set; }

        /// <summary>
        /// Time passed from start of current step
        /// </summary>
        public int Elapsed { get; set; }

        /// <summary>
        /// Indicates if current animation repeats after the end
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Gets a value that indicates if some animation is playing now
        /// </summary>
        public bool Playing
        {
            get { return AnimationIndex != -1; }
        }

        /// <summary>
        /// Gets current voxel model state
        /// </summary>
        public VoxelModelState State
        {
            get
            {
                return Playing ? _internalState : VisualVoxelModel.VoxelModel.States[0];
            }
        }

        public VoxelModelInstance(VisualVoxelModel model)
        {
            AnimationIndex = -1;

            if (model == null) throw new ArgumentNullException("model");
            VisualVoxelModel = model;
            _internalState = new VoxelModelState(model.VoxelModel.States[0]);
        }

        /// <summary>
        /// Call this method when the parent model has changed parts count (Editor use case)
        /// </summary>
        public void UpdateStates()
        {
            _internalState = new VoxelModelState(VisualVoxelModel.VoxelModel.States[0]);
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

            var animation = VisualVoxelModel.VoxelModel.Animations.FindIndex(a => a.Name == animationName);

            if (animation == -1)
                throw new ArgumentOutOfRangeException("animationName", "Model have not animation called " + animationName);

            AnimationIndex = animation;
            AnimationStepIndex = 0;
            Elapsed = 0;
            Repeat = repeat;
        }

        /// <summary>
        /// Starts animation with index specified
        /// </summary>
        /// <param name="index"></param>
        /// <param name="repeat"> </param>
        public void Play(int index, bool repeat = false)
        {
            if (index < 0 || index >= VisualVoxelModel.VoxelModel.Animations.Count)
                throw new ArgumentOutOfRangeException("index", "Model have not animation with index " + index);

            AnimationIndex = index;
            AnimationStepIndex = 0;
            Elapsed = 0;
            Repeat = repeat;
        }

        /// <summary>
        /// Updates current animation
        /// </summary>
        /// <param name="timePassed"></param>
        public void Update(ref long timePassed)
        {
            if (!Playing) return;
            var ms = (int)timePassed;

            Elapsed += ms;

            var animation = VisualVoxelModel.VoxelModel.Animations[AnimationIndex];

            int duration = animation.Steps[AnimationStepIndex].Duration;

            if (Elapsed > duration)
            {
                Elapsed -= duration;
                AnimationStepIndex++;

                if (AnimationStepIndex == animation.Steps.Count)
                {
                    if (Repeat)
                        AnimationStepIndex = 0;
                    else
                        Stop();
                }
            }

            if (Playing)
            {
                var state0 = VisualVoxelModel.VoxelModel.States[animation.Steps[AnimationStepIndex].StateIndex];
                VoxelModelState state1;
                if(AnimationStepIndex == animation.Steps.Count -1)
                {
                    state1 = VisualVoxelModel.VoxelModel.States[animation.Steps[0].StateIndex];
                }
                else state1 = VisualVoxelModel.VoxelModel.States[animation.Steps[AnimationStepIndex+1].StateIndex];

                for (int i = 0; i < _internalState.PartsStates.Count; i++)
                {
                    _internalState.PartsStates[i].ActiveFrame = state0.PartsStates[i].ActiveFrame;
                    var step = (float)Elapsed / duration;
                    Matrix.SmoothStep(ref state0.PartsStates[i].Transform, ref state1.PartsStates[i].Transform, step, out _internalState.PartsStates[i].Transform);
                }
            }
        }

        /// <summary>
        /// Draws a model using its instance data and a world marix
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="world"></param>
        public void Draw(HLSLVoxelModel effect, Matrix world)
        {
            effect.CBPerFrame.Values.World = world;
            effect.CBPerFrame.IsDirty = true;
            VisualVoxelModel.Draw(effect, State);
        }

        /// <summary>
        /// Draws current instance
        /// </summary>
        /// <param name="effect"></param>
        public void Draw(HLSLVoxelModel effect)
        {
            VisualVoxelModel.Draw(effect, State);
        }

        /// <summary>
        /// Stops current animation
        /// </summary>
        public void Stop()
        {
            AnimationIndex = -1;
        }
    }
}
