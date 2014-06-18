using System;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// This structure is used in DynamicEntity Update method
    /// </summary>
    public struct DynamicUpdateState
    {
        public UtopiaTimeSpan ElapsedTime;
        public TimeSpan RealTime;
        public UtopiaTime CurrentTime;
    }
}
