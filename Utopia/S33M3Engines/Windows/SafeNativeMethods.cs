using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace S33M3Engines.Windows
{
    internal static class SafeNativeMethods
    {
        // Fields
        public const int GWLP_WNDPROC = -4;

        // Methods
        [SecuritySafeCritical, SecurityCritical]
        public static IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return UnsafeNativeMethods.CallWindowProc(lpPrevWndFunc, hWnd, msg, wParam, lParam);
        }

        [SecuritySafeCritical, SecurityCritical]
        public static IntPtr GetFunctionPointerForDelegate(Delegate d)
        {
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        [SecurityCritical, SecuritySafeCritical]
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return UnsafeNativeMethods.GetWindowLong32(hWnd, nIndex);
            }
            return UnsafeNativeMethods.GetWindowLongPtr64(hWnd, nIndex);
        }

        [SecurityCritical, SecuritySafeCritical]
        public static uint RegisterWindowMessage(string lpString)
        {
            return UnsafeNativeMethods.RegisterWindowMessage(lpString);
        }

        [SecurityCritical, SecuritySafeCritical]
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return UnsafeNativeMethods.SendMessage(hWnd, msg, wParam, lParam);
        }

        [SecurityCritical, SecuritySafeCritical]
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newValue)
        {
            if (IntPtr.Size == 4)
            {
                return UnsafeNativeMethods.SetWindowLong32(hWnd, nIndex, newValue);
            }
            return UnsafeNativeMethods.SetWindowLongPtr64(hWnd, nIndex, newValue);
        }
    }

 

}
