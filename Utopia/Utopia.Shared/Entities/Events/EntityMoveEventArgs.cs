using System;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Events
{
    public class EntityMoveEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }

        public Vector3D PreviousPosition { get; set; }
    }
}