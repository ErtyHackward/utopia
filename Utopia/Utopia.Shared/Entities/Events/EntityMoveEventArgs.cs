using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityMoveEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }

        public Vector3D PreviousPosition { get; set; }
    }
}