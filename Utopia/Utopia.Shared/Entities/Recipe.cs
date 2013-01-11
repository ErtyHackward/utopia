using System.Collections.Generic;
using ProtoBuf;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Contains information about entities to be transfromed to other entities
    /// </summary>
    [ProtoContract]
    public class Recipe
    {
        /// <summary>
        /// Gets or sets list of recipe ingredients
        /// </summary>
        [ProtoMember(1)]
        public List<RecipeIngredient> Ingredients { get; set; }

        /// <summary>
        /// Gets or sets recipe result item and count
        /// </summary>
        [ProtoMember(2)]
        public RecipeIngredient Result { get; set; }

        /// <summary>
        /// Gets recipe display name
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Describes items required 
    /// </summary>
    public struct RecipeIngredient
    {
        /// <summary>
        /// Item blueprint id
        /// </summary>
        public ushort BlueprintId { get; set; }

        /// <summary>
        /// Number of items required
        /// </summary>
        public int Count { get; set; }
    }
}
