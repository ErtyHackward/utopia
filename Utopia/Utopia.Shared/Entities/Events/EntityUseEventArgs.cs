using System;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityUseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets sender entity
        /// </summary>
        public IDynamicEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the tool was used (maybe null)
        /// </summary>
        public IItem Tool { get; set; }

        public int RecipeIndex { get; set; }

        /// <summary>
        /// Gets the use type (put, or use)
        /// </summary>
        public UseType UseType { get; set; }

        /// <summary>
        /// Gets or sets supplied dynamic entity information
        /// </summary>
        public DynamicEntityState State { get; set; }

        /// <summary>
        /// Resulted impact
        /// </summary>
        public IToolImpact Impact { get; set; }

        /// <summary>
        /// Creates event args from entity state
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static EntityUseEventArgs FromState(IDynamicEntity owner)
        {
            var state = owner.EntityState.Clone();

            var e = new EntityUseEventArgs
                        {
                            State = state,
                            Entity = owner
                        };

            return e;
        }
    }
}