namespace Utopia.Shared.Structs
{
    public enum ScheduleCallType
    {
        /// <summary>
        /// Task will be executed once and then deleted from manager
        /// </summary>
        Once,
        /// <summary>
        /// Task will be executed periodically
        /// </summary>
        Periodic,
        /// <summary>
        /// Task will be executed each day at time specified
        /// </summary>
        SpecialTime
    }
}