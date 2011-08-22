namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents a null entity (no any entity)
    /// </summary>
    public class NoEntity : Entity
    {
        public override string DisplayName
        {
            get { return "Empty"; }
        }

        public override EntityClassId ClassId
        {
            get
            {
                return EntityClassId.None;
            }
        }

        public static void SaveEmpty(System.IO.BinaryWriter writer)
        {
            writer.Write((ushort)EntityClassId.None);
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // only in this class we no need to call base members because none of any data should be saved

            writer.Write((ushort)EntityClassId.None);
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            // only in this class we no need to call base members because none of any data should be saved

            // skip the null byte
            reader.ReadUInt16();
        }
    }
}
