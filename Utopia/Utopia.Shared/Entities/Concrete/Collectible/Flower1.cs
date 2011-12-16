using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower1 : CubePlaceableSpriteItem, IBlockLinkedEntity
    {
        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.Flower1; }
        }

        public override string DisplayName
        {
            get { return "Flower"; }
        }

        public override string Description
        {
            get { return "Flower description"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion

        public Flower1()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Format = SpriteFormat.Billboard;
            Size = new Vector3(0.3f, 0.3f, 0.3f);
        }
        
        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            LinkedCube = reader.ReadVector3I();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(LinkedCube);
        }
        #endregion
    }
}
