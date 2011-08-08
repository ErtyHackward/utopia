using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.Windows
{
    internal abstract class WindowMessageHooker : IDisposable
    {
        // Fields
        private static uint clobberDetectionMessage = SafeNativeMethods.RegisterWindowMessage(typeof(WindowMessageHooker).FullName);
        private Hook currentHook;
        private static List<Hook> deadHooks = new List<Hook>();
        protected const int VK_HOME = 0x24;
        protected const int WM_ACTIVATE = 6;
        protected const int WM_CHAR = 0x102;
        protected const int WM_IME_CHAR = 0x286;
        protected const int WM_IME_COMPOSITION = 0x10f;
        protected const int WM_IME_ENDCOMPOSITION = 270;
        protected const int WM_IME_NOTIFY = 0x282;
        protected const int WM_IME_SETCONTEXT = 0x281;
        protected const int WM_IME_STARTCOMPOSITION = 0x10d;
        protected const int WM_INPUTLANGCHANGE = 0x51;
        protected const int WM_KEYDOWN = 0x100;
        protected const int WM_KEYUP = 0x101;
        protected const int WM_LBUTTONDBLCLK = 0x203;
        protected const int WM_LBUTTONDOWN = 0x201;
        protected const int WM_LBUTTONUP = 0x202;
        protected const int WM_MBUTTONDBLCLK = 0x209;
        protected const int WM_MBUTTONDOWN = 0x207;
        protected const int WM_MBUTTONUP = 520;
        protected const int WM_MOUSEMOVE = 0x200;
        protected const int WM_MOUSEWHEEL = 0x20a;
        protected const int WM_NCACTIVATE = 0x86;
        protected const int WM_RBUTTONDBLCLK = 0x206;
        protected const int WM_RBUTTONDOWN = 0x204;
        protected const int WM_RBUTTONUP = 0x205;
        protected const int WM_SETCURSOR = 0x20;
        protected const int WM_XBUTTONDBLCLK = 0x20d;
        protected const int WM_XBUTTONDOWN = 0x20b;
        protected const int WM_XBUTTONUP = 0x20c;

        // Methods
        protected WindowMessageHooker()
        {
        }

        private static void CollectDeadHooks()
        {
            for (int i = deadHooks.Count - 1; i >= 0; i--)
            {
                if (deadHooks[i].TryRemove())
                {
                    deadHooks.RemoveAt(i);
                    i = deadHooks.Count;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.RemoveCurrentHook();
            CollectDeadHooks();
        }

        ~WindowMessageHooker()
        {
            this.Dispose(false);
        }

        private void RemoveCurrentHook()
        {
            if (this.currentHook != null)
            {
                if (this.currentHook.TryRemove())
                {
                    CollectDeadHooks();
                }
                else
                {
                    this.currentHook.isHookRemoved = true;
                    deadHooks.Add(this.currentHook);
                }
                this.currentHook = null;
            }
        }

        public void Update()
        {
            if ((this.currentHook != null) && !this.currentHook.isWindowDestroyed)
            {
                this.currentHook.seenClobberDetectionMessage = false;
                SafeNativeMethods.SendMessage(this.currentHook.hWnd, clobberDetectionMessage, IntPtr.Zero, IntPtr.Zero);
                if (!this.currentHook.seenClobberDetectionMessage)
                {
                    this.currentHook.isHookRemoved = true;
                    deadHooks.Add(this.currentHook);
                    this.currentHook = new Hook(this, this.currentHook.hWnd);
                }
            }
        }

        protected abstract IntPtr? WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Properties
        public IntPtr WindowHandle
        {
            get
            {
                if (this.currentHook != null)
                {
                    return this.currentHook.hWnd;
                }
                return IntPtr.Zero;
            }
            set
            {
                if (value != this.WindowHandle)
                {
                    this.RemoveCurrentHook();
                    if (value != IntPtr.Zero)
                    {
                        this.currentHook = new Hook(this, value);
                    }
                }
            }
        }

        // Nested Types
        private class Hook
        {
            // Fields
            public IntPtr hWnd;
            public bool isHookRemoved;
            public bool isWindowDestroyed;
            private WindowMessageHooker parent;
            private IntPtr previousWndProc;
            public bool seenClobberDetectionMessage;
            private WndProcDelegate wndProcDelegate;
            private IntPtr wndProcFunction;

            // Methods
            public Hook(WindowMessageHooker parent, IntPtr hWnd)
            {
                this.parent = parent;
                this.hWnd = hWnd;
                this.previousWndProc = SafeNativeMethods.GetWindowLongPtr(hWnd, -4);
                this.wndProcDelegate = new WndProcDelegate(this.WndProc);
                this.wndProcFunction = SafeNativeMethods.GetFunctionPointerForDelegate(this.wndProcDelegate);
                SafeNativeMethods.SetWindowLongPtr(hWnd, -4, this.wndProcFunction);
            }

            public bool TryRemove()
            {
                if (!this.isWindowDestroyed)
                {
                    if (SafeNativeMethods.GetWindowLongPtr(this.hWnd, -4) != this.wndProcFunction)
                    {
                        return false;
                    }
                    SafeNativeMethods.SetWindowLongPtr(this.hWnd, -4, this.previousWndProc);
                }
                return true;
            }

            private IntPtr WndProc(IntPtr msgWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                if (msg == WindowMessageHooker.clobberDetectionMessage)
                {
                    this.seenClobberDetectionMessage = true;
                    return IntPtr.Zero;
                }
                if (msg == 130)
                {
                    this.isWindowDestroyed = true;
                    for (int i = WindowMessageHooker.deadHooks.Count - 1; i >= 0; i--)
                    {
                        if (WindowMessageHooker.deadHooks[i].hWnd == msgWnd)
                        {
                            WindowMessageHooker.deadHooks.RemoveAt(i);
                        }
                    }
                }
                if (!this.isHookRemoved)
                {
                    IntPtr? nullable = this.parent.WndProc(msgWnd, msg, wParam, lParam);
                    if (nullable.HasValue)
                    {
                        return nullable.Value;
                    }
                }
                return SafeNativeMethods.CallWindowProc(this.previousWndProc, msgWnd, msg, wParam, lParam);
            }

            // Nested Types
            private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
