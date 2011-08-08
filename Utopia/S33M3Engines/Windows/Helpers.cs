using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace S33M3Engines.Windows
{
    public static class Helpers
    {
        public static unsafe int SmartGetHashCode(object obj)
        {
            int num3;
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            try
            {
                int num4 = Marshal.SizeOf(obj);
                int num2 = 0;
                int num = 0;
                for (int* numPtr = (int*)handle.AddrOfPinnedObject().ToPointer(); (num2 + 4) <= num4; numPtr++)
                {
                    num ^= numPtr[0];
                    num2 += 4;
                }
                num3 = (num == 0) ? 0x7fffffff : num;
            }
            finally
            {
                handle.Free();
            }
            return num3;
        }

        public static CultureInfo CultureOfCurrentLayout()
        {
            StringBuilder sb = new StringBuilder(9);
            StringBuilder sbKLID = new StringBuilder();

            if (UnsafeNativeMethods.GetKeyboardLayoutName(sbKLID))
            {
                int klid = int.Parse(sbKLID.ToString().Substring(8), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

                // strip all but the bottom half of the number
                klid &= 0xffff;

                return new CultureInfo(klid, false);
            }

            return (null);
        }
    }
}
