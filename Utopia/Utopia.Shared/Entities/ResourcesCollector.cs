﻿using System;
using ProtoBuf;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// The base class use to collect things in the world (= Removed them and put them in the inventory)
    /// </summary>
    [ProtoContract]
    public abstract class ResourcesCollector : Item, ITool
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDynamicEntity _owner;
        private ScheduleTask _task;

        /// <summary>
        /// Using a Collector type Tool Item will start to hit block from world an place it into own bag.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new ToolImpact { Success = false };
            
            if (!EntityFactory.ServerSide)
            {
                // client run
                if (owner.ModelInstance != null)
                {
                    if (owner.EntityState.MouseUp)
                    {
                        owner.ModelInstance.Stop("Dig"); 
                    }
                    else if (owner.EntityState.IsBlockPicked)
                    {
                        owner.ModelInstance.TryPlayForced("Dig", true);                        
                    }
                }
                return impact;
            }

            // server side logic

            _owner = owner;

            if (owner.EntityState.MouseUp)
            {
                if (_task != null)
                {
                    EntityFactory.ScheduleManager.RemoveTask(_task);
                    _task = null;
                }
                
                return impact;
            }

            if (owner.EntityState.IsBlockPicked)
            {
                // tool will start to hit the block each second
                _task = EntityFactory.ScheduleManager.AddPeriodic(EntityFactory.ScheduleManager.Clock.RealToGameSpan(TimeSpan.FromSeconds(1)), BlockHit);
                
                return impact;
            }

            impact.Message = "No target selected for use";
            return impact;
        }

        private void BlockHit()
        {
            var cursor = LandscapeManager.GetCursor(_owner.EntityState.PickedBlockPosition);

            if (cursor.PeekProfile().Hardness == 0)
            {
                //Indestrutible cube, cannot be removed !
                return;
            }

            DamageTag damage;

            var cube = cursor.Read(out damage);
            if (cube != WorldConfiguration.CubeId.Air)
            {
                if (damage == null)
                {
                    damage = new DamageTag { Strength = 15, TotalStrength = 15 };
                }

                damage.Strength--;

                if (damage.Strength <= 0)
                {
                    var chunk = LandscapeManager.GetChunkFromBlock(_owner.EntityState.PickedBlockPosition);
                    
                    chunk.Entities.RemoveAll<BlockLinkedItem>(e => e.LinkedCube == _owner.EntityState.PickedBlockPosition);

                    cursor.Write(WorldConfiguration.CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.
                }
                else
                {
                    cursor.Write(cube, damage);
                }
            }
        }
    }
}
