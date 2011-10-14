using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls;
using S33M3Engines.Shared.Sprites;

namespace Utopia.GUI.D3D.Map
{
    public class MapControl : Control
    {
        public SpriteTexture MapTexture { get; set; }
        public SpriteTexture PlayerMarker { get; set; }
        public Point MarkerPosition { get; set; }
    }
}
