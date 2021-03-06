﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using ProtoBuf;
using Utopia.Shared.Tools;

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
        public List<InitSlot> Ingredients { get; set; }

        /// <summary>
        /// Gets or sets recipe result item
        /// </summary>
        [Editor(typeof(BlueprintTypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        [ProtoMember(2)]
        public ushort ResultBlueprintId { get; set; }

        /// <summary>
        /// Gets or sets recipe result items count
        /// </summary>
        [ProtoMember(3)]
        public int ResultCount { get; set; }

        /// <summary>
        /// Gets recipe display name
        /// </summary>
        [ProtoMember(4)]
        public string Name { get; set; }

        /// <summary>
        /// Conatiner where this recipe is belong to
        /// </summary>
        [Browsable(false)]
        [ProtoMember(5)]
        public ushort ContainerBlueprintId { get; set; }

        [Description("How many seconds needs to create the item, 0 - instantly")]
        [ProtoMember(6)]
        public int Time { get; set; }

        public Recipe()
        {
            Ingredients = new List<InitSlot>();
            ResultCount = 1;
            Name = "Noname";
        }

        public override string ToString()
        {
            return Name + (ResultCount == 1 ? "" : " x" + ResultCount);
        }
    }

    /// <summary>
    /// Contains blueprint id with items count
    /// </summary>
    [ProtoContract]
    public struct InitSlot
    {
        /// <summary>
        /// Item blueprint id
        /// </summary>
        [Editor(typeof(BlueprintTypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        [ProtoMember(1)]
        public ushort BlueprintId { get; set; }

        /// <summary>
        /// Number of items required
        /// </summary>
        [ProtoMember(2)]
        public int Count { get; set; }

        /// <summary>
        /// Optional initialization set for containers. In case if the blueprint is the container allows to fill it with the set provided
        /// </summary>
        [TypeConverter(typeof(ContainerSetSelector))]
        [Description("Optional initialization set for containers. In case if the blueprint is the container allows to fill it with the set provided")]
        [ProtoMember(3)]
        public string SetName { get; set; }
        
        public override string ToString()
        {
            return "Slot";
        }
    }
}
