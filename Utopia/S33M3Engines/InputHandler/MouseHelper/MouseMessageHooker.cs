using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Windows;

namespace S33M3Engines.InputHandler.MouseHelper
{
    class MouseMessageHooker : WindowMessageHooker
    {
        // Fields
        private static int currentWheel;

        // Methods
        unsafe protected override IntPtr? WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == 0x20a)
            {
                int num = (short)((int)wParam.ToPointer() >> 0x10);
                currentWheel += num;
            }
            return null;
        }

        // Properties
        internal static int CurrentWheel
        {
            get
            {
                return currentWheel;
            }
        }
   
    }
}
