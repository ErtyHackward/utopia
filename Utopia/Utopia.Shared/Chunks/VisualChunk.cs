using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk
    {
        //todo: 3d specific fields and properties
        //todo: move this class to Utopia project
        private TerraCube[] _cubes;

        public VisualChunk()
        {
            _cubes = new TerraCube[ChunkBlocksByteLength]; // creating additional data for each block, actually we can remove Id from TerraCube structure to release client memory
        }

    }
}
