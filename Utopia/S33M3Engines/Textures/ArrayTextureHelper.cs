using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Textures
{
    public static class ArrayTextureHelper
    {
        public static int GetMipSize( int mipSlice, int baseSliceSize )
	    {
		    float size = (float)baseSliceSize;
		
		    while( mipSlice > 0 )
		    {
                size = MathHelper.Fastfloor(size / 2.0f);
                mipSlice--;
		    }
		
		    return (int)size;
	    }
    }
}
