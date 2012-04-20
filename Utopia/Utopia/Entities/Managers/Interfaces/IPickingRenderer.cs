using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Entities.Voxel;
using S33M3Resources.Structs;
using S33M3DXEngine.Main.Interfaces;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IPickingRenderer : IDrawableComponent
    {
        void SetPickedBlock(ref Vector3I pickedUpCube, double cubeYOffset);
        void SetPickedEntity(VisualEntity pickedEntity);
    }
}
