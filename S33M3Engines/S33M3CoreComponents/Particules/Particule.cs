using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;

namespace S33M3CoreComponents.Particules
{
    public class Particule
    {
        public Vector3 Velocity;
        public Vector2 Size;
        public Vector3D Position;
        public float Age;

        public Particule(Vector3D position,Vector3 velocity, Vector2 size)
        {
            Age = 0;
            Size = size;
            Velocity = velocity;
            Position = position;
        }

    }
}
