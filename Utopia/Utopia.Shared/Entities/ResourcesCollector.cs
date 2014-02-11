using ProtoBuf;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

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
        [ProtoMember(1)]
        public int Damage { get; set; }

        /// <summary>
        /// Using a Collector type Tool Item will start to hit block from world an place it into own bag.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public IToolImpact Use(IDynamicEntity owner)
        {
            if (!owner.EntityState.IsBlockPicked)
            {
                return new ToolImpact { 
                    Message = "Expected picked block" 
                };
            }
            
            return BlockHit(owner);
        }

        private IToolImpact BlockHit(IDynamicEntity owner)
        {
            var impact = new ToolImpact();
            
            var cursor = LandscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);

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
                var hardness = cursor.PeekProfile().Hardness;

                if (damage == null)
                {
                    damage = new DamageTag {
                        Strength = (int)hardness,
                        TotalStrength = (int)hardness
                    };
                }

                damage.Strength -= Damage;

                if (damage.Strength <= 0)
                {
                    var chunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.PickedBlockPosition);
                    chunk.Entities.RemoveAll<BlockLinkedItem>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition);
                    cursor.Write(WorldConfiguration.CubeId.Air);
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
}
