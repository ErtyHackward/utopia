using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface INpc
    {
        /// <summary>
        /// Allows to order the entity to move somewhere
        /// </summary>
        IMoveAI Movement { get; }

        /// <summary>
        /// Gets wrapped character
        /// </summary>
        CharacterEntity Character { get; }

        /// <summary>
        /// Gets or sets npc faction
        /// </summary>
        Faction Faction { get; set; }
    }
}