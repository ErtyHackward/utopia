using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Grass : CubePlaceableSpriteItem, IBlockLinkedEntity
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
        public Grass(ILandscapeManager2D landscapeManager)
            : base(landscapeManager)
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            GrowPhase = 0; //Set Default Grow Phase
        }

        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            GrowPhase = reader.ReadByte();
            Vector3I linkedCube = new Vector3I();
            linkedCube.X = reader.ReadInt32();
            linkedCube.Y = reader.ReadInt32();
            linkedCube.Z = reader.ReadInt32();
            LinkedCube = linkedCube;
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(GrowPhase);
            writer.Write(LinkedCube.X);
            writer.Write(LinkedCube.Y);
            writer.Write(LinkedCube.Z);
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
