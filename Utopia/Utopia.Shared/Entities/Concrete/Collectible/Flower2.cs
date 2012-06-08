using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower2 : CubePlaceableSpriteItem, IBlockLinkedEntity
    {
        #region Private properties
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public override string StackType
        {
            get { return this.GetType().Name; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Flower2; }
        }

        public override string DisplayName
        {
            get { return "Flower 2"; }
        }

        public override string Description
        {
            get { return "Flower 2"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Flower2()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Size = new Vector3(0.7f, 0.7f, 0.7f);
            Format = SpriteFormat.Cross;
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

        #region Private methods
        #endregion

    }
}
