using S33M3Resources.Structs;
using System;
using System.ComponentModel;
using System.IO;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for Item that will be placed in a centered way on a block,
    /// This entity cannot be placed on a block where another entity is placed.
    /// </summary>
    public abstract class BlockItem : Item, ITool, IWorldIntercatingEntity, IBlockLocationRoot
    {
        #region Public Properties
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Browsable(false)]
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected automagically by entityfactory
        /// </summary>
        [Browsable(false)]
        public EntityFactory entityFactory { get; set; }

        /// <summary>
        /// The cube where the entity root belongs to.
        /// </summary>
        [Browsable(false)]
        public Vector3I BlockLocationRoot { get; set; }
        #endregion

        #region Public Methods
        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer = false)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == ToolUseMode.RightMouse)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    var cursor = LandscapeManager.GetCursor(owner.EntityState.NewBlockPosition);
                    
                    // check if the place is free for te entity "Root"
                    if (cursor.PeekProfile().IsSolidToEntity) return impact;

                    // create a new version of the item, and put it into the world
                    var cubeEntity = (BlockItem)entityFactory.CreateFromBluePrint(BluePrintId);
                    cubeEntity.BlockLocationRoot = owner.EntityState.NewBlockPosition;
                    // Get the chunk where the entity will be added and check if another entity is present inside this block
                    var workingchunk = LandscapeManager.GetChunk(owner.EntityState.NewBlockPosition);
                    foreach (IBlockLocationRoot entity in workingchunk.Entities.Entities.Values)
                    {
                        if (entity.BlockLocationRoot == cubeEntity.BlockLocationRoot)
                        {
                            // IBlockLocationRoot Entity already present at this location
                            return impact;
                        }
                    }

                    // Do the Chunk on chunk Next to this one ==> TO DO

                    // If was not possible to set Item Place do nothing
                    if (!SetNewItemPlace(cubeEntity, owner)) return impact;

                    cursor.AddEntity(cubeEntity, owner.DynamicId);

                    impact.Success = true;
                }
            }
            return impact;
        }

        protected virtual bool SetNewItemPlace(BlockItem cubeEntity, IDynamicEntity owner)
        {
            // Center the Entity on the newlockPosition
            cubeEntity.Position = new Vector3D(owner.EntityState.NewBlockPosition.X + 0.5f,
                                               owner.EntityState.NewBlockPosition.Y,
                                               owner.EntityState.NewBlockPosition.Z + 0.5f);
            return true;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }


        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            BlockLocationRoot = reader.ReadVector3I();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(BlockLocationRoot);
        }
        #endregion
    }
}
