using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Cactus : CubePlaceableSpriteItem, IBlockLinkedEntity
    {
        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return true; } }
        public Vector3I LinkedCube { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.Cactus; }
        }

        public override string DisplayName
        {
            get { return "Cactus"; }
        }

        public override string Description
        {
            get { return "Cactus description"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion

        public Cactus()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Format = SpriteFormat.Cross;
            Size = new Vector3(1f, 2f, 1f);
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
