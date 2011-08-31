using System;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityViewEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}
