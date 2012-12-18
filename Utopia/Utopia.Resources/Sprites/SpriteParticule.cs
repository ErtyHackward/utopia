using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Particules.ParticulesCol;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Resources.Sprites
{
    public class SpriteParticule : BaseParticule
    {
        public float maxAge;
        public float SizeGrowSpeed;
        public ByteColor ColorModifier;
        public Vector3D InitialPosition;
        public Vector3D AccelerationForce;
        public int ParticuleId;
    }
}
