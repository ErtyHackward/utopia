﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3DXEngine.Textures
{
    public static class ArrayTextureHelper
    {
        public static int GetMipSize(int mipSlice, int baseSliceSize)
        {
            float size = (float)baseSliceSize;

            while (mipSlice > 0)
            {
                size = MathHelper.Floor(size / 2.0f);
                mipSlice--;
            }

            return (int)size;
        }

        public static int CalcSubresource(int MipSlice, int ArraySlice, int Miplevels)
        {
            return MipSlice + ArraySlice * Miplevels;
        }
    }
}
