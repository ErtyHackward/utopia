using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Invisible entity that holds tree data
    /// </summary>
    [ProtoContract]
    [EditorHide]
    public class TreeSoul : StaticEntity
    {
        /// <summary>
        /// Configuration blueprint index
        /// </summary>
        [ProtoMember(1)]
        public int TreeBlueprintIndex { get; set; }

        /// <summary>
        /// Random seed to create the tree
        /// </summary>
        [ProtoMember(2)]
        public int TreeRndSeed { get; set; }

        /// <summary>
        /// In case of huge damage of the trunk the tree will slowly disappear
        /// This time marks the beginning of the process
        /// </summary>
        [ProtoMember(3)]
        public UtopiaTime DisappearStarted { get; set; }
    }
}
