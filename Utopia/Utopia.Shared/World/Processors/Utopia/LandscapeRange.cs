using System.Drawing;
using ProtoBuf;

namespace Utopia.Shared.World.Processors.Utopia
{
    /// <summary>
    /// Landscape parameters class
    /// </summary>
    [ProtoContract]
    public class LandscapeRange
    {
        /// <summary>
        /// The Range of the concerned Landscape, must be between 0 and 1
        /// </summary>
        [ProtoMember(1)]
        public double Size { get; set; }

        /// <summary>
        /// An associed color for the range
        /// </summary>
        public Color Color { get; set; }

        [ProtoMember(2)]
        public int ColorSerialized
        {
            get { return Color.ToArgb(); }
            set { Color = Color.FromArgb(value); }
        }

        /// <summary>
        /// The Range Name (Flat, Montains, ...)
        /// </summary>
        [ProtoMember(3)]
        public string Name { get; set; }

        /// <summary>
        /// The transition zone from the landscape to the "previous" one
        /// </summary>
        [ProtoMember(4)]
        public double MixedPreviousArea { get; set; }
        
        /// <summary>
        /// The transition zone from the landscape to the "next" one
        /// </summary>
        [ProtoMember(5)]
        public double MixedNextArea { get; set; }

    }
}
