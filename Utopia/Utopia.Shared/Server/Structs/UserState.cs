using System.IO;

namespace Utopia.Shared.Server.Structs
{
    public struct UserState
    {
        private uint _entityId;
        /// <summary>
        /// Players entity id
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        public byte[] Save()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);

                writer.Write(EntityId);

                return ms.ToArray();
            }
        }

        public static UserState Load(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return new UserState();

            using (var ms = new MemoryStream(bytes))
            {
                var reader = new BinaryReader(ms);

                UserState state;

                state._entityId = reader.ReadUInt32();
                
                return state;
            }
        }
    }
}
