using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public class TreeGenerator
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public IEnumerable<BlockWithPosition> GenerateMesh(Vector3I WorldBlockLocation, 
                                                           byte trunkBlockId, 
                                                           byte foliageBlockId, 
                                                           FastRandom fastRnd)
        {
            List<BlockWithPosition> mesh = new List<BlockWithPosition>();

            //Generate the Mesh.
            var height = fastRnd.Next(2, 4);
            int size = 1;

            height += size;

            //Create trunk Kern
            int kernSize = size - 1;
            for (int trunkY = WorldBlockLocation.Y; trunkY <= height + WorldBlockLocation.Y; trunkY++)
            {
                for (int trunkX = WorldBlockLocation.X - kernSize; trunkX <= WorldBlockLocation.X + kernSize; trunkX++)
                {
                    for (int trunkZ = WorldBlockLocation.Z - kernSize; trunkZ <= WorldBlockLocation.Z + kernSize; trunkZ++)
                    {
                        mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(trunkX, trunkY, trunkZ) });
                    }
                }

                //Create trunk Border
                if (size > 0)
                {
                    //Left
                    mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(WorldBlockLocation.X - size, trunkY, WorldBlockLocation.Z) });
                    //Right
                    mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(WorldBlockLocation.X + size, trunkY, WorldBlockLocation.Z) });
                    //Front
                    mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(WorldBlockLocation.X, trunkY, WorldBlockLocation.Z + size) });
                    //Back
                    mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(WorldBlockLocation.X, trunkY, WorldBlockLocation.Z - size) });
                }else{
                    mesh.Add(new BlockWithPosition() { BlockId = trunkBlockId, WorldPosition = new Vector3I(WorldBlockLocation.X, trunkY, WorldBlockLocation.Z) });
                }
            }


            //Create foliage
            for (int foliageGroup = 0; foliageGroup <= size + 1; foliageGroup++)
            {
                //Get Position
                Vector3I foliageRoot = new Vector3I()
                {
                    Y = WorldBlockLocation.Y + height + fastRnd.Next((int)height / 2, height),
                    X = WorldBlockLocation.X + fastRnd.Next(size * 2),
                    Z = WorldBlockLocation.X + fastRnd.Next(size * 2)
                };

                mesh.Add(new BlockWithPosition() { BlockId = foliageBlockId, WorldPosition = foliageRoot });

                //Create shape centred around the Root
                //TODO
            }

            return mesh;
            
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
