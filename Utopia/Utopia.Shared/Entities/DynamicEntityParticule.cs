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
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities
{

    /// <summary>
    /// Class that will store data needed for a dynamic entity to emit particules
    /// </summary>
    [ProtoContract]
    public class DynamicEntityParticule
    {
        private ByteColor _particuleColor = new ByteColor(255, 255, 255);

        public EntityParticuleType ParticuleType { get; set; }
        public int ParticuleId { get; set; }
        public bool ApplyWindForce { get; set; }
        public Vector2 Size { get; set; }
        public ByteColor ParticuleColor
        {
            get { return _particuleColor; }
            set { _particuleColor = value; }
        }
        public float ParticuleLifeTime { get; set; }
        public float ParticuleLifeTimeRandomness { get; set; }
        public Vector3 EmitVelocityRandomness { get; set; }
        public Vector3 AccelerationForce { get; set; }
    }
}
