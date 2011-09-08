using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SharpDX;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents entites which has a voxel nature
    /// </summary>
    public abstract class VoxelEntity : Entity
    {
        public byte[, ,] Blocks;//XXX this will be optimized later, maybe one dimensional array 
      
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

        public void PlainCubeFill()
        {
            Random r = new Random();

            int xmax = Blocks.GetLength(0) - 1;
            int ymax = Blocks.GetLength(1) - 1;
            int zmax = Blocks.GetLength(2) - 1;
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < Blocks.GetLength(2); z++)
                    {
                        //if (x == 0 || y == 0 || z == 0 || x == xmax || y == ymax || z == zmax)
                         //   Blocks[x, y, z] = (byte)r.Next(1,63);
                        
                        if (x == 0 )
                           Blocks[x, y, z] = 1; 
                        else if (y == 0)
                            Blocks[x, y, z] = 4;
                        else if (z == 0) 
                            Blocks[x, y, z] = 8;
                        else if (x == xmax) 
                            Blocks[x, y, z] = 12;
                        else if (y == ymax)
                            Blocks[x, y, z] = 16;
                        else if (z == zmax)
                           Blocks[x, y, z] = 20;
                        else 
                            Blocks[x, y, z] = 63;
                    }
                }
            }
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
           
            byte[] temp = reader.ReadBytes(Blocks.Length);
            Blocks = (byte[,,]) formatter.Deserialize(ms);//really not sure about this
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, Blocks);
            
            writer.Write(ms.ToArray()); //intendend for 16*16*16 = 4096 bytes , no need to buffer !
        }
    }
}
