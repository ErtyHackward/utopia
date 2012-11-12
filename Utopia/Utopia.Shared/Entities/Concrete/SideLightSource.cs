using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    public class SideLightSource : CubePlaceableItem, ILightEmitterEntity
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 190, 94); //Fixed light color ?

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

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            _emittedLightColor = new ByteColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(_emittedLightColor.R);
            writer.Write(_emittedLightColor.G);
            writer.Write(_emittedLightColor.B);
            writer.Write(_emittedLightColor.A);
        }
    }
}
