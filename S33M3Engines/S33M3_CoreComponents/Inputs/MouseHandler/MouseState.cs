using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace S33M3_CoreComponents.Inputs.MouseHandler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseState
    {
        internal int x;
        internal int y;
        internal ButtonState leftButton;
        internal ButtonState rightButton;
        internal ButtonState middleButton;
        internal ButtonState xb1;
        internal ButtonState xb2;
        internal int wheel;
        public MouseState(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2)
        {
            this.x = x;
            this.y = y;
            this.wheel = scrollWheel;
            this.leftButton = leftButton;
            this.rightButton = rightButton;
            this.middleButton = middleButton;
            this.xb1 = xButton1;
            this.xb2 = xButton2;
        }

        public int X
        {
            get
            {
                return this.x;
            }
        }
        public int Y
        {
            get
            {
                return this.y;
            }
        }
        public ButtonState LeftButton
        {
            get
            {
                return this.leftButton;
            }
        }
        public ButtonState RightButton
        {
            get
            {
                return this.rightButton;
            }
        }
        public ButtonState MiddleButton
        {
            get
            {
                return this.middleButton;
            }
        }
        public ButtonState XButton1
        {
            get
            {
                return this.xb1;
            }
        }
        public ButtonState XButton2
        {
            get
            {
                return this.xb2;
            }
        }
        public int ScrollWheelValue
        {
            get
            {
                return this.wheel;
            }
        }
        public int ScrollWheelTicks
        {
            get
            {
                return this.wheel / 120;
            }
        }
        public override int GetHashCode()
        {
            return (((((((this.x.GetHashCode() ^ this.y.GetHashCode()) ^ this.leftButton.GetHashCode()) ^ this.rightButton.GetHashCode()) ^ this.middleButton.GetHashCode()) ^ this.xb1.GetHashCode()) ^ this.xb2.GetHashCode()) ^ this.wheel.GetHashCode());
        }

        public override string ToString()
        {
            string str = string.Empty;
            if (this.leftButton == ButtonState.Pressed)
            {
                str = str + (string.IsNullOrEmpty(str) ? "" : " ") + "Left";
            }
            if (this.rightButton == ButtonState.Pressed)
            {
                str = str + (string.IsNullOrEmpty(str) ? "" : " ") + "Right";
            }
            if (this.middleButton == ButtonState.Pressed)
            {
                str = str + (string.IsNullOrEmpty(str) ? "" : " ") + "Middle";
            }
            if (this.xb1 == ButtonState.Pressed)
            {
                str = str + (string.IsNullOrEmpty(str) ? "" : " ") + "XButton1";
            }
            if (this.xb2 == ButtonState.Pressed)
            {
                str = str + (string.IsNullOrEmpty(str) ? "" : " ") + "XButton2";
            }
            if (string.IsNullOrEmpty(str))
            {
                str = "None";
            }
            return string.Format(CultureInfo.CurrentCulture, "{{X:{0} Y:{1} Buttons:{2} Wheel:{3}}}", new object[] { this.x, this.y, str, this.wheel });
        }

        public override bool Equals(object obj)
        {
            return ((obj is MouseState) && (this == ((MouseState)obj)));
        }

        public static bool operator ==(MouseState left, MouseState right)
        {
            return (((((left.x == right.x) && (left.y == right.y)) && ((left.leftButton == right.leftButton) && (left.rightButton == right.rightButton))) && (((left.middleButton == right.middleButton) && (left.xb1 == right.xb1)) && (left.xb2 == right.xb2))) && (left.wheel == right.wheel));
        }

        public static bool operator !=(MouseState left, MouseState right)
        {
            return !(left == right);
        }
    }


}
