using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Structs;
using Utopia.Shared.Enums;
using SharpDX;
using S33M3Resources.Structs;
using System.ComponentModel;
using System.IO;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Settings
{
    [Serializable]
    public partial class CubeProfile : IBinaryStorable
    {
        [Browsable(false)]
        public bool IsSystemCube { get; set; }
        [Description("The cube's name"), Category("General")]
        public string Name { get; set; }
        [Description("The cube's description"), Category("General")]
        public string Description { get; set; }
        [Browsable(false), Description("Cube internal ID"), Category("General")]
        public byte Id { get; set; }
        [Description("Can be picked-up by player"), Category("General")]
        public bool IsPickable { get; set; }
        [Browsable(false)]
        public bool IsBlockingLight { get { return LightAbsorbed == 255; } }
        [Description("Blocking light block"), Category("General")]
        public byte LightAbsorbed { get; set; }
        [Description("Is partially or completly transparent ?"), Category("General")]
        public bool IsSeeThrough { get; set; }
        [Description("Is blocking water propagation ?"), Category("General")]
        public bool IsBlockingWater { get; set; }
        [Description("Is subject to collision detection"), Category("General")]
        public bool IsSolidToEntity { get; set; }
        [Browsable(false), Description("Can this block contain TAG informations = Metadata information"), Category("Technical")]
        public bool IsTaggable { get; set; }
        [Description("Is Y offseted block, to create not full block"), Category("General")]
        public double YBlockOffset { get; set; }
        [Description("Cube Familly"), Category("General")]
        public enuCubeFamilly CubeFamilly { get; set; }
        [Description("Value indicating if the block up and down faces are shorter"), Category("General")]
        public byte SideOffsetMultiplier { get; set; }


        public ByteColor EmissiveColor;
        [Description("Is the block emitting light"), Category("Light Source Color")]
        public bool IsEmissiveColorLightSource { get; set; }
        [Description("A value for the emmited color"), Category("Light Source Color")]
        public byte EmissiveColorA { get { return EmissiveColor.A; } set { EmissiveColor.A = value; } }
        [Description("R value for the emmited color"), Category("Light Source Color")]
        public byte EmissiveColorR { get { return EmissiveColor.R; } set { EmissiveColor.R = value; } }
        [Description("G value for the emmited color"), Category("Light Source Color")]
        public byte EmissiveColorG { get { return EmissiveColor.G; } set { EmissiveColor.G = value; } }
        [Description("B value for the emmited color"), Category("Light Source Color")]
        public byte EmissiveColorB { get { return EmissiveColor.B; } set { EmissiveColor.B = value; } }

        [Description("Low friction value will make the move on it easier = faster"), Category("Physics")]
        public float Friction { get; set; }
        [Description("When stop moving on the block, will the player continue to move"), Category("Physics")]
        public float SlidingValue { get; set; }

        private byte _biomeColorArrayTexture = 255;
        [Browsable(false), Description("Is the block subject to biome color mapping"), Category("Biome color")]
        public byte BiomeColorArrayTexture
        {
            get { return _biomeColorArrayTexture; }
            set { _biomeColorArrayTexture = value; }
        }

        //Texture id foreach face
        public byte[] Textures = new byte[6];
        [Description("Front texture Id"), Category("Textures")]
        public byte Tex_Front { get { return Textures[(int)CubeFaces.Front]; } set { Textures[(int)CubeFaces.Front] = value; } }
        [Description("Back texture Id"), Category("Textures")]
        public byte Tex_Back { get { return Textures[(int)CubeFaces.Back]; } set { Textures[(int)CubeFaces.Back] = value; } }
        [Description("Left texture Id"), Category("Textures")]
        public byte Tex_Left { get { return Textures[(int)CubeFaces.Left]; } set { Textures[(int)CubeFaces.Left] = value; } }
        [Description("Right texture Id"), Category("Textures")]
        public byte Tex_Right { get { return Textures[(int)CubeFaces.Right]; } set { Textures[(int)CubeFaces.Right] = value; } }
        [Description("Top texture Id"), Category("Textures")]
        public byte Tex_Top { get { return Textures[(int)CubeFaces.Top]; } set { Textures[(int)CubeFaces.Top] = value; } }
        [Description("Bottom texture Id"), Category("Textures")]
        public byte Tex_Bottom { get { return Textures[(int)CubeFaces.Bottom]; } set { Textures[(int)CubeFaces.Bottom] = value; } }

        private List<SoundSource> _walkingOverSound = new List<SoundSource>();
        [Description("Sound played when entity walk over a this cube"), Category("Sound")]
        public List<SoundSource> WalkingOverSound { get { return _walkingOverSound; } set { _walkingOverSound = value; } }

        /// <summary>
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer)
        {
            writer.Write(IsSystemCube);
            writer.Write(Name);
            writer.Write(Id);
            writer.Write(IsPickable);
            writer.Write(LightAbsorbed);
            writer.Write(IsSeeThrough);
            writer.Write(IsBlockingWater);
            writer.Write(IsSolidToEntity);
            writer.Write(IsTaggable);
            writer.Write(YBlockOffset);
            writer.Write((byte)CubeFamilly);
            writer.Write(SideOffsetMultiplier);
            writer.Write(IsEmissiveColorLightSource);
            writer.Write(EmissiveColor.R);
            writer.Write(EmissiveColor.G);
            writer.Write(EmissiveColor.B);
            writer.Write(EmissiveColor.A);
            writer.Write(Friction);
            writer.Write(SlidingValue);
            writer.Write(BiomeColorArrayTexture);
            writer.Write(Textures[0]);
            writer.Write(Textures[1]);
            writer.Write(Textures[2]);
            writer.Write(Textures[3]);
            writer.Write(Textures[4]);
            writer.Write(Textures[5]);

            BinarySerialize.SerializeArray(WalkingOverSound, writer);
        }

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            IsSystemCube = reader.ReadBoolean();
            Name = reader.ReadString();
            Id = reader.ReadByte();
            IsPickable = reader.ReadBoolean();
            LightAbsorbed = reader.ReadByte();
            IsSeeThrough = reader.ReadBoolean();
            IsBlockingWater = reader.ReadBoolean();
            IsSolidToEntity = reader.ReadBoolean();
            IsTaggable = reader.ReadBoolean();
            YBlockOffset = reader.ReadDouble();
            CubeFamilly = (enuCubeFamilly)reader.ReadByte();
            SideOffsetMultiplier = reader.ReadByte();
            IsEmissiveColorLightSource = reader.ReadBoolean();
            EmissiveColor = new ByteColor();
            EmissiveColor.R = reader.ReadByte();
            EmissiveColor.G = reader.ReadByte();
            EmissiveColor.B = reader.ReadByte();
            EmissiveColor.A = reader.ReadByte();
            Friction = reader.ReadSingle();
            SlidingValue = reader.ReadSingle();
            BiomeColorArrayTexture = reader.ReadByte();
            Textures[0] = reader.ReadByte();
            Textures[1] = reader.ReadByte();
            Textures[2] = reader.ReadByte();
            Textures[3] = reader.ReadByte();
            Textures[4] = reader.ReadByte();
            Textures[5] = reader.ReadByte();

            BinarySerialize.DeserializeArray(reader, out _walkingOverSound);
        }
    }
}
