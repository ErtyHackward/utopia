using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.GUI.TopPanel
{
    public class EnergyBar : PanelControl
    {
        //Graphics properties
        public TimeSpan TimeFromOldToNewValue { get; set; }

        /// <summary>
        /// The current value of the Bar, in [0 to 1] range. 1 = Full, 0 = Empty
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// The new value we are aiming to
        /// </summary>
        public float NewValue { get; set; }
    }
}
