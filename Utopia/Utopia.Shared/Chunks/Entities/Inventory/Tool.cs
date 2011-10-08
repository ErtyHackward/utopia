using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public abstract class Tool : VoxelItem
    {
        /// <summary>
        /// Gets or sets tool wear
        /// </summary>
        public byte Durability { get; set; }

        /// <summary>
        /// Performs tool business logic
        /// </summary>
        /// <param name="runOnServer">Indicates if tool is run on the server</param>
        /// <returns></returns>
        public abstract IToolImpact Use(bool runOnServer = false);

        /// <summary>
        /// Performs actions to rollback preliminary made actions on the client side
        /// </summary>
        /// <param name="impact"></param>
        public abstract void Rollback(IToolImpact impact);

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
