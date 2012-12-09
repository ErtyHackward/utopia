using S33M3CoreComponents.Sound;

namespace Utopia.Shared.Entities.Interfaces
{
    internal interface ISoundEmitterEntity
    {
        ISoundEngine SoundEngine { get; set; }
    }
}
