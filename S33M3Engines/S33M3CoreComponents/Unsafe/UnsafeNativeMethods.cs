using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;

namespace S33M3CoreComponents.Unsafe
{
    public static unsafe class UnsafeNativeMethods
    {
        // Methods
        [DllImport("User32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll")]
        public static extern uint RegisterWindowMessage(string lpString);
        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr newValue);
        [DllImport("User32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newValue);
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern unsafe bool GetKeyboardState(byte* lpKeyState);

        #region cursor
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(POINT* lpPoint);
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, POINT* lpPoint);
         [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, POINT* lpPoint);
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);
        #endregion 

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
   
        [DllImport("user32.dll")]
        public static extern bool GetKeyboardLayoutName(StringBuilder pwszKLID);
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int virtualKeyCode);

    //        <StructLayout(LayoutKind.Sequential)> _
    //Public Structure KERNINGPAIR
    //    Public First As Short
    //    Public Second As Short
    //    Public KernAmount As Integer
    //End Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct KERNINGPAIR
        {
            public short First;
            public short Second;
            public int KernAmount;
        }

        [DllImport("gdi32.dll")]
        public static extern int GetKerningPairs(IntPtr hdc, int cPairs, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)][Out()] KERNINGPAIR[] lpkrnpair);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
    }
 

}
