using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    public abstract class SpriteItem : SpriteEntity, IItem
    {
        #region Private variables
        #endregion

        #region Public variables
        public EquipmentSlotType AllowedSlots { get; set; }
        //public SpriteTexture Icon { get; set; }
        //public Rectangle? IconSourceRectangle  { get; set; }
        public abstract int MaxStackSize { get; }
        public string UniqueName { get; set; }
        public abstract string Description { get; }

        public virtual string StackType
        {
            get { return this.GetType().Name; }
        }

        #endregion

        protected SpriteItem()
        {
            UniqueName = string.Empty;
        }

        #region Public Methods
        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(UniqueName);
        }
        #endregion

        #region Private methods
        #endregion
        
        public Chunks.AbstractChunk ParentChunk { get; set; }
    }
}
