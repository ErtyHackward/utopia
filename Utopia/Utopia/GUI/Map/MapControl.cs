using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using S33M3_CoreComponents.Sprites;
using S33M3_CoreComponents.GUI.Nuclex.Controls;

namespace Utopia.GUI.Map
{
    public class MapControl : Control
    {
        public SpriteTexture MapTexture { get; set; }
        public SpriteTexture PlayerMarker { get; set; }
        public Point MarkerPosition { get; set; }
    }
}
