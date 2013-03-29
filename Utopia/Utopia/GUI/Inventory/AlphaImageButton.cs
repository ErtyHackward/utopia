using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Special image button with different alpha values for each state
    /// </summary>
    public class AlphaImageButtonControl : ButtonControl
    {
        /// <summary>
        /// Alpha level of default state [0; 1]
        /// </summary>
        public float AlphaDefault { get; set; }

        /// <summary>
        /// Alpha level of hover state [0; 1]
        /// </summary>
        public float AlphaHover { get; set; }

        /// <summary>
        /// Alpha level of down state [0; 1]
        /// </summary>
        public float AlphaDown { get; set; }

        /// <summary>
        /// Alpha level of disabled state [0; 1]
        /// </summary>
        public float AlphaDisabled { get; set; }
    }
}
