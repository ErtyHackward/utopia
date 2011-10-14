using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.Structs
{
    public struct TerraCubeWithPosition
    {
        public TerraCube Cube;
        public Vector3I Position;//XXX should be readonly and removing lots of ref in methods 
        
        public TerraCubeWithPosition(Vector3I pos, TerraCube cube)
        {
            Position = pos;
            Cube = cube;
        }
    }
}
