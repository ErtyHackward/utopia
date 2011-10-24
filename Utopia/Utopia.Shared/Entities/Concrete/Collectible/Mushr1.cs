﻿using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Mushr1 : SpriteItem, IBlockLinkedEntity
    {
        #region Private properties
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.Mushroom1; }
        }

        public override string DisplayName
        {
            get { return "Mushroom"; }
        }

        public override string Description
        {
            get { return "Mushroom description"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Mushr1()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Format = SpriteFormat.Billboard;
            Size = new Vector3(0.5f, 0.5f, 0.5f);
        }

        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
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
            writer.Write(LinkedCube.X);
            writer.Write(LinkedCube.Y);
            writer.Write(LinkedCube.Z);
        }
        #endregion

        #region Private methods
        #endregion

    }
}
