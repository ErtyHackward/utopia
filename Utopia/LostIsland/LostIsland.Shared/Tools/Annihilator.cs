using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;

namespace LostIsland.Shared.Tools
{
    /// <summary>
    /// Test tool that can remove anything
    /// </summary>
    public class Annihilator : BlockRemover
    {
        private readonly ILandscapeManager2D _landscapeManager;

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Annihilator; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public Annihilator(ILandscapeManager2D landscapeManager)
        {
            _landscapeManager = landscapeManager;
        }

        public override IToolImpact Use(bool runOnServer = false)
        {
            if (Parent.EntityState.IsPickingActive)
            {
                if (Parent.EntityState.IsEntityPicked)
                {
                    return EntityImpact();
                }
                else
                {
                    return BlockImpact();
                }
            }
            else
            {
                var impact = new ToolImpact { Success = false };
                impact.Message = "No target selected for use";
                return impact;
            }
        }

        public override void Rollback(Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }


        private IToolImpact BlockImpact()
        {
            var impact = new ToolImpact { Success = false };
            var cursor = _landscapeManager.GetCursor(Parent.EntityState.PickedBlockPosition);
            byte cube = cursor.Read();
            if (cube != CubeId.Air) 
            {
                //change the Block to AIR
                cursor.Write(CubeId.Air);
                impact.Success = true;

                //Check static entity impact of the Block removal.
                //Get the chunk
                var chunk = _landscapeManager.GetChunk(new Vector2I(Parent.EntityState.PickedBlockPosition.X, Parent.EntityState.PickedBlockPosition.Z));

                IBlockLinkedEntity entity;
                for (int entityId = chunk.Entities.Data.Count-1; entityId >= 0; entityId--)
                {
                    entity = chunk.Entities.Data[entityId] as IBlockLinkedEntity;
                    if (entity != null)
                    {
                        //If the linkedCube entity is removed, then remove the entity also.
                        if (entity.LinkedCube == Parent.EntityState.PickedBlockPosition)
                        {
                            chunk.Entities.Data.RemoveAt(entityId);
                            // Add entity on ground or in Inventory
                            //TOTO Enity picking ??
                        }
                    }
                }

                //If the Tool Owner is a player, then Add the resource removed into the inventory
                var character = Parent as CharacterEntity;
                if (character != null)
                {
                    var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
                    adder.CubeId = cube;
                    character.Inventory.PutItem(adder);
                }
                return impact;
            }
            impact.Message = "Cannot remove Air block !";
            return impact;
        }

        private IToolImpact EntityImpact()
        {
            var impact = new ToolImpact { Success = false };
            var cursor = _landscapeManager.GetCursor(Parent.EntityState.PickedBlockPosition);
            byte cube = cursor.Read();
            if (cube != 0)
            {
                cursor.Write(0);
                impact.Success = true;

                var character = Parent as CharacterEntity;
                if (character != null)
                {
                    var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
                    adder.CubeId = cube;

                    character.Inventory.PutItem(adder);
                }

                return impact;
            }
            return impact;
        }

    }

}
