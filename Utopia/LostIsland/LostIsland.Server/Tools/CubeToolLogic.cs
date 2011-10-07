using System;
using Utopia.Server.Managers;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Cubes;

namespace Utopia.Server.Tools
{
    /// <summary>
    /// Common tool logic for cube adder and cube removers
    /// </summary>
    public class CubeToolLogic : IToolLogic
    {
        public ServerLandscapeManager LandscapeManager { get; private set; }

        public CubeToolLogic(ServerLandscapeManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            LandscapeManager = manager;
        }

        /// <summary>
        /// Perform cube modification
        /// </summary>
        /// <param name="callerTool"></param>
        /// <returns></returns>
        public IToolImpact Use(Tool callerTool)
        {
            var entity = callerTool.Parent;
            var impact = new ToolImpact { Success = false };
            if (callerTool is Annihilator)
            {
                if (entity.EntityState.IsPickingActive)
                {
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    byte cube = cursor.Read();
                    if (cube != 0)
                    {
                        cursor.Write(0);
                        impact.Success = true;

                        CharacterEntity character = entity as CharacterEntity;
                        if (character != null)
                        {
                            BlockAdder adder = (BlockAdder)EntityFactory.Instance.CreateEntity(EntityClassId.BlockAdder);
                            adder.CubeId = cube;

                            character.Inventory.PutItem(adder);
                        }

                        return impact;
                    }
                }
                impact.Message = "Pick a cube to use this tool";
                return impact;
            }

            if (callerTool is BlockAdder)
            {
                if (entity.EntityState.IsPickingActive)
                {
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                    if (cursor.Read() == 0)
                    {
                        cursor.Write(((BlockAdder)callerTool).CubeId);
                        impact.Success = true;
                        return impact;
                    }
                }
                impact.Message = "Pick a cube to use this tool";
                return impact;
            }

            if (callerTool is Shovel)
            {
                if (entity.EntityState.IsPickingActive)
                {
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);

                    var blockValue = cursor.Read();
                    if (blockValue == CubeId.Dirt || blockValue == CubeId.Grass)
                    {
                        cursor.Write(0);
                        impact.Success = true;
                        return impact;
                    }

                    impact.Message = "Shovel can only dig dirt and grass";
                    return impact;
                    
                }
                impact.Message = "Pick a cube to use this tool";
                return impact;
            }

            throw new InvalidOperationException(string.Format("CubeToolLogic can not be used with {0} tool type", callerTool.GetType()));
        }
    }
}
