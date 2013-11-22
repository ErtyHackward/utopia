using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Structs.Landscape
{
    /// <summary>
    /// The default object used by the renderer to memorize Cube dynamic informations
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerraCube
    {
        public byte Id; //Represent the ID of the cube and it's linked texture in the array
        //Lighting channels
        public ByteColor EmissiveColor; //Color received
        public bool IsSunLightSource;
        

        public TerraCube(byte Id)
        {
            this.Id = Id;
            EmissiveColor.R = 0;
            EmissiveColor.G = 0;
            EmissiveColor.B = 0;
            if (Id == 0)
            {
                EmissiveColor.A = 255;
                IsSunLightSource = true;
            }
            else
            {
                EmissiveColor.A = 0;
                IsSunLightSource = false;
            }
        }
    }
}
