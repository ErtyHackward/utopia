
namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IVoxelEntity : IEntity
    {
        byte[, ,] Blocks { get; set;}
        void RandomFill(int emptyProbabilityPercent);   
    }
}
