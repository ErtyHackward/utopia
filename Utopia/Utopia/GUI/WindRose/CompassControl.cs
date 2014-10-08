using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.Sprites2D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.GUI.WindRose
{
    public class CompassControl : PanelControl
    {
        public SpriteTexture CompassTexture { get; set; }
        public SpriteTexture DayCircle { get; set; }
        public SpriteTexture MaskArrow { get; set; }
        public SpriteTexture SoulStoneIcon { get; set; }
        public float Rotation { get; set; }
        public SamplerState sampler { get; set; }
        public float RotationDayCycle { get; set; }
        public float SoulStoneFacing { get; set; }
    }
}
