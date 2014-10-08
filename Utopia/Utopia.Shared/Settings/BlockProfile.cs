using System.Collections.Generic;
using ProtoBuf;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Enums;
using S33M3Resources.Structs;
using System.ComponentModel;
using System.Globalization;
using System;
using System.Drawing.Design;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Settings
{
    [ProtoContract]
    public class BlockProfile
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

        private byte _biomeColorArrayTexture = 255;
        [Browsable(true), Description("Is the block subject to biome color mapping"), Category("Biome color")]
        [ProtoMember(19, IsRequired = true)]
        public byte BiomeColorArrayTexture
        {
            get { return _biomeColorArrayTexture; }
            set { _biomeColorArrayTexture = value; }
        }

        private List<SoundSource> _walkingOverSound = new List<SoundSource>();

        [Description("Sound played when entity walk over a this cube"), Category("Sound")]
        [ProtoMember(21)]
        public List<SoundSource> WalkingOverSound { get { return _walkingOverSound; } set { _walkingOverSound = value; } }

        [Description("Block hardness value, 0 = undestructible"), Category("General")]
        [ProtoMember(22)]
        public uint Hardness { get; set; }

        [Description("Sound played when entity hits this cube"), Category("Sound")]
        [ProtoMember(23)]
        public List<SoundSource> HitSounds { get; set; }

        [ProtoMember(24)]
        public TextureData[] Textures = new TextureData[6];
        [Description("Front texture Id"), Category("Textures")]
        public TextureData Tex_Front { get { return Textures[(int)CubeFaces.Front]; } set { Textures[(int)CubeFaces.Front] = value; } }
        [Description("Back texture Id"), Category("Textures")]
        public TextureData Tex_Back { get { return Textures[(int)CubeFaces.Back]; } set { Textures[(int)CubeFaces.Back] = value; } }
        [Description("Left texture Id"), Category("Textures")]
        public TextureData Tex_Left { get { return Textures[(int)CubeFaces.Left]; } set { Textures[(int)CubeFaces.Left] = value; } }
        [Description("Right texture Id"), Category("Textures")]
        public TextureData Tex_Right { get { return Textures[(int)CubeFaces.Right]; } set { Textures[(int)CubeFaces.Right] = value; } }
        [Description("Top texture Id"), Category("Textures")]
        public TextureData Tex_Top { get { return Textures[(int)CubeFaces.Top]; } set { Textures[(int)CubeFaces.Top] = value; } }
        [Description("Bottom texture Id"), Category("Textures")]
        public TextureData Tex_Bottom { get { return Textures[(int)CubeFaces.Bottom]; } set { Textures[(int)CubeFaces.Bottom] = value; } }

        [Description("Indestructible block"), Category("General")]
        [ProtoMember(25)]
        public bool Indestructible { get; set; }

        [Description("Block health impact when hitted, < 0 = damge, > 0 = Healing per second value"), Category("General")]
        [ProtoMember(26)]
        public int HealthModification { get; set; }

        /// <summary>
        /// Possible block transformations (example: ore from block)
        /// </summary>
        [Category("Gameplay")]
        [Description("Allows to transform the item when it is picked")]
        [ProtoMember(27)]
        public List<ItemTransformation> Transformations { get; set; }

        public BlockProfile()
        {
            HitSounds = new List<SoundSource>();
            Transformations = new List<ItemTransformation>();
        }

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


    [ProtoContract]
    [TypeConverter(typeof(TextureDataTypeConverter))]
    public class TextureData
    {
        [ProtoContract]
        public class TextureMeta
        {
            [Browsable(true)]
            [ProtoMember(1)]
            public string Name { get; set; }
            [Browsable(false)]
            [ProtoMember(2)]
            public byte AnimationFrames { get; set; }

            public override string ToString()
            {
                if (Name == null) return "";

                if (AnimationFrames > 1)
                {
                    return string.Format("{0} [anim. {1} frames]", Name, AnimationFrames);
                }
                else
                {
                    return Name;
                }
            }
        }

        [ProtoMember(1)]
        [Description("Texture file name")]
        [Editor(typeof(TextureSelector), typeof(UITypeEditor))]
        public TextureMeta Texture { get; set; }

        [ProtoMember(2)]
        [Description("Texture speed animation in fps")]
        public byte AnimationSpeed { get; set; }

        [Browsable(false)]
        public int TextureArrayId { get; set; } //Need to be filled in at runtime.

        [Browsable(false)]
        public bool isAnimated { get { return Texture == null ? false : (Texture.AnimationFrames > 1); } }

        public TextureData()
        {
            AnimationSpeed = 20;
            if (this.Texture == null) this.Texture = new TextureMeta();
        }

        public TextureData(string name)
            :base()
        {
            this.Texture = new TextureMeta() { Name = name };
        }

        //Property Grid editing Purpose
        public class TextureDataTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    TextureData d = (TextureData)value;
                    return d.Texture.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
