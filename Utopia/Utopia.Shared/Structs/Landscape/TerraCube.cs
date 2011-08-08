using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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
            EmissiveColor.SunLight = 0;
        }

        public byte[] RawSerialize()
        {
            int rawSize = Marshal.SizeOf(this);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(this, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
        }

        public void RawDeserialize(byte[] rawData)
        {
            int position = 0;
            Type type = typeof(TerraCube);
            int rawsize = Marshal.SizeOf(type);
            if (rawsize > rawData.Length) return;

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            this = (TerraCube)Marshal.PtrToStructure(buffer, type);
            Marshal.FreeHGlobal(buffer);
        }
    }
}
