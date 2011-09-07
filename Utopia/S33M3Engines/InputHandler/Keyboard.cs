using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.Windows;

namespace S33M3Engines.InputHandler
{
    [StructLayout(LayoutKind.Sequential, Size = 256)]
    internal unsafe struct KeyByteArray
    {
        public fixed byte Keys[256];
    }

    public static class Keyboard
    {
        public static unsafe KeyboardState GetState()
        {
            KeyByteArray keys;

            if (!UnsafeNativeMethods.GetKeyboardState((byte*)&keys))
            {
                throw new Exception("Error getting keyboard state!");
            }

            KeyboardState state = new KeyboardState();

            for (int i = 0; i < 256; i++)
            {
                byte key = keys.Keys[i];

                if ((key & 0x80) != 0)
                {
                    state.AddPressedKey(i);
                }
            }
            return state;
        }
    }


}
