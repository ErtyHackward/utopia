﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Entities.Voxel
{
    public interface IVisualEntityContainer
    {
         VisualVoxelEntity VisualEntity { get; set; }
    }
}