using SharpDX;
using Utopia.Shared.Structs;

namespace System.IO
{
    public static class BinaryExtensions
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

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            Quaternion q;

            q.X = reader.ReadSingle();
            q.Y = reader.ReadSingle();
            q.Z = reader.ReadSingle();
            q.W = reader.ReadSingle();

            return q;
        }

        public static void Write(this BinaryWriter writer, Quaternion quaternion)
        {
            writer.Write(quaternion.X);
            writer.Write(quaternion.Y);
            writer.Write(quaternion.Z);
            writer.Write(quaternion.W);
        }

        public static IntVector2 ReadIntVector2(this BinaryReader reader)
        {
            IntVector2 vec;

            vec.X = reader.ReadInt32();
            vec.Y = reader.ReadInt32();

            return vec;
        }

        public static void Write(this BinaryWriter writer, IntVector2 vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
        }

        public static Range2 ReadRange2(this BinaryReader reader)
        {
            var r = new Range2();

            r.Position = reader.ReadIntVector2();
            r.Size = reader.ReadIntVector2();

            return r;
        }

        public static void Write(this BinaryWriter writer, Range2 range)
        {
            writer.Write(range.Position);
            writer.Write(range.Size);
        }
    }
}
