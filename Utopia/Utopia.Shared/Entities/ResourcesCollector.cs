using System;
using ProtoBuf;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// The base class use to collect things in the world (= Removed them and put them in the inventory)
    /// </summary>
    [ProtoContract]
    public abstract class ResourcesCollector : Item, ITool, IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory entityFactory { get; set; }

        #region Private methods
        private IToolImpact BlockImpact(IDynamicEntity owner, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new ToolImpact { Success = false };

            //Remove block and all attached entities to this blocks !
            var character = owner as CharacterEntity;

            var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
            var cube = cursor.Read();
            if (cube != WorldConfiguration.CubeId.Air)
            {
                var chunk = LandscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                if (character != null)
                {
                    foreach (var chunkEntity in chunk.Entities.EnumerateFast())
                    {
                        IBlockLinkedEntity cubeBlockLinkedEntity = chunkEntity as IBlockLinkedEntity;
                        if (cubeBlockLinkedEntity != null && cubeBlockLinkedEntity.LinkedCube == owner.EntityState.PickedBlockPosition)
                        {
                            //Insert in the inventory the entity that will be removed !
                            var adder = (IItem)entityFactory.CreateFromBluePrint(chunkEntity.BluePrintId);
                            character.Inventory.PutItem(adder);
                        }
                    }
                }

                //Removed all entities from collection that where linked to this removed cube !
                chunk.Entities.RemoveAll<IBlockLinkedEntity>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition);

                //change the Block to AIR
                cursor.Write(WorldConfiguration.CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.

                //Add the removed cube into the inventory
                impact.Success = true;

                return impact;
            }

            impact.Message = "Pick a cube to use this tool";
            return impact;
        }

        private IToolImpact EntityImpact(IDynamicEntity owner, bool runOnServer = false)
        {
            var impact = new ToolImpact { Success = false };

            EntityLink entity = owner.EntityState.PickedEntityLink;
            IChunkLayout2D chunk = LandscapeManager.GetChunk(entity.ChunkPosition);
            IStaticEntity entityRemoved;

            //Remove the entity from chunk
            chunk.Entities.RemoveById(entity.Tail[0], owner.DynamicId, out entityRemoved);

            var character = owner as CharacterEntity;
            if (character != null && entityRemoved != null)
            {
                //Create a new entity of the same clicked one and place it into the inventory
                var adder = (IItem)entityFactory.CreateFromBluePrint(entityRemoved.BluePrintId);
                character.Inventory.PutItem(adder);
            }
            return impact;
        }
        #endregion

        #region Public methods
        //Using a Collector type Tool Item will remove then selected entities (or block) from world an place it into own bag.
        public IToolImpact Use(IDynamicEntity owner, bool runOnServer = false)
        {
            if (owner.EntityState.IsBlockPicked)
            {
                return BlockImpact(owner, runOnServer);
            }
            else if (owner.EntityState.IsEntityPicked)
            {
                return EntityImpact(owner, runOnServer);
            }

            var impact = new ToolImpact { Success = false };
            impact.Message = "No target selected for use";
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
