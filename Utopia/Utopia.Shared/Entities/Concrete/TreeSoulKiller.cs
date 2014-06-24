using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Allows to unmount any pickeable entity
    /// </summary>
    [ProtoContract]
    [Description("Stops tree activity (regenerate blocks, spawn new items, die)")]
    public class TreeSoulKiller : Item, ITool
    {
        [Category("Gameplay")]
        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(1)]
        public bool RepeatedActionsAllowed { get; set; }

        public IToolImpact Use(IDynamicEntity owner)
        {
            IToolImpact checkImpact;

            if (!CanDoBlockAction(owner, out checkImpact))
                return checkImpact;

            var impact = new BlockToolImpact
            {
                SrcBlueprintId = BluePrintId
            };

            var cursor = LandscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);
            
            if (cursor == null)
            {
                impact.Dropped = true;
                return impact;
            }

            cursor.OwnerDynamicId = owner.DynamicId;

            var selectedCube = cursor.Read();

            var treeSystem = new TreeLSystem();
            bool removed = false;
            foreach (var soul in EntityFactory.LandscapeManager.AroundChunks(owner.Position, 16).SelectMany(c => c.Entities.Enumerate<TreeSoul>()))
            {
                var treeBlueprint = EntityFactory.Config.TreeBluePrintsDico[soul.TreeTypeId];
                
                if (selectedCube != treeBlueprint.TrunkBlock && selectedCube != treeBlueprint.FoliageBlock)
                    continue;

                var blocks = treeSystem.Generate(soul.TreeRndSeed, BlockHelper.EntityToBlock(soul.Position), treeBlueprint);

                if (blocks.Any(b => b.WorldPosition == owner.EntityState.PickedBlockPosition))
                {
                    cursor.RemoveEntity(soul.GetLink());
                    removed = true;
                    break;
                }
            }

            if (!removed)
            {
                impact.Message = "This is not an alive tree";
                return impact;
            }

            TakeFromPlayer(owner);
            return impact;
        }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            return PickType.Pick;
        }
    }
}
