using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3Resources.Structs.Helpers.Interfaces
{
    public interface IPackedVector
    {
        // Methods
        void PackFromVector4(Vector4 vector);
        Vector4 ToVector4();
    }

    public interface IPackedVector<TPacked> : IPackedVector
    {
        // Properties
        TPacked PackedValue { get; set; }
    }
}
