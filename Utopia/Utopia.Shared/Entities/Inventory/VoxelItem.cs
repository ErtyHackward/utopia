using System;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    public abstract class VoxelItem : StaticEntity, IItem, IVoxelEntity
    {
        private readonly VoxelModel _model = new VoxelModel();

        #region Properties

        /// <summary>
        /// Gets voxel entity model
        /// </summary>
        public VoxelModel Model
        {
            get { return _model; }
        }

        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel shovel)
        /// </summary>
        public virtual string UniqueName { get; set; }

        public virtual string StackType
        {
            get { return GetType().Name; }            
        }

        public virtual EquipmentSlotType AllowedSlots
        {
            get { return EquipmentSlotType.None; }
        }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        public abstract int MaxStackSize { get; }


        public abstract string Description { get; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }

        #endregion

        public event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        public void OnVoxelModelChanged(VoxelModelEventArgs e)
        {
            var handler = VoxelModelChanged;
            if (handler != null) handler(this, e);
        }

        public void CommitModel()
        {
            OnVoxelModelChanged(new VoxelModelEventArgs { Model = _model });
        }

        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);

            Model.Load(reader);

            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            Model.Save(writer);

            writer.Write(UniqueName);
        }      
    }
}
