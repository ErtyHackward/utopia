using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using System;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [EditorHideAttribute]
    public class CubeResource : Item, ITool
    {
        [ProtoMember(1)]
        public byte CubeId { get; set; }

        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(2)]
        public bool RepeatedActionsAllowed { get; set; }

        public override IToolImpact Put(IDynamicEntity owner, Item worldDroppedItem = null)
        {
            // don't allow to put out the cube resource
            return new ToolImpact { Message = "This action is not allowed by design" };
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            IToolImpact impact;

            if (!CanDoBlockAction(owner, out impact))
                return impact;
            
            return BlockImpact(owner);
        }

        public IToolImpact BlockImpact(IDynamicEntity owner, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new BlockToolImpact { SrcBlueprintId = BluePrintId };

            if (entity.EntityState.IsBlockPicked)
            {
                //Do Dynamic entity collision testing (Cannot place a block if a dynamic entity intersect.
                var blockBB = new BoundingBox(entity.EntityState.NewBlockPosition, entity.EntityState.NewBlockPosition + Vector3.One);
                foreach (var dynEntity in EntityFactory.DynamicEntityManager.EnumerateAround(entity.EntityState.NewBlockPosition))
                {
                    var dynBB = new BoundingBox(dynEntity.Position.AsVector3(), dynEntity.Position.AsVector3() + dynEntity.DefaultSize);
                    if (blockBB.Intersects(ref dynBB))
                    {
                        impact.Message = "Cannot place a block where someone is standing";
                        return impact;
                    }
                }

                // Get the chunk where the entity will be added and check if another block static entity is present inside this block
                var workingchunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.NewBlockPosition);
                if (workingchunk == null)
                {
                    //Impossible to find chunk, chunk not existing, event dropped
                    impact.Message = "Chunk is not existing, event dropped";
                    impact.Dropped = true;
                    return impact;
                }
                foreach (var staticEntity in workingchunk.Entities.OfType<IBlockLocationRoot>())
                {
                    if (staticEntity.BlockLocationRoot == entity.EntityState.NewBlockPosition)
                    {
                        impact.Message = "There is something there, remove it first " + staticEntity.BlockLocationRoot;
                        return impact;
                    }
                }

                //Add new block
                var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                if (cursor == null)
                {
                    //Impossible to find chunk, chunk not existing, event dropped
                    impact.Message = "Block not existing, event dropped";
                    impact.Dropped = true;
                    return impact;
                }
                if (cursor.Read() == WorldConfiguration.CubeId.Air)
                {
                    if (!EntityFactory.Config.IsInfiniteResources)
                    {
                        var charEntity = owner as CharacterEntity;
                        if (charEntity == null)
                        {
                            impact.Message = "Character entity is expected";
                            return impact;
                        }

                        var slot = charEntity.Inventory.FirstOrDefault(s => s.Item.StackType == StackType);

                        if (slot == null)
                        {
                            // we have no more items in the inventory, remove from the hand
                            slot = charEntity.Equipment[EquipmentSlotType.Hand];
                            impact.Success = charEntity.Equipment.TakeItem(slot.GridPosition);
                        }
                        else
                        {
                            impact.Success = charEntity.Inventory.TakeItem(slot.GridPosition);
                        }

                        if (!impact.Success)
                        {
                            impact.Message = "Unable to take an item from the inventory";
                            return impact;
                        }
                    }

                    cursor.Write(CubeId);
                    impact.Success = true;
                    impact.CubeId = CubeId;

                    if (SoundEngine != null && EntityFactory.Config.ResourcePut != null)
                    {
                        SoundEngine.StartPlay3D(EntityFactory.Config.ResourcePut, entity.EntityState.NewBlockPosition + new Vector3(0.5f));
                    }

                    return impact;
                }
            }
            impact.Message = "Pick a cube to use this tool";
            return impact;
        }
    }

    public class CubeChangedEventArgs : EventArgs
    {
        public Vector3I Position { get; set; }
        public byte Value { get; set; }
        public IDynamicEntity DynamicEntity { get; set; }
    }
}
