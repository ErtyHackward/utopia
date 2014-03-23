using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using SharpDX;
using Color = System.Drawing.Color;
using System.ComponentModel;
using System.Globalization;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    public enum EntityParticuleType
    {
        Billboard
    }

    /// <summary>
    /// Class that will store data needed for a static entity to emit particules
    /// </summary>
    [ProtoContract]
    public class StaticEntityParticule
    {
        private ByteColor _particuleColor = new ByteColor(255, 255, 255);

        [ProtoMember(1)]
        [Description("Particule Type")]
        public EntityParticuleType ParticuleType { get; set; }
        [ProtoMember(2)]
        [Description("Texture ID of the particule")]
        public int ParticuleId { get; set; }
        [ProtoMember(3)]
        [Description("Force applied to the Entity")]
        public Vector3 EmitVelocity { get; set; }
        [ProtoMember(4)]
        [Description("Accelerated Force")]
        public Vector3 AccelerationForces { get; set; }
        [ProtoMember(5)]
        [Description("Startup Offset from entity center (0;0;0)")]
        public Vector3 PositionOffset { get; set; }
        [ProtoMember(6)]
        [Description("Apply windforce constant")]
        public bool ApplyWindForce { get; set; }
        [ProtoMember(7)]
        [Description("Startup particule size")]
        public Vector2 Size { get; set; }
        [ProtoMember(8)]
        [Description("SizeGrowSpeed in Unit/sec")]
        public float SizeGrowSpeed { get; set; }
        [ProtoMember(9 , IsRequired=true)]
        [Browsable(false)]
        public ByteColor ParticuleColor
        {
            get { return _particuleColor; }
            set { _particuleColor = value; }
        }
        [Description("Color Modifier value for the sprite")]
        [DisplayName("Color Modifier")]
        public Color Color
        {
            get
            {
                return Color.FromArgb(_particuleColor.A, _particuleColor.R, _particuleColor.G,
                                      _particuleColor.B);
            }
            set { _particuleColor = new ByteColor(value.R, value.G, value.B, value.A); }
        }
        [Description("Particule lifeTime in seconds")]
        [ProtoMember(10)]
        public float ParticuleLifeTime { get; set; }
        [Description("Rate at wish the particules are emitted in sec.")]
        [ProtoMember(11)]
        public float EmittedParticuleRate { get; set; }
        [Description("Amount of particules emitted at once")]
        [ProtoMember(12)]
        public int EmittedParticulesAmount { get; set; }
        [ProtoMember(13)]
        [Description("Amount of randomness for to the velocity (+/-)")]
        public Vector3 EmitVelocityRandomness { get; set; }
        [ProtoMember(14)]
        [Description("Amount of randomness for to the particule lifetime (+/-)")]
        public float ParticuleLifeTimeRandomness { get; set; }
        [ProtoMember(15)]
        [Description("Amount of randomness for to startup position of the particules (+/-)")]
        public Vector3 PositionRandomness { get; set; }
        [ProtoMember(16)]
        [Description("Fading base on particule life time. 0 = No fading. The value is the Power base value. (1 = Linear)")]
        public double AlphaFadingPowBase { get; set; }
    }
}
