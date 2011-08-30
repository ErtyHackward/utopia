using System;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityLeaveAreaEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
        public Vector3 PreviousPosition { get; set; }
    }
}