using System;
using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    /// <remarks></remarks>
    [ProtoContract]
    [EditorHide]
    public sealed class PlayerCharacter : RpgCharacterEntity
    {
        public static float DefaultMoveSpeed = 5f;
        
        /// <summary>
        /// List of player toolbar 
        /// Each items represents the BlueprintId of an item
        /// </summary>
        [ProtoMember(1, OverwriteList = true)]
        public List<ushort> Toolbar { get; set; }

        /// <summary>
        /// Tells the last inventory take position
        /// This allows to prevent mixing of items in the inventory
        /// On switch tool put the previous active tool to that position (if possible)
        /// </summary>
        [ProtoMember(2)]
        public Vector2I ActiveToolInventoryPosition { get; set; }

        public PlayerCharacter()
        {
            //Define the default PlayerCharacter ToolBar
            Toolbar = new List<ushort>();
            for (int i = 0; i < 10; i++)
            {
                Toolbar.Add(0);
            }

            MoveSpeed = DefaultMoveSpeed;               //Default player MoveSpeed
            RotationSpeed = 10f;          //Default Player Rotation Speed
            DefaultSize = new Vector3(0.5f, 1.9f, 0.5f); //Default player size
            
            BodyRotation = Quaternion.Identity;
            ModelName = "Girl";
            Name = "Player";
        }
        
        public IToolImpact ToolUse()
        {
            return ToolUse((ITool)Equipment.RightTool);
        }

        public IToolImpact HandUse()
        {
            return ToolUse(HandTool);
        }

        public IToolImpact PutUse()
        {
            if (Equipment.RightTool != null)
            {
                var args = EntityUseEventArgs.FromState(this);
                args.Tool = Equipment.RightTool;
                args.UseType = UseType.Put;
                args.Impact = Equipment.RightTool.Put(this);

                OnUse(args);

                return args.Impact;
            }
            return new ToolImpact { Message = "RightTool is null" };
        }

        public IItem LookupItem(uint itemId)
        {
            if (itemId == 0) return null;
            foreach (var slot in Inventory)
            {
                if (slot.Item.StaticId == itemId) return slot.Item;
            }

            var equipmentSlot = Equipment.Find(itemId);

            if (equipmentSlot != null)
                return equipmentSlot.Item;

            return null;
        }

        /// <summary>
        /// Returns appropriate container from the player and provides correct position inside the container
        /// </summary>
        /// <param name="link"></param>
        /// <param name="position"></param>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        public SlotContainer<ContainedSlot> FindContainer(EntityLink link, Vector2I position, out Vector2I newPosition)
        {
            
            newPosition = position;

            if (link.IsPointsTo(this))
            {
                if (position.X == -1)
                {
                    newPosition.X = 0;
                    return Equipment;
                }
                return Inventory;
            }
            if (link.IsStatic)
            {
                var entity = link.ResolveStatic(EntityFactory.LandscapeManager);
                if (entity == null)
                    return null;
                return (entity as Container).Content;
            }
            return null;
        }

        public IToolImpact ReplayUse(EntityUseMessage msg)
        {
            EntityState = msg.State;

            switch (msg.UseType)
                {
                    case UseType.Use:
                        if (msg.ToolId != 0)
                        {
                            var tool = FindItemById(msg.ToolId) as ITool;

                            if (tool == null)
                                return new ToolImpact { Message = "This item is not the tool" };

                            return ToolUse(tool);
                        }
                        return ToolUse(HandTool);
                    case UseType.Put:
                        return PutUse();
                    case UseType.Craft:
                        return CraftUse(msg.RecipeIndex);
                    case UseType.Command:
                        return new ToolImpact();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        public bool ReplayTransfer(ItemTransferMessage itm)
        {
            #region Switch
            if (itm.IsSwitch)
            {
                var srcPosition = itm.SourceContainerSlot;
                var dstPosition = itm.DestinationContainerSlot;

                var srcContainer = FindContainer(itm.SourceContainerEntityLink, srcPosition, out srcPosition);
                var dstContainer = FindContainer(itm.DestinationContainerEntityLink, dstPosition, out dstPosition);

                if (srcContainer == null || dstContainer == null)
                {
                    return false;
                }

                // switching is allowed only if we have both slots busy
                var srcSlot = srcContainer.PeekSlot(srcPosition);
                var dstSlot = dstContainer.PeekSlot(dstPosition);

                if (srcSlot == null || dstSlot == null)
                {
                    return false;
                }

                if (!srcContainer.TakeItem(srcSlot.GridPosition, srcSlot.ItemsCount))
                {
                    return false;
                }
                if (!dstContainer.TakeItem(dstSlot.GridPosition, dstSlot.ItemsCount))
                {
                    return false;
                }
                if (!srcContainer.PutItem(dstSlot.Item, srcSlot.GridPosition, dstSlot.ItemsCount))
                {
                    return false;
                }
                if (!dstContainer.PutItem(srcSlot.Item, dstSlot.GridPosition, srcSlot.ItemsCount))
                {
                    return false;
                }

                // ok
                return true;
            }
            #endregion

            if (itm.SourceContainerSlot.X == -2)
            {
                // set toolbar slot
                var item = FindItemById(itm.ItemEntityId);

                if (item == null)
                    return false;

                Toolbar[itm.SourceContainerSlot.Y] = item.BluePrintId;
                return true;
            }

            ContainedSlot slot;
            if (TakeItem(itm, out slot))
            {
                if (PutItem(itm, slot))
                {
                    // ok
                    return true;
                }
                if (!RollbackItem(itm, slot))
                    return false;
            }

            // impossible to transfer
            return false;
        }

        private bool TakeItem(ItemTransferMessage itemTransferMessage, out ContainedSlot slot)
        {
            slot = null;
            
            var position = itemTransferMessage.SourceContainerSlot;
            var srcLink = itemTransferMessage.SourceContainerEntityLink;

            // item from nowhere
            if (srcLink.IsEmpty)
            {
                slot = new ContainedSlot { 
                    Item = (IItem)EntityFactory.CreateFromBluePrint((ushort)itemTransferMessage.ItemEntityId), 
                    ItemsCount = itemTransferMessage.ItemsCount
                };
                return true;
            }

            // detect the container
            var container = FindContainer(srcLink, position, out position);

            if (container == null)
                return false;

            slot = container.PeekSlot(position);

            if (!container.TakeItem(position, itemTransferMessage.ItemsCount))
            {
                slot = null;
                return false;
            }

            slot.ItemsCount = itemTransferMessage.ItemsCount;

            if (!itemTransferMessage.IsSwitch && container == Inventory)
            {
                var pos = itemTransferMessage.DestinationContainerSlot;
                var destContainer = FindContainer(itemTransferMessage.DestinationContainerEntityLink, itemTransferMessage.DestinationContainerSlot, out pos);

                if (destContainer == Equipment)
                {
                    ActiveToolInventoryPosition = itemTransferMessage.SourceContainerSlot;
                }
            }

            return true;
        }

        private bool RollbackItem(ItemTransferMessage itm, Slot slot)
        {
            if (slot != null)
            {
                var position = itm.SourceContainerSlot;
                SlotContainer<ContainedSlot> container = null;
                if (itm.SourceContainerEntityLink.IsPointsTo(this))
                {
                    if (itm.SourceContainerSlot.X == -1)
                    {
                        container = Equipment;
                        position.X = 0;
                    }
                    else
                        container = Inventory;
                }

                if (container != null)
                    container.PutItem(slot.Item, position, slot.ItemsCount);
                else
                    return false;
            }

            return true;
        }

        private bool PutItem(ItemTransferMessage itemTransferMessage, Slot slot)
        {
            // detect the container

            var position = itemTransferMessage.DestinationContainerSlot;
            var destLink = itemTransferMessage.DestinationContainerEntityLink;

            var container = FindContainer(destLink, position, out position);
            
            if (container == null)
                return false;

            return container.PutItem(slot.Item, position, slot.ItemsCount);
        }

    }
}
