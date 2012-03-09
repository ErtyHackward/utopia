﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Chunks;
using S33M3_CoreComponents.Physics.Verlet;
using S33M3_Resources.Structs;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntityPickingManager
    {
        bool isDirty { get; set; }
        PlayerEntityManager Player { get; set; }
        IWorldChunks WorldChunks { get; set; }
        bool CheckEntityPicking(ref Ray pickingRay, out VisualEntity pickedEntity);
        void isCollidingWithEntity(VerletSimulator physicSimu,ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);
    }
}
