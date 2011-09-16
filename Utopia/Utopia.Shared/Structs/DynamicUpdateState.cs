using System;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// This structure is used in DynamicEntity Update method
    /// </summary>
    public struct DynamicUpdateState
    {
        public TimeSpan ElapsedTime;
        public DateTime CurrentTime;
    }
}
