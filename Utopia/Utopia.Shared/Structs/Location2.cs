using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Structs
{
    public struct Location2<T>
    {
        public T X;
        public T Z;

        public Location2(T X, T Z)
        {
            this.X = X;
            this.Z = Z;
        }
    }
}
