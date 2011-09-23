using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Structs
{
    public struct TerraCubeWithPosition
    {
        public TerraCube Cube;
        public Location3<int> Position;//XXX should be readonly and removing lots of ref in methods 
        
        public TerraCubeWithPosition(Location3<int> pos, TerraCube cube)
        {
            Position = pos;
            Cube = cube;
        }
    }
}
