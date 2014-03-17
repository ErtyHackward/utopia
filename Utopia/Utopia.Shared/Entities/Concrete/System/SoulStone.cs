using ProtoBuf;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete.System
{
    [ProtoContract]
    [EditorHide]
    [Description("Entity that will be use as player soulstone")]
    public class SoulStone : OrientedBlockItem, ILightEmitterEntity, IOwnerBindable
    {
        private ByteColor _emittedLightColor = new ByteColor(255, 255, 255);

        [ProtoMember(1)]
        [Browsable(false)]
        public int DynamicEntityOwnerID { get; set;}

        public SoulStone()
        {
            GroupName = "System entities";
            Name = "SoulStone";
            IsSystemEntity = true;
        }

        [ProtoMember(2)]
        [Browsable(false)]
        public ByteColor EmittedLightColor
        {
            get { return _emittedLightColor; }
            set { _emittedLightColor = value; }
        }

        [DisplayName("EmittedLightColor")]
        public Color EditorColor
        {
            get
            {
                return Color.FromArgb(_emittedLightColor.A, _emittedLightColor.R, _emittedLightColor.G,
                                      _emittedLightColor.B);
            }
            set { _emittedLightColor = new ByteColor(value.R, value.G, value.B, value.A); }
        }


        /// <summary>
        /// Executes put operation
        /// Removes one item from the inventory and puts it into 
        /// the world
        /// </summary>
        /// <param name="owner">entity that runs the operation</param>
        /// <returns></returns>
        public override IToolImpact Put(IDynamicEntity owner, out Item worldDroppedItem)
        {
            var putResult = base.Put(owner, out worldDroppedItem);

            if (putResult.Success && worldDroppedItem != null)
            {
                //The soulStone has been placed into the world !
                //Bind this entity with the player.

                var charEntity = owner as CharacterEntity;
                charEntity.BindedSoulStone = worldDroppedItem as SoulStone;
            }

            return putResult;      
        }

    }
}
