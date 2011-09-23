using System;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
using S33M3Engines.Shared.Math;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityMoveEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }

        public Vector3D PreviousPosition { get; set; }
    }
}