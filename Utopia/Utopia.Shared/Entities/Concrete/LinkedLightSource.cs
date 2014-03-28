using System.ComponentModel;
using System.Drawing;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Entity of this type will emit world light. Use it for torches, lamps, candles, etc. This light is block linked")]
    public class LinkedLightSource : BlockLinkedItem, ILightEmitterEntity
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 190, 94);

        [ProtoMember(1)]
        [Browsable(false)]
        public ByteColor EmittedLightColor
        {
            get { return _emittedLightColor; }
            set { _emittedLightColor = value; }
        }

        [Category("LightSource")]
        [DisplayName("EmittedLightColor")]
        public Color EditorColor
        {
            get
            {
                return Color.FromArgb(_emittedLightColor.A, _emittedLightColor.R, _emittedLightColor.G,
                                      _emittedLightColor.B);
            }
            set { _emittedLightColor = new ByteColor(value.R, value.G, value.B, value.A); }
        }
    }
}
