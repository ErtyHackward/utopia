﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// The base class use to collect things in the world (= Removed them and put them in the inventory)
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(BasicCollector))]
    public abstract class ResourcesCollector : Item, ITool, ISoundEmitterEntity
    {
        public ISoundEngine SoundEngine { get; set; }

        /// <summary>
        /// Tool block damage
        /// negative values will repair blocks
        /// </summary>
        [ProtoMember(1)]
        public int Damage { get; set; }

        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(2)]
        public bool RepeatedActionsAllowed { get; set; }

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

            if (cursor.PeekProfile().Hardness == 0)
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
                        SoundEngine.StartPlay3D(sound, owner.EntityState.PickedBlockPosition);
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
                    chunk.Entities.RemoveAll<BlockLinkedItem>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition, owner.DynamicId);
                    cursor.Write(WorldConfiguration.CubeId.Air);
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
