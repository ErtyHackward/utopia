using System;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a voxel model for voxel entities
    /// </summary>
    public class VoxelModel : IBinaryStorable
    {
        public byte[, ,] Blocks { get; set; } //XXX this will be optimized later, maybe one dimensional array 

        public void RandomFill(int emptyProbabilityPercent)
        {
            var r = new Random();
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < Blocks.GetLength(2); z++)
                    {
                        if (r.Next(100) < emptyProbabilityPercent)
                            Blocks[x, y, z] = 0;
                        else
                            Blocks[x, y, z] = (byte)r.Next(1, 63);
                    }
                }
            }
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            if (Blocks != null)
            {
                writer.Write(Blocks.GetLength(0) * Blocks.GetLength(1) * Blocks.GetLength(2));
                writer.Write(Blocks.GetLength(0));
                writer.Write(Blocks.GetLength(1));
                writer.Write(Blocks.GetLength(2));
                for (int x = 0; x < Blocks.GetLength(0); x++)
                {
                    for (int y = 0; y < Blocks.GetLength(1); y++)
                    {
                        for (int z = 0; z < Blocks.GetLength(2); z++)
                        {
                            writer.Write(Blocks[x, y, z]);
                        }
                    }
                }
            }
            else writer.Write(0);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            var len = reader.ReadInt32();

            if (len > 0)
            {
                Blocks = new byte[reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()];
                for (int x = 0; x < Blocks.GetLength(0); x++)
                {
                    for (int y = 0; y < Blocks.GetLength(1); y++)
                    {
                        for (int z = 0; z < Blocks.GetLength(2); z++)
                        {
                            Blocks[x, y, z] = reader.ReadByte();
                        }
                    }
                }
            }
        }
    }
}
