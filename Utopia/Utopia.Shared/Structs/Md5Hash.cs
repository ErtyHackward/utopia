using System;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a 16 byte md5 hash
    /// </summary>
    public class Md5Hash : IEquatable<Md5Hash>
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// Gets hash bytes
        /// </summary>
        public byte[] Bytes
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Creates new instance of Md5Hash
        /// </summary>
        /// <param name="bytes"></param>
        public Md5Hash(byte[] bytes)
        {
            if(bytes == null || bytes.Length != 16)
                throw new ArgumentException("Md5 hash must be constructed from 16 bytes array");

            _bytes = bytes;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            return Equals((Md5Hash)obj);
        }

        public bool Equals(Md5Hash other)
        {
            if (other == null) return false;
            for (int i = 0; i < 16; i++)
            {
                if (_bytes[i] != other._bytes[i]) return false;
            }
            return true;
        }

        public static bool operator ==(Md5Hash one, Md5Hash two)
        {
            if (ReferenceEquals(one, two)) return true;
            if (ReferenceEquals(one, null)) return false;
            if (ReferenceEquals(two, null)) return false;
            
            return one.Equals(two);
        }

        public static bool operator !=(Md5Hash one, Md5Hash two)
        {
            return !(one == two);
        }

        public override int GetHashCode()
        {
            return _bytes.GetHashCode();
        }
    }
}
