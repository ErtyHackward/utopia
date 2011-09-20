using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents entites which has a voxel nature
    /// </summary>
    public abstract class VoxelEntity : Entity, IVoxelEntity
    {
        public byte[, ,] Blocks { get; set; }//XXX this will be optimized later, maybe one dimensional array 

        // we need to override save and load!
        
        public void RandomFill(int emptyProbabilityPercent)
        {
            Random r = new Random();
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



        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

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

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

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
    }
}
