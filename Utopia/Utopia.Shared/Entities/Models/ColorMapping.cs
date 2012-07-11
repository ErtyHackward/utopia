using System.IO;
using SharpDX;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Defines a color mapping information of a model part
    /// </summary>
    public class ColorMapping
    {
        /// <summary>
        /// Gets colors scheme, maximum 64 items
        /// </summary>
        public ByteColor[] BlockColors { get; set; }

        public static ColorMapping Read(BinaryReader reader)
        {
            var colorMappingLength = reader.ReadByte();

            ColorMapping colorMapping = null;

            if (colorMappingLength > 0)
            {
                colorMapping = new ColorMapping();

                colorMapping.BlockColors = new ByteColor[colorMappingLength];

                for (var i = 0; i < colorMappingLength; i++)
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    byte a = reader.ReadByte();

                    colorMapping.BlockColors[i] = new ByteColor(r, g, b, a);
                }
            }

            return colorMapping;
        }

        public static void Write(BinaryWriter writer, ColorMapping mapping)
        {
            if (mapping == null || mapping.BlockColors.Length == 0)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)mapping.BlockColors.Length);

                foreach (var t in mapping.BlockColors)
                {
                    writer.Write(t.R);
                    writer.Write(t.G);
                    writer.Write(t.B);
                    writer.Write(t.A);
                }
            }
        }
    }
}