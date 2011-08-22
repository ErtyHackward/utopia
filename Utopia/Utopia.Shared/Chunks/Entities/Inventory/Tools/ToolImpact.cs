using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public struct ToolImpact
    {
        public readonly String Message; //for displaying an information, see the survey tool for example
        public readonly TerraCubeWithPosition[] CubesImpact;

        public ToolImpact(TerraCubeWithPosition[] cubesImpact, string message = null)
        {
            Message = message;
            CubesImpact = cubesImpact;
        }

        public ToolImpact(TerraCubeWithPosition cubeImpact, string message = null)
        {
            CubesImpact = new TerraCubeWithPosition[] { cubeImpact };
            Message = message;
        }

        public ToolImpact(string message)
        {
            CubesImpact = null;
            Message = message;

        }
    }
}
