using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ProtoBuf;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a voxel model animation
    /// </summary>
    [ProtoContract]
    public class VoxelModelAnimation : IBinaryStorable
    {
        /// <summary>
        /// Gets or sets animation name
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets animation duration in milliseconds, 1000 = 1 sec
        /// </summary>
        [ProtoMember(2)]
        public int Duration { get; set; }

        /// <summary>
        /// Gets list of animation steps
        /// </summary>
        [ProtoMember(3)]
        public List<AnimationStep> Steps { get; private set; }

        /// <summary>
        /// Gets start animation index (default -1)
        /// </summary>
        [ProtoMember(4)]
        [DefaultValue(-1)]
        public int StartFrame { get; set; }

        public VoxelModelAnimation()
        {
            Steps = new List<AnimationStep>();
            StartFrame = -1;
        }

        public void Save(BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Name))
                Name = "unnamed";

            writer.Write(Name);

            writer.Write((byte)Steps.Count);

            foreach (var animationStep in Steps)
            {
                writer.Write(animationStep.StateIndex);
                writer.Write(animationStep.Duration);
            }

        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            var count = reader.ReadByte();

            Steps.Clear();

            for (var i = 0; i < count; i++)
            {
                AnimationStep step;

                step.StateIndex = reader.ReadByte();
                step.Duration = reader.ReadInt32();

                Steps.Add(step);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents an animation step
    /// </summary>
    [ProtoContract]
    public struct AnimationStep
    {
        /// <summary>
        /// Index of a state to be applied
        /// </summary>
        [ProtoMember(1)]
        public byte StateIndex;

        /// <summary>
        /// State transition animation time
        /// </summary>
        [ProtoMember(2)]
        public int Duration;

        public override string ToString()
        {
            return string.Format("State {0} Time {1}", StateIndex, Duration);
        }
    }
}
