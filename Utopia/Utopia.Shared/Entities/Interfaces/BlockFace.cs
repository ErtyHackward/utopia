using System;

namespace Utopia.Shared.Entities.Interfaces
{
    [Flags]
    public enum BlockFace : byte
    {
        Top = 0x1,
        Sides = 0x2,
        Bottom = 0x4,
        Any = 0x1 | 0x2 | 0x4
    }
}