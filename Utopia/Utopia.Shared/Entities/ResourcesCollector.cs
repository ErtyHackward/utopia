using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// The base class use to collect things in the world (= Removed them and put them in the inventory)
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(BasicCollector))]
    public abstract class ResourcesCollector : Item, ITool
    {
        /// <summary>
        /// Tool block damage
        /// negative values will repair blocks
        /// </summary>
        [Category("Gameplay")]
        [ProtoMember(1)]
        public int Damage { get; set; }

        [Category("Gameplay")]
        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(2)]
        public bool RepeatedActionsAllowed { get; set; }

        [Category("Gameplay")]
        [Description("Allows to set damage for specified types of blocks")]
        [ProtoMember(3, OverwriteList = true)]
        public List<CubeDamage> SpecialDamages { get; set; }

        protected ResourcesCollector()
        {
            SpecialDamages = new List<CubeDamage>();
        }

        /// <summary>
        /// Using a Collector type Tool Item will start to hit block from world an place it into own bag.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public IToolImpact Use(IDynamicEntity owner)
        {
            IToolImpact impact;

            if (!CanDoBlockAction(owner, out impact))
            {
                return impact;
            }
            
            return BlockHit(owner);
        }

        private IToolImpact BlockHit(IDynamicEntity owner)
        {
            var impact = new BlockToolImpact {
                SrcBlueprintId = BluePrintId,
                Position = owner.EntityState.PickedBlockPosition
            };
            
            var cursor = LandscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);
            if (cursor == null)
            {
                //Impossible to find chunk, chunk not existing, event dropped
                impact.Message = "Block not existing, event dropped";
                impact.Dropped = true;
                return impact;
            }
            cursor.OwnerDynamicId = owner.DynamicId;

            if (cursor.PeekProfile().Indestructible)
            {
                impact.Message = "Indestrutible cube, cannot be removed !";
                return impact;
            }

            DamageTag damage;

            var cube = cursor.Read(out damage);
            if (cube != WorldConfiguration.CubeId.Air)
            {
                impact.CubeId = cube;
                var hardness = cursor.PeekProfile().Hardness;

                if (damage == null)
                {
                    damage = new DamageTag {
                        Strength = (int)hardness,
                        TotalStrength = (int)hardness
                    };
                }

                var toolBlockDamage = Damage;

                if (SpecialDamages != null)
                {
                    var index = SpecialDamages.FindIndex(cd => cd.CubeId == cube);

                    if (index != -1)
                        toolBlockDamage = SpecialDamages[index].Damage;
                }

                damage.Strength -= toolBlockDamage;

                if (toolBlockDamage > 0 && SoundEngine != null)
                {
                    var profile = EntityFactory.Config.BlockProfiles[cube];
                    if (profile.HitSounds.Count > 0)
                    {
                        var random = new Random();
                        var sound = profile.HitSounds[random.Next(0, profile.HitSounds.Count)];
                        SoundEngine.StartPlay3D(sound, owner.EntityState.PickedBlockPosition + new Vector3(0.5f));
                    }
                }

                if (damage.Strength <= 0)
                {
                    var chunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.PickedBlockPosition);
                    if (chunk == null)
                    {
                        //Impossible to find chunk, chunk not existing, event dropped
                        impact.Message = "Chunk is not existing, event dropped";
                        impact.Dropped = true;
                        return impact;
                    }
                    chunk.Entities.RemoveAll<BlockLinkedItem>(e => e.Linked && e.LinkedCube == owner.EntityState.PickedBlockPosition, owner.DynamicId);
                    cursor.Write(WorldConfiguration.CubeId.Air);

                    foreach (var treeSoul in EntityFactory.LandscapeManager.AroundEntities(owner.EntityState.PickedBlockPosition, 16).OfType<TreeSoul>())
                    {
                        var treeBp = EntityFactory.Config.TreeBluePrintsDico[treeSoul.TreeTypeId];

                        if (cube != treeBp.FoliageBlock && cube != treeBp.TrunkBlock)
                            continue;

                        var treeLSystem = new TreeLSystem();

                        var treeBlocks = treeLSystem.Generate(treeSoul.TreeRndSeed, (Vector3I)treeSoul.Position, treeBp);

                        // did we remove the block of the tree?
                        if (treeBlocks.Exists(b => b.WorldPosition == owner.EntityState.PickedBlockPosition))
                        {
                            treeSoul.IsDamaged = true;

                            // count removed trunk blocks
                            var totalTrunks = treeBlocks.Count(b => b.BlockId == treeBp.TrunkBlock);

                            var existsTrunks = treeBlocks.Count(b =>
                            {
                                if (b.BlockId == treeBp.TrunkBlock)
                                {
                                    cursor.GlobalPosition = b.WorldPosition;
                                    return cursor.Read() == treeBp.TrunkBlock;
                                }
                                return false;
                            });

                            if (existsTrunks < totalTrunks / 2)
                            {
                                treeSoul.IsDying = true;
                            }
                        }
                    }

                    

                    if (SoundEngine != null && EntityFactory.Config.ResourceTake != null)
                    {
                        SoundEngine.StartPlay3D(EntityFactory.Config.ResourceTake, owner.EntityState.PickedBlockPosition + new Vector3(0.5f));
                    }
                    
                    var charEntity = owner as CharacterEntity;
                    if (charEntity == null)
                    {
                        impact.Message = "Charater entity is expected";
                        return impact;
                    }
                    
                    // in case of infinite resources we will not add more than 1 block entity 
                    var existingSlot = charEntity.FindSlot(s => s.Item.BluePrintId == cube);

                    if (!EntityFactory.Config.IsInfiniteResources || existingSlot == null)
                    {
                        if (!charEntity.Inventory.PutItem((IItem)EntityFactory.CreateFromBluePrint(cube)))
                            impact.Message = "Can't put the item to inventory";
                    }

                    impact.CubeId = WorldConfiguration.CubeId.Air;
                }
                else if (damage.Strength >= hardness)
                {
                    cursor.Write(cube);
                }
                else
                {
                    cursor.Write(cube, damage);
                }

                impact.Success = true;
                return impact;
            }

            impact.Message = "Cannot hit air block";
            return impact;
        }

        public override object Clone()
        {
            var collector = (ResourcesCollector)base.Clone();

            collector.SpecialDamages = new List<CubeDamage>(SpecialDamages);

            return collector;
        }
    }

    [ProtoContract]
    public struct CubeDamage
    {
        [TypeConverter(typeof(CubeSelector))]
        [ProtoMember(1)]
        public byte CubeId { get; set; }

        [ProtoMember(2)]
        public int Damage { get; set; }

        public override string ToString()
        {
            return string.Format("{0} => {1}", CubeId, Damage);
        }
    }
}
