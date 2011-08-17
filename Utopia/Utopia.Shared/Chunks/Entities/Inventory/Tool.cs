using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public class Tool : Item
    {

        /// <summary>
        /// nullable CubeID : Some tools like blockadder work with a cubeID
        /// </summary>
        public Nullable<byte> CubeID { get; set; } 

    }
}
