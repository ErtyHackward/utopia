using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Particules.ParticulesCol;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Resources.Sprites
{
    public class ColoredParticule : BaseParticule
    {
        public bool isFrozen;
        public float computationAge;
        public bool wasColliding;
        public Color ParticuleColor;
        public ByteColor ColorReceived;
        public Vector3D InitialPosition;
        public Quaternion SpinningRotation;
        public FTSValue<Quaternion> RotationAngles;
    }
}
