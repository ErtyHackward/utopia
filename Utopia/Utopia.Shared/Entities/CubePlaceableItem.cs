using System;
using System.ComponentModel;
using System.IO;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube
    /// </summary>
    public abstract class CubePlaceableItem : Item, ITool, IWorldIntercatingEntity, IBlockLinkedEntity
    {
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Browsable(false)]
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        [Browsable(false)]
        public EntityFactory entityFactory { get; set; }

        [Browsable(false)]
        public Vector3I LinkedCube { get; set; }
        
        [Description("Allows to specify the possible face of the block where entity can be attached to")]
        public BlockFace MountPoint { get; set; }
        
        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == ToolUseMode.RightMouse)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    var cursor = LandscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);

                    var moveVector = owner.EntityState.NewBlockPosition - owner.EntityState.PickedBlockPosition;

                    // check if entity can be put there
                    if (moveVector.Y == 1 && !MountPoint.HasFlag(BlockFace.Top))
                        return impact;

                    if (moveVector.Y == -1 && !MountPoint.HasFlag(BlockFace.Bottom))
                        return impact;

                    if ((Math.Abs(moveVector.X) == 1 || Math.Abs(moveVector.Z) == 1) && !MountPoint.HasFlag(BlockFace.Sides))
                        return impact;

                    // check if the place is free
                    if (MountPoint.HasFlag(BlockFace.Top) && cursor.Up().IsSolid())
                        return impact;
                    
                    if (MountPoint.HasFlag(BlockFace.Sides) && cursor.Offset(moveVector).IsSolid())
                        return impact;

                    if (MountPoint.HasFlag(BlockFace.Bottom) && cursor.Down().IsSolid())
                        return impact;

                    cursor.GlobalPosition = owner.EntityState.PickedBlockPosition;

                    //Create a new version of the item, and put it into the world
                    var cubeEntity = (IItem)entityFactory.CreateFromBluePrint(BluePrintId);

                    var blockLinked = (IBlockLinkedEntity)cubeEntity;
                    blockLinked.LinkedCube = owner.EntityState.PickedBlockPosition;

                    // locate the entity
                    if (moveVector.Y == 1)
                    {
                        cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                           owner.EntityState.PickedBlockPosition.Y + 1f,
                                                           owner.EntityState.PickedBlockPosition.Z + 0.5f);
                    }
                    else if (moveVector.Y == -1)
                    {
                        cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                           owner.EntityState.PickedBlockPosition.Y,
                                                           owner.EntityState.PickedBlockPosition.Z + 0.5f);
                    }
                    else
                    {
                        var newBlockPos = owner.EntityState.NewBlockPosition;

                        cubeEntity.Position = new Vector3D(newBlockPos + new Vector3(0.5f - (float)moveVector.X / 2, 0.5f, 0.5f - (float)moveVector.Z / 2));
                        cubeEntity.Position += new Vector3D(moveVector.X == -1 ? -0.01 : 0, 0, moveVector.Z == -1 ? -0.01 : 0);
                        
                        var slope = 0d;

                        if (moveVector.X == -1) slope = -Math.PI / 2;
                        if (moveVector.X == 1) slope = Math.PI / 2; // ok
                        if (moveVector.Z == -1) slope = Math.PI; // ok
                        if (moveVector.Z == 1) slope = 0 ;

                        cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)slope);
                    }

                    cursor.AddEntity(cubeEntity);
                    
                    impact.Success = true;
                }
            }
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            LinkedCube = reader.ReadVector3I();
            MountPoint = (BlockFace)reader.ReadByte();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(LinkedCube);
            writer.Write((byte)MountPoint);
        }
    }
}
