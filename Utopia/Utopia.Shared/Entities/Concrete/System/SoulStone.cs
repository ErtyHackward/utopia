using ProtoBuf;
using S33M3Resources.Structs;
using System.ComponentModel;
using System.Drawing;
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
        public uint DynamicEntityOwnerID { get; set;}

        [ProtoMember(2)]
        [Browsable(false)]
        public ByteColor EmittedLightColor
        {
            get { return _emittedLightColor; }
            set { _emittedLightColor = value; }
        }

        [Category("LightSource")]
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

        public SoulStone()
        {
            GroupName = "System entities";
            Name = "SoulStone";
            IsSystemEntity = true;
        }

        /// <summary>
        /// Executes put operation
        /// Removes one item from the inventory and puts it into 
        /// the world
        /// </summary>
        /// <param name="owner">entity that runs the operation</param>
        /// <returns></returns>
        public override IToolImpact Put(IDynamicEntity owner, Item worldDroppedItem = null)
        {
            //Is the player already binded to a soulstone ?
            var charEntity = owner as CharacterEntity;
            if (charEntity.BindedSoulStone != null)
            {
                IToolImpact impact = new ToolImpact();
                impact.Message = "You are already binded to a soulstone.";
                impact.Success = false;
                return impact;
            }
            
            if (worldDroppedItem == null)
            {
                SoulStone clonedNewSoulstone = (SoulStone)this.Clone();
                clonedNewSoulstone.DynamicEntityOwnerID = charEntity.DynamicId;
                worldDroppedItem = clonedNewSoulstone as Item;
            }

            var putResult = base.Put(owner, worldDroppedItem);

            if (putResult.Success && worldDroppedItem != null)
            {
                //The soulStone has been placed into the world !
                //Bind this entity with the player.
                charEntity.BindedSoulStone = worldDroppedItem as SoulStone;
                DynamicEntityOwnerID = charEntity.DynamicId;
            }

            return putResult;      
        }

        public override void BeforeDestruction(IDynamicEntity destructor)
        {
            var charEntity = destructor as CharacterEntity;
            if (charEntity.DynamicId == this.DynamicEntityOwnerID)
            {
                //Unbind the soulstone from player
                charEntity.BindedSoulStone = null;
            }

            base.BeforeDestruction(destructor);
        }

    }
}
