using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.LandscapeEntities.Trees;
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
        /// Gets or sets model author nickname
        /// </summary>
        [ProtoMember(8)]
        public string Author { get; set; }

        [ProtoAfterDeserialization]
        public void Deserialized()
        {
            foreach (var voxelModelState in States)
            {
                voxelModelState.ParentModel = this;
            }
        }
        
        /// <summary>
        /// Calculates a md5 hash from a model
        /// </summary>
        public void UpdateHash()
        {
            using (var ms = new MemoryStream())
            {
                Hash = null;
                Serializer.Serialize(ms, this);
                ms.Position = 0;
                Hash = Md5Hash.Calculate(ms);
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
            if (File.Exists(path))
                File.Delete(path);

            using (var fs = new GZipStream(File.OpenWrite(path), CompressionMode.Compress))
            {
                Serializer.Serialize(fs, this);
            }
        }

        public static VoxelModel LoadFromFile(string path)
        {
            return LoadFromStream(File.OpenRead(path));
        }

        public static VoxelModel LoadFromStream(Stream stream)
        {
            using (var fs = new GZipStream(stream, CompressionMode.Decompress))
            {
                return Serializer.Deserialize<VoxelModel>(fs);
            }
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
            var mainIndex = States.FindIndex(s => string.Equals(s.Name, "Main", StringComparison.InvariantCultureIgnoreCase));

            if (mainIndex == -1)
                mainIndex = 0;

            return States[mainIndex];
        }

        /// <summary>
        /// Returns the state that will be used as default icon
        /// </summary>
        /// <returns></returns>
        public VoxelModelState GetIconState()
        {
            var iconIndex = States.FindIndex(s => string.Equals(s.Name, "Icon", StringComparison.InvariantCultureIgnoreCase));

            if (iconIndex != -1)
                return States[iconIndex];

            return GetMainState();
        }

        public void AddPart(VoxelModelPart part)
        {
            foreach (var voxelModelState in States)
            {
                voxelModelState.PartsStates.Add(new VoxelModelPartState());
            }

            Parts.Add(part);
        }

        public void RemovePartAt(int selectedPartIndex)
        {
            Parts.RemoveAt(selectedPartIndex);

            foreach (var voxelModelState in States)
            {
                voxelModelState.PartsStates.RemoveAt(selectedPartIndex);
            }
        }

        public void RemoveFrameAt(int index)
        {
            Frames.RemoveAt(index);
            
            foreach (var voxelModelState in States)
            {
                foreach (var ps in voxelModelState.PartsStates)
                {
                    if (ps.ActiveFrame == index)
                        ps.ActiveFrame = byte.MaxValue;
                    if (ps.ActiveFrame == byte.MaxValue)
                        continue;
                    if (ps.ActiveFrame > index)
                        ps.ActiveFrame--;
                }
            }
        }

        /// <summary>
        /// Performs integrity checks
        /// </summary>
        public void Validate()
        {
            foreach (var voxelModelState in States)
            {
                if (voxelModelState.PartsStates.Count != Parts.Count)
                {
                    if (Debugger.IsAttached)
                    {
                        // we have different partsStates count than Parts count
                        // remember what you were doing to have this issue
                        // tell erty about that, he will fix the bug

                        Debugger.Break();
                    }

                    if (voxelModelState.PartsStates.Count > Parts.Count)
                        voxelModelState.PartsStates.RemoveRange(Parts.Count, voxelModelState.PartsStates.Count - Parts.Count);
                }
            }
        }

        public static VoxelModel GenerateTreeModel(int seed, TreeBluePrint blueprint, TreeLSystem treeSystem = null)
        {
            if (treeSystem == null)
                treeSystem = new TreeLSystem();

            var blocks = treeSystem.Generate(seed, new Vector3I(), blueprint);

            var max = new Vector3I(int.MinValue);
            var min = new Vector3I(int.MaxValue);

            foreach (var blockWithPosition in blocks)
            {
                max = Vector3I.Max(max, blockWithPosition.WorldPosition);
                min = Vector3I.Min(min, blockWithPosition.WorldPosition);
            }

            var size = max - min + Vector3I.One;
            var model = new VoxelModel();
            model.Name = "Tree Example";
            model.ColorMapping = new ColorMapping();
            model.ColorMapping.BlockColors = new Color4[64];
            model.ColorMapping.BlockColors[0] = Color.Brown.ToColor4();
            model.ColorMapping.BlockColors[1] = Color.Green.ToColor4();

            var frame = new VoxelFrame(size);
            foreach (var blockWithPosition in blocks)
            {
                frame.BlockData.SetBlock(blockWithPosition.WorldPosition - min, blockWithPosition.BlockId == blueprint.TrunkBlock ? (byte)1 : (byte)2);
            }

            model.Frames.Add(frame);
            var part = new VoxelModelPart();
            part.Name = "Main";
            model.Parts.Add(part);

            var state = new VoxelModelState(model);
            state.Name = "Default";
            state.PartsStates[0].Translation = new Vector3(min.X, 0, min.Z);
            model.States.Add(state);

            return model;
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
