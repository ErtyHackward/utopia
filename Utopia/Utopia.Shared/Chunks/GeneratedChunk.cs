namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a world generator chunk. Should be used to work with world generation processors
    /// </summary>
    public class GeneratedChunk : AbstractChunk
    {
        /// <summary>
        /// Gets or sets medium chunk terrain elevation level
        /// </summary>
        public int GroundHeight { get; set; }

        public GeneratedChunk() : base(new InsideDataProvider())
        {
            
        }
    }
}
