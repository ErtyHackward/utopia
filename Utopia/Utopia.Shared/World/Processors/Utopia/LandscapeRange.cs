using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia
{
    /// <summary>
    /// Landscape parameters class
    /// </summary>
    public class LandscapeRange
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
    }
}
