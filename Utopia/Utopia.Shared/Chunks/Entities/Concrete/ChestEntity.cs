﻿using System.Collections.Generic;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents a voxel chest
    /// </summary>
    public class ChestEntity : VoxelEntity, IContainerEntity
    {
        /// <summary>
        /// Indicates if chest cover is opened
        /// </summary>
        public bool IsOpened { get; set; }

        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        public int Capacity { get { return 50; } }

        // todo: change items to provide entity and entities count
        /// <summary>
        /// Gets or sets a list of contained items
        /// </summary>
        public List<Entity> Items { get; set; }

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            IsOpened = reader.ReadBoolean();

            var count = reader.ReadInt32();

            Items.Clear();
            for (int i = 0; i < count; i++)
            {
                var entity = EntityFactory.Instance.CreateFromBytes(reader);
                Items.Add(entity);
            }

        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(IsOpened);

            // we need to save items count to be able to load again
            writer.Write(Items.Count);

            foreach (var entity in Items)
            {
                entity.Save(writer);
            }
        }
    }
}
