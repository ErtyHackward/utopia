using System;

namespace Utopia.Shared.Entities.Interfaces
{
    [Flags]
    public enum BlockFace : byte
    {
        Top = 0x1,
        Sides = 0x2,
        Bottom = 0x4,
        TopAndBottom = Top | Bottom,
        Any = Top | Sides | Bottom,
    }
}