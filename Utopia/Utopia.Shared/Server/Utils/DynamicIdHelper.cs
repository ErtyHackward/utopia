namespace Utopia.Shared.Server.Utils
{
    /// <summary>
    /// Server side helper to generate unique id for dynamic entities
    /// </summary>
    public static class DynamicIdHelper
    {
        private static uint _currentId;

        static object _syncRoot = new object();

        public static uint MaximumId
        {
            get { return _currentId; }
        }

        public static uint GetNextUniqueId()
        {
            lock (_syncRoot)
                return ++_currentId;
        }

        public static void SetMaxExistsId(uint id)
        {
            lock (_syncRoot)
                _currentId = id;
        }

    }
}
