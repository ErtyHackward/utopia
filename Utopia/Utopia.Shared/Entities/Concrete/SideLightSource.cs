using System.IO;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class SideLightSource : BlockLinkedItem, ILightEmitterEntity
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 190, 94); //Fixed light color ?

        [ProtoMember(1)]
        public ByteColor EmittedLightColor
        {
            get { return _emittedLightColor; }
            set { _emittedLightColor = value; }
        }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        public override ushort ClassId
        {
            get { return EntityClassId.SideLightSource; }
        }

        public SideLightSource()
        {
            MountPoint = BlockFace.Sides;
        }
    }
}
