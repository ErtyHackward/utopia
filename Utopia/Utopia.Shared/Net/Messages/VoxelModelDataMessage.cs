using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Net.Messages
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VoxelModelDataMessage : IBinaryMessage
    {
        private byte[] _compressed;
        private VoxelModel _voxelModel;

        public byte MessageId
        {
            get { return (byte)MessageTypes.VoxelModelData; }
        }
        
        public VoxelModel VoxelModel
        {
            get { return _voxelModel; }
            set { _voxelModel = value; }
        }

        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }

        private void Compress()
        {
            if (_compressed != null)
                return;

            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress))
            {
                var writer = new BinaryWriter(zip);
                VoxelModel.Save(writer);
            }

            _compressed = ms.ToArray();
        }

        public static void Write(BinaryWriter writer, VoxelModelDataMessage msg)
        {
            msg.Compress();
            writer.Write(msg._compressed.Length);
            writer.Write(msg._compressed);
        }

        public static VoxelModelDataMessage Read(BinaryReader reader)
        {
            VoxelModelDataMessage msg;

            var len = reader.ReadInt32();

            msg._compressed = reader.ReadBytes(len);

            if (msg._compressed.Length != len)
                throw new EndOfStreamException();

            msg._voxelModel = new VoxelModel();

            using (var ms = new MemoryStream(msg._compressed))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    var decompressed = new MemoryStream();
                    zip.CopyTo(decompressed);
                    decompressed.Position = 0;
                    var reader2 = new BinaryReader(decompressed);
                    msg._voxelModel.Load(reader2);
                    decompressed.Dispose();
                }
            }
            

            return msg;
        }
    }
}
