using System.Collections.Generic;
using ProtoBuf;
using Utopia.Shared.Enums;
using S33M3Resources.Structs;
using System.ComponentModel;

namespace Utopia.Shared.Settings
{
    [ProtoContract]
    public class CubeProfile
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public bool IsSystemCube { get; set; }

        [Description("The cube's name"), Category("General")]
        [ProtoMember(2)]
        public string Name { get; set; }

        [Description("The cube's description"), Category("General")]
        [ProtoMember(3)]
        public string Description { get; set; }

        [Browsable(false), Description("Cube internal ID"), Category("General")]
        [ProtoMember(4)]
        public byte Id { get; set; }

        [Description("Can be picked-up by player"), Category("General")]
        [ProtoMember(5)]
        public bool IsPickable { get; set; }

        [Browsable(false)]
        public bool IsBlockingLight { get { return LightAbsorbed == 255; } }
        
        [Description("Blocking light block"), Category("General")]
        [ProtoMember(7)]
        public byte LightAbsorbed { get; set; }

        [Description("Is partially or completly transparent ?"), Category("General")]
        [ProtoMember(8)]
        public bool IsSeeThrough { get; set; }

        [Description("Is blocking water propagation ?"), Category("General")]
        [ProtoMember(9)]
        public bool IsBlockingWater { get; set; }

        [Description("Is subject to collision detection"), Category("General")]
        [ProtoMember(10)]
        public bool IsSolidToEntity { get; set; }
        
        [Browsable(false), Description("Can this block contain TAG informations = Metadata information"), Category("Technical")]
        [ProtoMember(11)]
        public bool IsTaggable { get; set; }

        [Description("Is Y offseted block, to create not full block"), Category("General")]
        [ProtoMember(12)]
        public double YBlockOffset { get; set; }

        [Description("Cube Familly"), Category("General")]
        [ProtoMember(13)]
        public enuCubeFamilly CubeFamilly { get; set; }

        [Description("Value indicating if the block up and down faces are shorter"), Category("General")]
        [ProtoMember(14)]
        public byte SideOffsetMultiplier { get; set; }


        [ProtoMember(15)]
        public ByteColor EmissiveColor;

        [Description("Is the block emitting light"), Category("Light Source Color")]
        [ProtoMember(16)]
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
        [ProtoMember(17)]
        public float Friction { get; set; }

        [Description("When stop moving on the block, will the player continue to move"), Category("Physics")]
        [ProtoMember(18)]
        public float SlidingValue { get; set; }

        [Browsable(true), Description("Is the block subject to biome color mapping"), Category("Biome color")]
        [ProtoMember(19)]
        public byte BiomeColorArrayTexture { get; set; }

        //Texture id foreach face
        [ProtoMember(20)]
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
        [ProtoMember(21)]
        public List<SoundSource> WalkingOverSound { get { return _walkingOverSound; } set { _walkingOverSound = value; } }

        [ProtoBeforeDeserialization]
        public void BeforeDeserialize()
        {
            Textures = null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
