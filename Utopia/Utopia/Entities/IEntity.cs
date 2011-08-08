﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.D3D;
using S33M3Engines.Cameras;
using S33M3Engines.Maths;

namespace Utopia.Entities
{
    public interface IEntity : IGameComponent, ICameraPlugin
    {
        FTSValue<DVector3> WorldPosition { get; }
        FTSValue<Quaternion> WorldRotation { get; }
        BoundingBox BoundingBox { get; }
    }
}
