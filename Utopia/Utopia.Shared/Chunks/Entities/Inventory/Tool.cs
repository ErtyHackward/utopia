using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public abstract class Tool : VoxelItem
    {
        /// <summary>
        /// Tries to use the tool. The tool should decide is it possible to use and return toolImpact.
        /// </summary>
        /// <returns></returns>
        public IToolImpact Use()
        {
            return ToolLogic.Use(this);
        }

        /// <summary>
        /// Gets or sets tool wear
        /// </summary>
        public byte Durability { get; set; }

        /// <summary>
        /// Gets or sets tool business logic object (separate for client and server)
        /// This object is not stored with tool instance
        /// </summary>
        public IToolLogic ToolLogic { get; set; }

        /// <summary>
        /// The tool owner, impossible to use the tool without owner
        /// This object is not stored with tool instance
        /// </summary>
        public DynamicEntity Parent { get; set; }

        protected Tool()
        {
            UniqueName = GetType().Name;
        }


        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            Durability = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(Durability);
        }

    }
}
