using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Tools.BinarySerializer
{
    public static class BinarySerialize
    {
        public static void SerializeArray<T>(T[] arrayValues, BinaryWriter writer) where T : IBinaryStorable
        {
            int arraySize = arrayValues.Length;
            writer.Write(arraySize); //Write down the Array size

            for (int i = 0; i < arraySize; i++)
            {
                arrayValues[i].Save(writer);
            }
        }

        public static void SerializeArray<T>(IList<T> arrayValues, BinaryWriter writer) where T : IBinaryStorable
        {
            int arraySize = arrayValues.Count;
            writer.Write(arraySize); //Write down the Array size

            for (int i = 0; i < arraySize; i++)
            {
                arrayValues[i].Save(writer);
            }
        }

        public static void DeserializeArray<T>(BinaryReader reader, out T[] returnedArray) where T : IBinaryStorable, new()
        {
            int arraySize = reader.ReadInt32();
            returnedArray = new T[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                returnedArray[i] = new T();
                returnedArray[i].Load(reader);
            }
        }

        public static void DeserializeArray<T>(BinaryReader reader, out List<T> returnedArray) where T : IBinaryStorable, new()
        {
            int arraySize = reader.ReadInt32();
            returnedArray = new List<T>(arraySize);
            for (int i = 0; i < arraySize; i++)
            {
                returnedArray[i] = new T();
                returnedArray[i].Load(reader);
            }
        }
    }
}
