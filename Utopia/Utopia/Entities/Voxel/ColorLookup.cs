﻿using System;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Generates a lookup array for 8 bits color
    /// No need to optimize it's purely static !
    /// </summary>
    public static class ColorLookup
    {
        public static readonly ByteColor[] Colours;

        /// <summary>
        /// Palette of 64 nice colours
        /// </summary>
        private static readonly string[] _palette = new string[] {
            "ffffffff","ffdbdbdb","ffb7b7b7","ff929292","ff6e6e6e","ff494949","ff252525","ff000000",
            "ffff0000","ffff2400","ffff4800","ffff6d00","ffff9100","ffffb600","ffffda00","ffffff00",
            "ffffff00","ffdbed00","ffb7db00","ff92c900","ff6eb700","ff49a500","ff259300","ff008000",
            "ff008000","ff006e24","ff005c48","ff004a6d","ff003791","ff0025b6","ff0013da","ff0000ff",
            "ff0000ff","ff2400db","ff4800b7","ff6d0092","ff91006e","ffb60049","ffda0025","ffff0000",
            "fff9decc","fff5d7c4","fff1d0bb","ffecc9b3","ffe8c2aa","ffe3bba2","ffdfb499","ffdaad90",
            "ffffffff","ffdbdbff","ffb7b7ff","ff9292ff","ff6e6eff","ff4949ff","ff2525ff","ff0000ff",
            "ffc08e6c","ff9a7156","ff735440","ff4c3729","ffff0000","ffff0055","ffff00aa","ffff00ff"
        };

        static ColorLookup()
        {
            Colours = new ByteColor[_palette.Length];
            for (int i = 0; i < _palette.Length; i++)
            {
                Colours[i] = HexStringToColor(_palette[i]);
            }
        }


        private static ByteColor HexStringToColor(string hexColor)
        {
            string a = hexColor.Substring(0, 2);
            string r = hexColor.Substring(2, 2);
            string g = hexColor.Substring(4, 2);
            string b = hexColor.Substring(6, 2);
            int ai = Int32.Parse(a, System.Globalization.NumberStyles.HexNumber);
            int ri = Int32.Parse(r, System.Globalization.NumberStyles.HexNumber);
            int gi = Int32.Parse(g, System.Globalization.NumberStyles.HexNumber);
            int bi = Int32.Parse(b, System.Globalization.NumberStyles.HexNumber);
            return new ByteColor(ri, gi, bi, ai);
        }

    }
}
