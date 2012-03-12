using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace S33M3_DXEngine.Debug
{
    public static class DXResource
    {
        //DEFINE_GUID(WKPDID_D3DDebugObjectName,0x429b8c22,0x9188,0x4b0c,0x87,0x42,0xac,0xb0,0xbf,0x85,0xc2,0x00);
//        public static readonly Guid WKPDID_D3DDebugObjectName = new Guid(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

//        private static List<GCHandle> _namesString = new List<GCHandle>();
//        public static void SetName(object resource, string Name)
//        {
//#if DEBUG
//            DeviceChild deviceChild = resource as DeviceChild;
//            if (deviceChild != null)
//            {
//                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
//                byte[] bArray = enc.GetBytes(Name);

//                GCHandle pinnedArray = GCHandle.Alloc(bArray, GCHandleType.Pinned);
//                IntPtr stringptr = pinnedArray.AddrOfPinnedObject();
//                if (stringptr != IntPtr.Zero)
//                {
//                    _namesString.Add(pinnedArray);
//                    deviceChild.SetPrivateData(WKPDID_D3DDebugObjectName, bArray.Length, stringptr);
//                }
//            }
//#endif
//        }

//        public static void CleanUpSetNames()
//        {
//#if DEBUG
//            foreach (GCHandle strings in _namesString) strings.Free();
//            _namesString.Clear();
//#endif
//        }
    }
}
