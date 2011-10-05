﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Shared.Sprites;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    public abstract class SpriteItem : SpriteEntity, IItem
    {
        #region Private variables
        #endregion

        #region Public variables
        public EquipmentSlotType AllowedSlots { get; set; }
        //public SpriteTexture Icon { get; set; }
        //public Rectangle? IconSourceRectangle  { get; set; }
        public int MaxStackSize { get; private set; }
        public string UniqueName { get; set; }
        #endregion

        #region Public Methods
        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(UniqueName);
        }
        #endregion

        #region Private methods
        #endregion


    }
}
