using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents entites which has a voxel nature
    /// </summary>
    public abstract class VoxelEntity : Entity
    {
        public byte[, ,] Blocks;//XXX this will be optimized later, maybe one dimensional array 
        public bool Altered = true;

        //public Texture2D icon;
        //Icon can be a 2d projection of the voxel array

        // we need to override save and load!

        public void RandomFill(int emptyProbabilityPercent)
        {
            Random r = new Random();
            for (uint x = 0; x < Blocks.GetLength(0); x++)
            {
                for (uint y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (uint z = 0; z < Blocks.GetLength(2); z++)
                    {
                        if (r.Next(100) < emptyProbabilityPercent)
                            Blocks[x, y, z] = 0;
                        else
                            Blocks[x, y, z] = (byte)r.Next(63);
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
