﻿using System;
using System.ComponentModel;
using System.IO;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using System.Linq;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube, wih a relationship with it (Removing the linked key will remove the entity)
    /// A Cube placeable Item will be by default a centered cube position, and can only be placed in a cube where no other CubePlaceableItem is present.
    /// </summary>
    public abstract class BlockLinkedItem : Item, ITool, IWorldIntercatingEntity, IBlockLinkedEntity, IBlockLocationRoot
    {
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

        /// <summary>
        /// The cube at wich the Entity is linked, if this cube is removed, the entity will also be removed
        /// </summary>
        [Browsable(false)]
        public Vector3I LinkedCube { get; set; }

        [Description("Allows to specify the possible face of the block where entity can be attached to")]
        public BlockFace MountPoint { get; set; }

        // tool logic
        public virtual IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == ToolUseMode.RightMouse)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    ILandscapeCursor cursor = LandscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);
                    
                    var moveVector = owner.EntityState.NewBlockPosition - owner.EntityState.PickedBlockPosition;

                    // check if entity can be put there
                    if (moveVector.Y == 1 && !MountPoint.HasFlag(BlockFace.Top))
                        return impact;

                    if (moveVector.Y == -1 && !MountPoint.HasFlag(BlockFace.Bottom))
                        return impact;

                    if ((Math.Abs(moveVector.X) == 1 || Math.Abs(moveVector.Z) == 1) && !MountPoint.HasFlag(BlockFace.Sides))
                        return impact;

                    // check if the place is free
                    if (MountPoint.HasFlag(BlockFace.Top) && moveVector.Y == 1 && cursor.PeekProfile(Vector3I.Up).IsSolidToEntity)
                        return impact;

                    if (MountPoint.HasFlag(BlockFace.Sides) && (moveVector.Y == 0) && cursor.PeekProfile(moveVector).IsSolidToEntity)
                        return impact;

                    if (MountPoint.HasFlag(BlockFace.Bottom) && moveVector.Y == -1 && cursor.PeekProfile(Vector3I.Down).IsSolidToEntity) 
                        return impact;

                    // create a new version of the item, and put it into the world
                    var cubeEntity = (BlockLinkedItem)entityFactory.CreateFromBluePrint(BluePrintId);
                    cubeEntity.LinkedCube = owner.EntityState.PickedBlockPosition;
                    cubeEntity.BlockLocationRoot = owner.EntityState.NewBlockPosition;

                    //Get the chunk where the entity will be added and check if another entity is not present at the destination root block !
                    var workingchunk = LandscapeManager.GetChunk(owner.EntityState.PickedBlockPosition); 
                    foreach (BlockLinkedItem entity in workingchunk.Entities.Entities.Values.Where(x => x is BlockLinkedItem && ((IBlockLinkedEntity)x).LinkedCube == owner.EntityState.PickedBlockPosition))
                    {
                        if (entity.BlockLocationRoot == owner.EntityState.NewBlockPosition && entity.LinkedCube == owner.EntityState.PickedBlockPosition)
                        {
                            //CubePlaced Entity already present at this location
                            return impact;
                        }
                    }

                    //cursor.GlobalPosition = owner.EntityState.PickedBlockPosition;

                    //If was not possible to set Item Place do nothing
                    if (!SetNewItemPlace(cubeEntity, owner, moveVector)) return impact;

                    cursor.AddEntity(cubeEntity, owner.DynamicId);
                    
                    impact.Success = true;
                }
            }
            return impact;
        }

        protected virtual bool SetNewItemPlace(BlockLinkedItem cubeEntity, IDynamicEntity owner, Vector3I vector)
        {
            // locate the entity
            if (vector.Y == 1) // = Put on TOP 
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
            }
            else if (vector.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                   owner.EntityState.PickedBlockPosition.Y,
                                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
            }
            else //Put on a side
            {
                var newBlockPos = owner.EntityState.NewBlockPosition;

                cubeEntity.Position = new Vector3D(newBlockPos + new Vector3(0.5f - (float)vector.X / 2, 0.5f, 0.5f - (float)vector.Z / 2));
                cubeEntity.Position += new Vector3D(vector.X == -1 ? -0.01 : 0, 0, vector.Z == -1 ? -0.01 : 0);

                var slope = 0d;

                if (vector.X == -1) slope = -Math.PI / 2;
                if (vector.X == 1) slope = Math.PI / 2; // ok
                if (vector.Z == -1) slope = Math.PI; // ok
                if (vector.Z == 1) slope = 0;

                cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)slope);
            }

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
            LinkedCube = reader.ReadVector3I();
            MountPoint = (BlockFace)reader.ReadByte();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(BlockLocationRoot);
            writer.Write(LinkedCube);
            writer.Write((byte)MountPoint);
        }
    }
}