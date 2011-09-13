using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IVoxelEntity : IEntity
    {
        byte[, ,] Blocks { get; set;}
        void RandomFill(int emptyProbabilityPercent);
        void PlainCubeFill();
    }
}
