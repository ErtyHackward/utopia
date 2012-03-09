using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Grass : CubePlaceableSpriteItem, IBlockLinkedEntity, IGrowEntity
    {
        #region Private properties
        private byte _growPhase;
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public byte GrowPhase
        {
            get { return _growPhase; }
            set { _growPhase = value; GrawPhaseChanged(); }
        }

        public override string StackType
        {
            get { return this.GetType().Name + GrowPhase; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Grass; }
        }

        public override string DisplayName
        {
            get { return "Grass"; }
        }

        public override string Description
        {
            get { return "Juicy green grass. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Grass()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            GrowPhase = 0; //Set Default Grow Phase
        }

        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            GrowPhase = reader.ReadByte();
            LinkedCube = reader.ReadVector3I();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(GrowPhase);
            writer.Write(LinkedCube);
        }
        #endregion

        #region Private methods
        private void GrawPhaseChanged()
        {
            switch (GrowPhase)
            {
                default:
                        Size = new Vector3(0.7f, 0.7f, 0.7f);
                        Format = SpriteFormat.Triangle;
                    break;
            }
        }
        #endregion

    }
}
