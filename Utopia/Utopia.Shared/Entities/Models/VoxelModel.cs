using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ProtoBuf;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a voxel model for voxel entities. Model consists from one or more parts. 
    /// Each part have its own relative position and rotation and may have a color mapping scheme.
    /// </summary>
    [ProtoContract]
    public class VoxelModel
    {
        public const int ModelFormatVersion = 2;

        public VoxelModel()
        {
            Frames = new List<VoxelFrame>();
            Parts = new List<VoxelModelPart>();
            States = new List<VoxelModelState>();
            Animations = new List<VoxelModelAnimation>();
        }

        /// <summary>
        /// Gets or sets model name
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets current model md5 hash
        /// </summary>
        [ProtoMember(2)]
        public Md5Hash Hash { get; private set; }

        /// <summary>
        /// Gets a list of model frames
        /// </summary>
        [ProtoMember(3)]
        public List<VoxelFrame> Frames { get; private set; }

        /// <summary>
        /// Gets a list of parts of the model
        /// </summary>
        [ProtoMember(4)]
        public List<VoxelModelPart> Parts { get; private set; }
        
        /// <summary>
        /// Gets a list of model states
        /// </summary>
        [ProtoMember(5)]
        public List<VoxelModelState> States { get; private set; }

        /// <summary>
        /// Gets a list of model animations
        /// </summary>
        [ProtoMember(6)]
        public List<VoxelModelAnimation> Animations { get; private set; }
        
        /// <summary>
        /// Gets or sets global color mapping
        /// </summary>
        [ProtoMember(7)]
        public ColorMapping ColorMapping { get; set; }

        /// <summary>
        /// Calculates a md5 hash from a model
        /// </summary>
        public void UpdateHash()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);

                SaveImpl(writer);

                ms.Position = 0;
                Hash = Md5Hash.Calculate(ms);
            }
        }

        private void SaveImpl(BinaryWriter writer, Md5Hash hash = null)
        {
            writer.Write(ModelFormatVersion);

            writer.Write(Name);

            if (hash != null)
            {
                writer.Write((byte)16);
                writer.Write(hash.Bytes);
            }
            else writer.Write((byte)0);

            ColorMapping.Write(writer, ColorMapping);

            writer.Write((byte)Frames.Count);
            foreach (var voxelFrame in Frames)
            {
                voxelFrame.Save(writer);
            }
            writer.Write((byte)Parts.Count);
            foreach (var voxelModelPart in Parts)
            {
                voxelModelPart.Save(writer);
            }
            writer.Write((byte)States.Count);
            foreach (var voxelModelState in States)
            {
                voxelModelState.Save(writer);
            }
            writer.Write((byte)Animations.Count);
            foreach (var voxelModelAnimation in Animations)
            {
                voxelModelAnimation.Save(writer);
            }
        }

        public void Save(BinaryWriter writer)
        {
            UpdateHash();
            SaveImpl(writer, Hash);
        }

        public void Load(BinaryReader reader)
        {
            var version = reader.ReadInt32();

            if (version != ModelFormatVersion)
                throw new InvalidDataException("Invalid model format version. Convert models to the v" + ModelFormatVersion + " format to use");

            Name = reader.ReadString();

            var count = reader.ReadByte();

            if (count > 0)
            {
                var hash = reader.ReadBytes(count);
                if (hash.Length != 16)
                    throw new EndOfStreamException();
                Hash = new Md5Hash(hash);
            }
            else Hash = null;

            ColorMapping = ColorMapping.Read(reader);

            count = reader.ReadByte();

            Frames.Clear();

            for (int i = 0; i < count; i++)
            {
                var frame = new VoxelFrame();
                frame.Load(reader);
                Frames.Add(frame);
            }

            count = reader.ReadByte();

            Parts.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelPart = new VoxelModelPart();
                modelPart.Load(reader);
                Parts.Add(modelPart);
            }

            count = reader.ReadByte();

            States.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelState = new VoxelModelState(this);
                modelState.Load(reader);
                States.Add(modelState);
            }

            count = reader.ReadByte();

            Animations.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelState = new VoxelModelAnimation();
                modelState.Load(reader);
                Animations.Add(modelState);
            }
        }

        /// <summary>
        /// Removes a state from index, updates animations indices
        /// </summary>
        /// <param name="selectedStateIndex"></param>
        public void RemoveStateAt(int selectedStateIndex)
        {
            States.RemoveAt(selectedStateIndex);

            foreach (var voxelModelAnimation in Animations)
            {
                for (var i = 0; i < voxelModelAnimation.Steps.Count; i++)
                {
                    var animationStep = voxelModelAnimation.Steps[i];
                    if (animationStep.StateIndex > selectedStateIndex)
                        animationStep.StateIndex--;
                    voxelModelAnimation.Steps[i] = animationStep;
                }
            }
        }

        public VoxelModelInstance CreateInstance()
        {
            return new VoxelModelInstance(this);
        }

        public void SaveToFile(string path)
        {
            using (var fs = new GZipStream(File.OpenWrite(path), CompressionMode.Compress))
            {
                var writer = new BinaryWriter(fs);
                Save(writer);
            }
        }

        public static VoxelModel LoadFromFile(string path)
        {
            var voxelModel = new VoxelModel();
            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);
                voxelModel.Load(reader);
            }
            return voxelModel;
        }

        /// <summary>
        /// Returns first arm (if any) otherwise null
        /// </summary>
        /// <returns></returns>
        public VoxelModelPartState GetArm()
        {
            var index = GetArmIndex();

            if (index == -1)
                return null;

            return States[0].PartsStates[index];
        }

        public int GetArmIndex()
        {
            return Parts.FindIndex(p => p.IsArm);
        }

        /// <summary>
        /// Returns main state (called main or the first)
        /// </summary>
        /// <returns></returns>
        public VoxelModelState GetMainState()
        {
            var mainIndex = States.FindIndex(s => s.Name == "Main");

            if (mainIndex == -1)
                mainIndex = 0;

            return States[mainIndex];
        }
    }

    public static class VoxelExtensions
    {
        public static VoxelModelState GetByName(this List<VoxelModelState> list, string name)
        {
            return list.First(s => s.Name == name);
        }

        /// <summary>
        /// Returns animation to perform state switch
        /// </summary>
        /// <param name="list"></param>
        /// <param name="stateIndexFrom"></param>
        /// <param name="stateIndexTo"></param>
        /// <returns></returns>
        public static VoxelModelAnimation GetStateSwitch(this List<VoxelModelAnimation> list, byte stateIndexFrom, byte stateIndexTo)
        {
            if (stateIndexFrom == stateIndexTo)
                throw new ArgumentException("State index could not be the same");

            foreach (var anim in list)
            {
                if (anim.Steps[0].StateIndex == stateIndexFrom && anim.Steps[anim.Steps.Count-1].StateIndex == stateIndexTo)
                {
                    return anim;
                }
            }

            return null;
        }
    }
}
