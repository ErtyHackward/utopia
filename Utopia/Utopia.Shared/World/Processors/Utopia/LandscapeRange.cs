using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.Processors.Utopia
{
    /// <summary>
    /// Landscape parameters class
    /// </summary>
    public class LandscapeRange : IBinaryStorable
    {
        /// <summary>
        /// The Range of the concerned Landscape, must be between 0 and 1
        /// </summary>
        public double Size { get; set; }
        /// <summary>
        /// An associed color for the range
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// The Range Name (Flat, Montains, ...)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The transition zone from the landscape to the "previous" one
        /// </summary>
        public double MixedPreviousArea { get; set; }
        /// <summary>
        /// The transition zone from the landscape to the "next" one
        /// </summary>
        public double MixedNextArea { get; set; }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(Size);
            writer.Write(Color.A);
            writer.Write(Color.R);
            writer.Write(Color.G);
            writer.Write(Color.B);
            writer.Write(Name);
            writer.Write(MixedPreviousArea);
            writer.Write(MixedNextArea);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            Size = reader.ReadDouble();
            Color = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            Name = reader.ReadString();
            MixedPreviousArea = reader.ReadDouble();
            MixedNextArea = reader.ReadDouble();
        }
    }
}
