using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Interfaces;

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

        public override Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            var entity = Parent;
            var impact = new ToolImpact { Success = false };

            if (entity.EntityState.IsPickingActive)
            {
                if (entity.EntityState.IsEntityPicked)
                {

                }
                else
                {
                    var cursor = _landscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    byte cube = cursor.Read();
                    if (cube != 0)
                    {
                        cursor.Write(0);
                        impact.Success = true;

                        var character = entity as CharacterEntity;
                        if (character != null)
                        {
                            var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
                            adder.CubeId = cube;

                            character.Inventory.PutItem(adder);
                        }

                        return impact;
                    }
                }
            }
            impact.Message = "Pick a cube to use this tool";
            return impact;
        }

        public override void Rollback(Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }
    }

}
