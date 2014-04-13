using ProtoBuf;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.Tools.BinarySerializer;
using System.Collections.Generic;

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

            loc.x = reader.ReadInt32();
            loc.y = reader.ReadInt32();
            loc.z = reader.ReadInt32();

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

        public static void Write(this BinaryWriter writer, Matrix matrix)
        {
            writer.Write(matrix.M11);
            writer.Write(matrix.M12);
            writer.Write(matrix.M13);
            writer.Write(matrix.M14);
            writer.Write(matrix.M21);
            writer.Write(matrix.M22);
            writer.Write(matrix.M23);
            writer.Write(matrix.M24);
            writer.Write(matrix.M31);
            writer.Write(matrix.M32);
            writer.Write(matrix.M33);
            writer.Write(matrix.M34);
            writer.Write(matrix.M41);
            writer.Write(matrix.M42);
            writer.Write(matrix.M43);
            writer.Write(matrix.M44);
        }

        public static Matrix ReadMatrix(this BinaryReader reader)
        {
            Matrix matrix;

            matrix.M11 = reader.ReadSingle();
            matrix.M12 = reader.ReadSingle();
            matrix.M13 = reader.ReadSingle();
            matrix.M14 = reader.ReadSingle();
            matrix.M21 = reader.ReadSingle();
            matrix.M22 = reader.ReadSingle();
            matrix.M23 = reader.ReadSingle();
            matrix.M24 = reader.ReadSingle();
            matrix.M31 = reader.ReadSingle();
            matrix.M32 = reader.ReadSingle();
            matrix.M33 = reader.ReadSingle();
            matrix.M34 = reader.ReadSingle();
            matrix.M41 = reader.ReadSingle();
            matrix.M42 = reader.ReadSingle();
            matrix.M43 = reader.ReadSingle();
            matrix.M44 = reader.ReadSingle();

            return matrix;
        }

        public static Md5Hash ReadMd5Hash(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(16);

            if (bytes.Length != 16)
                throw new EndOfStreamException();

            return new Md5Hash(bytes);
        }

        public static void Write(this BinaryWriter writer, Md5Hash hash)
        {
            if (hash == null) throw new ArgumentNullException("hash");
            writer.Write(hash.Bytes);
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

        public static Range2I ReadRange2(this BinaryReader reader)
        {
            return new Range2I(reader.ReadVector2I(), reader.ReadVector2I());
        }

        public static void Write(this BinaryWriter writer, Range2I range)
        {
            writer.Write(range.Position);
            writer.Write(range.Size);
        }
        
        public static byte[] Serialize(this IBinaryStorable item)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                item.Save(writer);
                return ms.ToArray();
            }
        }

        public static byte[] ProtoSerialize(this object item)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                return Serializer.Deserialize<T>(ms);   
            }
        }

        public static void SerializeArray<T>(this BinaryWriter writer, T[] arrayValues) where T : IBinaryStorable
        {
            int arraySize = arrayValues.Length;
            writer.Write(arraySize); //Write down the Array size

            for (int i = 0; i < arraySize; i++)
            {
                arrayValues[i].Save(writer);
            }
        }

        public static void SerializeArray<T>(this BinaryWriter writer, IList<T> arrayValues) where T : IBinaryStorable
        {
            int arraySize = arrayValues.Count;
            writer.Write(arraySize); //Write down the Array size

            for (int i = 0; i < arraySize; i++)
            {
                arrayValues[i].Save(writer);
            }
        }

        public static void DeserializeArray<T>(this BinaryReader reader, out T[] returnedArray) where T : IBinaryStorable, new()
        {
            int arraySize = reader.ReadInt32();
            returnedArray = new T[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                returnedArray[i] = new T();
                returnedArray[i].Load(reader);
            }
        }

        public static void DeserializeArray<T>(this BinaryReader reader, out List<T> returnedArray) where T : IBinaryStorable, new()
        {
            int arraySize = reader.ReadInt32();
            returnedArray = new List<T>(arraySize);
            for (int i = 0; i < arraySize; i++)
            {
                returnedArray.Add(new T());
                returnedArray[i].Load(reader);
            }
        }
    }
}
