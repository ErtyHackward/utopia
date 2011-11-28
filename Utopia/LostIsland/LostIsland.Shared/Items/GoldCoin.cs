using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Engines.Shared.Math;
using System;

namespace LostIsland.Shared.Items
{
    /// <summary>
    /// Represents a gold coin entity
    /// </summary>
    public sealed class GoldCoin : SpriteItem, ITool
    {
        private ILandscapeManager2D _landscapeManager;

        public GoldCoin()
        {
            UniqueName = DisplayName;
            Format = SpriteFormat.Billboard;
            Type = EntityType.Static;
        }

        public GoldCoin(ILandscapeManager2D landscapeManager)
            :this()
        {
            _landscapeManager = landscapeManager;
        }

        public override int MaxStackSize
        {
            get { return 100000; }
        }

        public override string Description
        {
            get { return "A coin made of gold. Very valuable thing."; }
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.GoldCoin; }
        }

        public override string DisplayName
        {
            get { return "Gold coin"; }
        }


        public IToolImpact Use(IDynamicEntity owner, byte useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == 1)
            {
                if (owner.EntityState.IsBlockPicked == true)
                {
                    IChunkLayout2D chunk = _landscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                    //Create a new version of the Grass, and put it into the world
                    var cubeEntity = (IItem)EntityFactory.Instance.CreateEntity(this.ClassId);
                    cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f, owner.EntityState.PickedBlockPosition.Y + 1f, owner.EntityState.PickedBlockPosition.Z + 0.5f);

                    chunk.Entities.Add(cubeEntity);

                    impact.Success = true;
                }
            }
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
