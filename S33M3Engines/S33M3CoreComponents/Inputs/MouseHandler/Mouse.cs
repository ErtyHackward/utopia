using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using System.Windows.Forms;
using S33M3CoreComponents.Unsafe;
using SharpDX;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.Inputs.MouseHandler
{
    public class Mouse : BaseComponent
    {
        // Fields
        private MouseMessageHooker _mouseMessageHooker;

        public IntPtr SetMouseMessageHooker
        {
            set
            {
                if (_mouseMessageHooker == null) ToDispose(_mouseMessageHooker = new MouseMessageHooker());
                _mouseMessageHooker.WindowHandle = value;
            }
        }

        // Methods
        public unsafe MouseState GetState()
        {
            POINT gpoint;
            MouseState state = new MouseState();
            UnsafeNativeMethods.GetCursorPos(&gpoint);
            if (D3DEngine.WindowHandle != null)
            {
                UnsafeNativeMethods.ScreenToClient(D3DEngine.WindowHandle, &gpoint);
            }
            state.x = *((int*)&gpoint);
            state.y = *((int*)(&gpoint) + 1);
            ButtonState state6 = (ButtonState)(((ushort)UnsafeNativeMethods.GetAsyncKeyState(Keys.LButton)) >> 15);
            state.leftButton = state6;
            ButtonState state5 = (ButtonState)(((ushort)UnsafeNativeMethods.GetAsyncKeyState(Keys.MButton)) >> 15);
            state.middleButton = state5;
            ButtonState state4 = (ButtonState)(((ushort)UnsafeNativeMethods.GetAsyncKeyState(Keys.RButton)) >> 15);
            state.rightButton = state4;
            ButtonState state3 = (ButtonState)(((ushort)UnsafeNativeMethods.GetAsyncKeyState(Keys.XButton1)) >> 15);
            state.xb1 = state3;
            ButtonState state2 = (ButtonState)(((ushort)UnsafeNativeMethods.GetAsyncKeyState(Keys.XButton2)) >> 15);
            state.xb2 = state2;
            state.wheel = MouseMessageHooker.CurrentWheel;
            return state;
        }

        public unsafe void SetPosition(int x, int y)
        {
            POINT gpoint;
            *((int*)&gpoint) = x;
            *((int*)(&gpoint) + 1) = y;
            if (D3DEngine.WindowHandle != null)
            {
                UnsafeNativeMethods.ClientToScreen(D3DEngine.WindowHandle, &gpoint);
            }
            UnsafeNativeMethods.SetCursorPos(*((int*)&gpoint), *((int*)(&gpoint)+1));
        }
    }

}
