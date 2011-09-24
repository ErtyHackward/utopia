using System;
using Utopia.Server.Managers;
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
        public LandscapeManager LandscapeManager { get; private set; }

        public CubeToolLogic(LandscapeManager manager)
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
                if(entity.EntityState.IsBlockPicked)
                {
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    if (cursor.Read() != 0)
                    {
                        cursor.Write(0);
                        impact.Success = true;
                        return impact;
                    }
                }
                impact.Message = "Can not erase an empty block";
                return impact;
            }

            if (callerTool is DirtAdder)
            {
                if (entity.EntityState.IsBlockPicked)
                {
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                    if (cursor.Read() == 0)
                    {
                        cursor.Write(CubeId.Dirt);
                        impact.Success = true;
                        return impact;
                    }
                }
                impact.Message = "Can not set to non-empty block";
                return impact;
            }

            throw new InvalidOperationException(string.Format("CubeToolLogic can not be used with {0} tool type", callerTool.GetType()));
        }
    }
}
