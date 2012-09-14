using System.IO;
using SharpDX;

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
        public Color4[] BlockColors { get; set; }

        public static ColorMapping Read(BinaryReader reader)
        {
            var colorMappingLength = reader.ReadByte();

            ColorMapping colorMapping = null;

            if (colorMappingLength > 0)
            {
                colorMapping = new ColorMapping();

                colorMapping.BlockColors = new Color4[colorMappingLength];

                for (var i = 0; i < colorMappingLength; i++)
                {
                    colorMapping.BlockColors[i] = (Color4)reader.ReadInt32();
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
                    writer.Write((int)t);
                }
            }
        }
    }
}