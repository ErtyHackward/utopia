using System.Collections.Generic;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    ///// <summary>
    ///// Represents a voxel container
    ///// </summary>
    //public abstract class ContainerEntity : VoxelEntity, IEntityContainer
    //{

    //    public ContainerEntity() {
    //        Capacity = 25;
    //    } 

    //    /// <summary>
    //    /// Indicates if chest cover is opened
    //    /// </summary>
    //    public bool IsOpened { get; set; }

    //    /// <summary>
    //    /// Gets maximum container capacity
    //    /// </summary>
    //    public int Capacity { get; private set; }

    //    /// <summary>
    //    /// Gets or sets a list of contained items
    //    /// </summary>
    //    public List<ContainedSlot> Items { get; set; }

    //    /// <summary>
    //    /// Gets a displayed entity name
    //    /// </summary>
    //    public override string DisplayName
    //    {
    //        get { return "Container"; }
    //    }

    //    public override void Load(System.IO.BinaryReader reader)
    //    {
    //        // first we need to load base information
    //        base.Load(reader);

    //        IsOpened = reader.ReadBoolean();

    //        // read contained entites count
    //        var count = reader.ReadInt32();

    //        // load contained slots (slot is count and entity example)
    //        Items.Clear();
    //        for (int i = 0; i < count; i++)
    //        {
    //            var containedSlot = new ContainedSlot();

    //            containedSlot.Load(reader);

    //            Items.Add(containedSlot);
    //        }

    //    }

    //    public override void Save(System.IO.BinaryWriter writer)
    //    {
    //        // first we need to save base information
    //        base.Save(writer);

    //        writer.Write(IsOpened);

    //        // we need to save items count to be able to load again
    //        writer.Write(Items.Count);

    //        // saving containing items
    //        foreach (var slot in Items)
    //        {
    //            slot.Save(writer);
    //        }
    //    }

    //}
}
