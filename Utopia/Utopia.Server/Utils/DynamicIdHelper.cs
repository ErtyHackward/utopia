namespace Utopia.Server.Utils
{
    /// <summary>
    /// Server side helper to generate unique id for dynamic entities
    /// </summary>
    public static class DynamicIdHelper
    {
        private static uint _currentId;

        public static uint MaximumId
        {
            get { return _currentId; }
        }

        public static uint GetNextUniqueId()
        {
            return ++_currentId;
        }

        public static void SetMaxExistsId(uint id)
        {
            _currentId = id;
        }

    }
}
