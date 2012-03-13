using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace S33M3CoreComponents.Inputs.KeyboardHandler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardState
    {
        static uint stateMask0;
        static uint stateMask1;
        static uint stateMask2;
        static uint stateMask3;
        static uint stateMask4;
        static uint stateMask5;
        static uint stateMask6;
        static uint stateMask7;
        uint currentState0;
        uint currentState1;
        uint currentState2;
        uint currentState3;
        uint currentState4;
        uint currentState5;
        uint currentState6;
        uint currentState7;

        static KeyboardState()
        {
            stateMask0 = uint.MaxValue;
            stateMask1 = uint.MaxValue;
            stateMask2 = uint.MaxValue;
            stateMask3 = uint.MaxValue;
            stateMask4 = uint.MaxValue;
            stateMask5 = uint.MaxValue;
            stateMask6 = uint.MaxValue;
            stateMask7 = uint.MaxValue;
            KeyboardState state = new KeyboardState();
            foreach (int num in Enum.GetValues(typeof(Keys)))
            {
                state.AddPressedKey(num);
            }
            stateMask0 = state.currentState0;
            stateMask1 = state.currentState1;
            stateMask2 = state.currentState2;
            stateMask3 = state.currentState3;
            stateMask4 = state.currentState4;
            stateMask5 = state.currentState5;
            stateMask6 = state.currentState6;
            stateMask7 = state.currentState7;
        }

        public KeyboardState(params Keys[] keys)
        {
            this.currentState0 = this.currentState1 = this.currentState2 = this.currentState3 = this.currentState4 = this.currentState5 = this.currentState6 = this.currentState7 = 0;
            if (keys != null)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    this.AddPressedKey((int)keys[i]);
                }
            }
        }

        internal void AddPressedKey(int key)
        {
            // num & stateMask0; ==> Keep only the true bit from num assigned in statmask0 !
            // currentState0 |= value =>>> currentState0 = currentState0 | value ==> "Add the bit set to true in value to currentstate"

            uint num = ((uint)1) << key;
            switch ((key >> 5))
            {
                case 0:
                    this.currentState0 |= num & stateMask0;
                    return;
                case 1:
                    this.currentState1 |= num & stateMask1;
                    return;
                case 2:
                    this.currentState2 |= num & stateMask2;
                    return;
                case 3:
                    this.currentState3 |= num & stateMask3;
                    return;
                case 4:
                    this.currentState4 |= num & stateMask4;
                    return;
                case 5:
                    this.currentState5 |= num & stateMask5;
                    return;
                case 6:
                    this.currentState6 |= num & stateMask6;
                    return;
                case 7:
                    this.currentState7 |= num & stateMask7;
                    return;
            }
        }

        internal void RemovePressedKey(int key)
        {
            uint num = ((uint)1) << key;
            switch ((key >> 5))
            {
                case 0:
                    this.currentState0 &= ~(num & stateMask0);
                    return;
                case 1:
                    this.currentState1 &= ~(num & stateMask1);
                    return;
                case 2:
                    this.currentState2 &= ~(num & stateMask2);
                    return;
                case 3:
                    this.currentState3 &= ~(num & stateMask3);
                    return;
                case 4:
                    this.currentState4 &= ~(num & stateMask4);
                    return;
                case 5:
                    this.currentState5 &= ~(num & stateMask5);
                    return;
                case 6:
                    this.currentState6 &= ~(num & stateMask6);
                    return;
                case 7:
                    this.currentState7 &= ~(num & stateMask7);
                    return;
            }
        }

        public KeyState this[Keys key]
        {
            get
            {
                uint num;
                switch ((((int)key) >> 5))
                {
                    case 0:
                        num = this.currentState0;
                        break;
                    case 1:
                        num = this.currentState1;
                        break;
                    case 2:
                        num = this.currentState2;
                        break;
                    case 3:
                        num = this.currentState3;
                        break;
                    case 4:
                        num = this.currentState4;
                        break;
                    case 5:
                        num = this.currentState5;
                        break;
                    case 6:
                        num = this.currentState6;
                        break;
                    case 7:
                        num = this.currentState7;
                        break;

                    default:
                        return KeyState.Up;
                }
                uint num2 = ((uint)1) << (int)key;
                if ((num & num2) == 0)
                {
                    return KeyState.Up;
                }
                return KeyState.Down;
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return (this[key] == KeyState.Down);
        }

        public bool IsKeyDown(Keys key, Keys KeyModifier)
        {
            return (this[key] == KeyState.Down && this[KeyModifier] == KeyState.Down);
        }

        public bool IsKeyDown(KeyWithModifier key)
        {
            if (key.Modifier != Keys.None)
            {
                return (this[key.MainKey] == KeyState.Down && this[key.Modifier] == KeyState.Down);
            }
            else
            {
                return (this[key.MainKey] == KeyState.Down);
            }
        }

        public bool IsKeyUp(Keys key)
        {
            return (this[key] == KeyState.Up);
        }

        public bool IsKeyUp(Keys key, Keys KeyModifier)
        {
            return (this[key] == KeyState.Up && this[KeyModifier] == KeyState.Down);
        }

        public bool IsKeyUp(KeyWithModifier key)
        {
            if (key.Modifier != Keys.None)
            {
                return (this[key.MainKey] == KeyState.Up && this[key.Modifier] == KeyState.Down);
            }
            else
            {
                return (this[key.MainKey] == KeyState.Up);
            }
        }

        public Keys[] GetPressedKeys()
        {
            int index = 0;
            CheckPressedKeys(this.currentState0, 0, null, ref index);
            CheckPressedKeys(this.currentState1, 0, null, ref index);
            CheckPressedKeys(this.currentState2, 0, null, ref index);
            CheckPressedKeys(this.currentState3, 0, null, ref index);
            CheckPressedKeys(this.currentState4, 0, null, ref index);
            CheckPressedKeys(this.currentState5, 0, null, ref index);
            CheckPressedKeys(this.currentState6, 0, null, ref index);
            CheckPressedKeys(this.currentState7, 0, null, ref index);
            Keys[] pressedKeys = new Keys[index];
            if (index > 0)
            {
                int num2 = 0;
                CheckPressedKeys(this.currentState0, 0, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState1, 1, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState2, 2, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState3, 3, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState4, 4, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState5, 5, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState6, 6, pressedKeys, ref num2);
                CheckPressedKeys(this.currentState7, 7, pressedKeys, ref num2);
            }
            return pressedKeys;
        }

        private static void CheckPressedKeys(uint packedState, int packedOffset, Keys[] pressedKeys, ref int index)
        {
            if (packedState != 0)
            {
                for (int i = 0; i < 0x20; i++)
                {
                    if ((packedState & (((int)1) << i)) != 0L)
                    {
                        if (pressedKeys != null)
                        {
                            pressedKeys[index] = (Keys)((packedOffset * 0x20) + i);
                        }
                        index++;
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            return (((((((this.currentState0.GetHashCode() ^ this.currentState1.GetHashCode()) ^ this.currentState2.GetHashCode()) ^ this.currentState3.GetHashCode()) ^ this.currentState4.GetHashCode()) ^ this.currentState5.GetHashCode()) ^ this.currentState6.GetHashCode()) ^ this.currentState7.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is KeyboardState) && (this == ((KeyboardState)obj)));
        }

        public static bool operator ==(KeyboardState a, KeyboardState b)
        {
            return (((((a.currentState0 == b.currentState0) && (a.currentState1 == b.currentState1)) && ((a.currentState2 == b.currentState2) && (a.currentState3 == b.currentState3))) && (((a.currentState4 == b.currentState4) && (a.currentState5 == b.currentState5)) && (a.currentState6 == b.currentState6))) && (a.currentState7 == b.currentState7));
        }

        public static bool operator !=(KeyboardState a, KeyboardState b)
        {
            return !(a == b);
        }

    }
}
