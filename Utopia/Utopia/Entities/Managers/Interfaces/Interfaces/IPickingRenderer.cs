using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Entities.Voxel;

namespace Utopia.Entities.Renderer.Interfaces
{
    public interface IPickingRenderer : IDrawableComponent
    {
        void SetPickedBlock(ref Vector3I pickedUpCube);
        void SetPickedEntity(IVisualEntityContainer pickedEntity);
    }
}
