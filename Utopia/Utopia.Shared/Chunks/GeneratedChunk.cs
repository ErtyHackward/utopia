namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a world generator chunk. Should be used to work with world generation processors
    /// </summary>
    public class GeneratedChunk : AbstractChunk
    {
        public GeneratedChunk() : base(new InsideDataProvider())
        {
            
        }
    }
}
