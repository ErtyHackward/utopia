﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Structs.Landscape
{
    /// <summary>
    /// The default object used by the renderer to memorize Cube dynamic informations
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerraCube
    {
        #region private properties
        public byte Id; //Represent the ID of the cube and it's linked texture in the array

        //Liquid force channel
        public byte MetaData1;  //Liquid Flowing Way Or Height.
        public byte MetaData2;  //Liquid Total Flowing Power
        public byte MetaData3;  //Liquid Flooding direction

        //Lighting channels
        public ByteColor EmissiveColor; //Color received

        #endregion

        public TerraCube(byte Id)
        {
            this.Id = Id;
            MetaData3 = 0;
            MetaData2 = 0;
            MetaData1 = 0;
            EmissiveColor.R = 0;
            EmissiveColor.G = 0;
            EmissiveColor.B = 0;
            EmissiveColor.A = 0;
        }
    }
}
