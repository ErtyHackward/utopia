using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityHealthChangeEventArgs : EventArgs
    {
        /// <summary>
        /// The entity concerned by the Health change
        /// </summary>
        public ICharacterEntity ImpactedEntity { get; set; }

        /// <summary>
        /// The entity that did the action that made the health change, can be null.
        /// </summary>
        public IDynamicEntity SourceEntity { get; set; }

        /// <summary>
        /// Health change done
        /// </summary>
        public float Change { get; set; }

        /// <summary>
        /// Health energy concerned
        /// </summary>
        public Energy Health { get; set; }

        /// <summary>
        /// The impact on the target where the healthchange is applied
        /// </summary>
        public Vector3 HealthChangeHitLocation { get; set; }

        /// <summary>
        /// The normal at the HealthChangeHitLocation
        /// </summary>
        public Vector3I HealthChangeHitLocationNormal { get; set; }
    }
}
