using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class SideLightSource : CubePlaceableItem, ILightEmitterEntity
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 190, 94);

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
