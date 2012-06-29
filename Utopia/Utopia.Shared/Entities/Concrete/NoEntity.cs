using System;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a null entity (no any entity). Used to represent an absence of the entity in the process of serialization
    /// </summary>
    public class NoEntity : Entity
    {
        public override string DisplayName
        {
            get { return "Empty"; }
        }

        public override ushort ClassId
        {
            get
            {
                return EntityClassId.None;
            }
        }

        public static void SaveEmpty(System.IO.BinaryWriter writer)
        {
            writer.Write(EntityClassId.None);
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // only in this class we no need to call base members because none of any data should be saved

            writer.Write(EntityClassId.None);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            // only in this class we no need to call base members because none of any data should be saved

            // skip the null byte
            reader.ReadUInt16();
        }

        /// <summary>
        /// throws NotSupportedException
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public override Structs.EntityLink GetLink()
        {
            throw new NotSupportedException();
        }
    }
}
