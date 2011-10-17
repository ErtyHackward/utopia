using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace System.IO
{
    public static class BinaryExtensions
    {
        public static void Write(this BinaryWriter writer, Vector3I loc)
        {
            writer.Write(loc.X);
            writer.Write(loc.Y);
            writer.Write(loc.Z);
        }

        public static Vector3I ReadVector3I(this BinaryReader reader)
        {
            Vector3I loc;

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

        public static Vector3D ReadVector3D(this BinaryReader reader)
        {
            Vector3D loc;

            loc.X = reader.ReadDouble();
            loc.Y = reader.ReadDouble();
            loc.Z = reader.ReadDouble();

            return loc;
        }

        public static void Write(this BinaryWriter writer, Vector3D loc)
        {
            writer.Write(loc.X);
            writer.Write(loc.Y);
            writer.Write(loc.Z);
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

        public static Vector2I ReadVector2I(this BinaryReader reader)
        {
            Vector2I vec;

            vec.X = reader.ReadInt32();
            vec.Y = reader.ReadInt32();

            return vec;
        }

        public static void Write(this BinaryWriter writer, Vector2I vec)
        {
            writer.Write(vec.X);
            writer.Write(vec.Y);
        }

        public static Range2 ReadRange2(this BinaryReader reader)
        {
            return new Range2(reader.ReadVector2I(), reader.ReadVector2I());
        }

        public static void Write(this BinaryWriter writer, Range2 range)
        {
            writer.Write(range.Position);
            writer.Write(range.Size);
        }

        public static byte[] ToArray(this IBinaryStorable item)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                item.Save(writer);
                return ms.ToArray();
            }
        }
    }
}
