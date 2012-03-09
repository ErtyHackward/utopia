﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Interfaces;
using S33M3_DXEngine.Main.Interfaces;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IDynamicEntityManager : IDrawableComponent
    {
        void AddEntity(IDynamicEntity entity);
        void RemoveEntity(IDynamicEntity entity);
        void RemoveEntityById(uint entityId,bool dispose=true);
        IDynamicEntity GetEntityById(uint p);

        List<IVisualEntityContainer> DynamicEntities { get; set; }
    }

}
