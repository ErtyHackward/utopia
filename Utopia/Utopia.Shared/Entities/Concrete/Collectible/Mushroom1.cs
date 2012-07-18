﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;
using System.IO;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Mushroom1 : CubePlaceableItem, IBlockLinkedEntity
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
            get { return EntityClassId.Mushroom1; }
        }

        public override string DisplayName
        {
            get { return "Mushroom"; }
        }

        public override string Description
        {
            get { return "Mushroom !"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Mushroom1()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            //DefaultSize = new Vector3(0.7f, 0.7f, 0.7f); //If not specified than the voxel body will be use for sizing
            ModelName = "Mushroom1";
            RndCreationYAxisRotation = true;
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