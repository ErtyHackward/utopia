using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D.Tools
{
    public static class Resource
    {
        public static int CalcSubresource(int MipSlice, int ArraySlice, int Miplevels)
        {
            return MipSlice + ArraySlice * Miplevels;
        }
    }
}
