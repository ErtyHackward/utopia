using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory;
using System.IO;
using SharpDX;

namespace Utopia.Shared.Chunks.Entities.Concrete.Collectible
{
    public class Grass : SpriteItem, IGrowEntity
    {
        #region Private properties
        private byte _growPhase;
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return true; } }

        public byte GrowPhase
        {
            get { return _growPhase; }
            set { _growPhase = value; GrawPhaseChanged(); }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Grass; }
        }

        public override string DisplayName
        {
            get { return "Grass"; }
        }

        #endregion
        public Grass()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Scale = new Vector3(1, 1, 1);
            GrowPhase = 0; //Set Default Grow Phase
        }

        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            GrowPhase = reader.ReadByte();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(GrowPhase);
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
