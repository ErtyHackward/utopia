using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using SharpDX;
using System.ComponentModel;
using System.Globalization;

namespace Utopia.Shared.Entities
{
    public enum EntityParticuleType
    {
        None,
        Billboard
    }

    /// <summary>
    /// Class that will store data in case of an entity can emit particules
    /// </summary>
    [TypeConverter(typeof(EntityParticuleConverter))]
    [ProtoContract]
    public partial struct EntityParticule
    {
        [ProtoMember(1)]
        public EntityParticuleType ParticuleType { get; set; }
        [ProtoMember(2)]
        public int ParticuleId { get; set; }
        [ProtoMember(3)]
        public Vector3 EmitVelocity { get; set; }
        [ProtoMember(4)]
        public Vector3 AccelerationForces { get; set; }
        [ProtoMember(5)]
        public Vector3 PositionOffset { get; set; }
    }
}
