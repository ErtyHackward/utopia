using SharpDX;
using Utopia.Shared.Structs;

namespace System.IO
{
    public static class BinaryHelper
    {
        public static void Write(this BinaryWriter writer, Location3<int> loc)
        {
            writer.Write(loc.X);
            writer.Write(loc.Y);
            writer.Write(loc.Z);
        }

        public static Location3<int> ReadIntLocation3(this BinaryReader reader)
        {
            Location3<int> loc;

            loc.X = reader.ReadInt32();
            loc.Y = reader.ReadInt32();
            loc.Z = reader.ReadInt32();

            return loc;
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            Vector3 loc;

            loc.X = reader.ReadSingle();
            loc.Y = reader.ReadSingle();
            loc.Z = reader.ReadSingle();

            return loc;
        }

        public static void Write(this BinaryWriter writer, Vector3 loc)
        {
            writer.Write(loc.X);
            writer.Write(loc.Y);
            writer.Write(loc.Z);
        }
    }
}
