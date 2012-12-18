using System.ComponentModel;
using System.Drawing;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class LightSource : BlockLinkedItem, ILightEmitterEntity
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 190, 94);

        [ProtoMember(1)]
        [Browsable(false)]
        public ByteColor EmittedLightColor
        {
            get { return _emittedLightColor; }
            set { _emittedLightColor = value; }
        }

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
        
        /// <summary>
        /// Gets entity class id
        /// </summary>
        public override ushort ClassId
        {
            get { return EntityClassId.LightSource; }
        }

        public LightSource()
        {
            MountPoint = BlockFace.Sides;
        }
    }
}
