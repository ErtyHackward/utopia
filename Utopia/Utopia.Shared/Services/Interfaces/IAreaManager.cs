using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Services.Interfaces
{
    public interface IAreaManager
    {
        /// <summary>
        /// Creates and adds ServerNpc wrapper to the world
        /// </summary>
        /// <param name="characterEntity"></param>
        /// <returns></returns>
        INpc CreateNpc(CharacterEntity characterEntity);

        void RemoveNpc(INpc npc);
    }
}