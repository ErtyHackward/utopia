using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Structs
{
    public struct Size<T>
    {
        public T Width;
        public T Height;

        public Size(T Width, T Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }
}
