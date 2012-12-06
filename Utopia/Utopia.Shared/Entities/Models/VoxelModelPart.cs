using System.IO;
using ProtoBuf;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a part of a voxel model. Model parts consists of frames
    /// </summary>
    [ProtoContract]
    public class VoxelModelPart : IBinaryStorable
    {
        /// <summary>
        /// Gets or sets voxel model part name, example "Head"
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Indicates if this part is the head, if true then played head rotation will be applied to the part
        /// </summary>
        [ProtoMember(2)]
        public bool IsHead { get; set; }

        /// <summary>
        /// Indicates if this part is the arm. Equipped tool will be displayed at the arm palm point
        /// </summary>
        [ProtoMember(3)]
        public bool IsArm { get; set; }

        public void Save(BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Name))
                Name = "unnamed";

            writer.Write(Name);
            writer.Write(IsHead);
            writer.Write(IsArm);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            IsHead = reader.ReadBoolean();
            IsArm = reader.ReadBoolean();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}