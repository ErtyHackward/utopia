using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Voxel;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntityPickingManager
    {
        PlayerEntityManager Player { get; set; }
        bool CheckEntityPicking(ref Vector3D pickingPoint, out IVisualEntityContainer pickedEntity);
        void isCollidingWithEntity(ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);
    }
}
