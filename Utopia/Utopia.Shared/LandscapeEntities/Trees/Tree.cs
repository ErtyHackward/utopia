using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities.Trees
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
            int size = 0;

            height += size;

            //Create foliage
            for (int foliageGroup = 0; foliageGroup <= size; foliageGroup++)
            {

                //Create Elipsoid shape centred around the Root
                var ellipsoidSize = new Vector3I(7, 4, 7);
                var range = new Range3I { Size = ellipsoidSize };
                var center = (Vector3)ellipsoidSize / 2;
                var radius = ellipsoidSize / 2;

                //Get Position
                Vector3I foliageRoot = new Vector3I()
                {
                    Y = WorldBlockLocation.Y + height + fastRnd.Next((int)ellipsoidSize.Y / 3, (int)ellipsoidSize.Y / 2),
                    X = WorldBlockLocation.X + fastRnd.Next(size * 2),
                    Z = WorldBlockLocation.Z + fastRnd.Next(size * 2)
                };


                foreach (var position in range)
                {
                    var point = position + new Vector3(0.5f, 0.5f, 0.5f);

                    if (IsInsideEllipsoid(center, radius, point))
                        mesh.Add(new BlockWithPosition() { BlockId = foliageBlockId, WorldPosition = position + foliageRoot - (ellipsoidSize / 2) });
                }
            }

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



            return mesh;
        }
        #endregion

        #region Private Methods
        private bool IsInsideEllipsoid(Vector3 center, Vector3 radius, Vector3 point)
        {
            var dx = point.X - center.X;
            var dy = point.Y - center.Y;
            var dz = point.Z - center.Z;

            return dx * dx / ( radius.X * radius.X ) + 
                   dy * dy / ( radius.Y * radius.Y ) +
                   dz * dz / ( radius.Z * radius.Z ) <= 1;
        }
        #endregion
    }
}
