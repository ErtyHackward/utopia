using System;
using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using System.Linq;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube, wih a relationship with it (Removing the linked key will remove the entity)
    /// A Cube placeable Item will be by default a centered cube position, and can only be placed in a cube where no other CubePlaceableItem is present.
    /// </summary>
    [ProtoContract]
    public abstract class BlockLinkedItem : Item, IBlockLinkedEntity, IBlockLocationRoot
    {
        /// <summary>
        /// The cube where the entity root belongs to.
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public Vector3I BlockLocationRoot { get; set; }

        /// <summary>
        /// The cube at wich the Entity is linked, if this cube is removed, the entity will also be removed
        /// </summary>
        [Browsable(false)]
        [ProtoMember(2)]
        public Vector3I LinkedCube { get; set; }

        [Description("Allows to specify the possible face of the block where entity can be attached to")]
        [ProtoMember(3)]
        public BlockFace MountPoint { get; set; }

        [Description("The entity will be centered into the choosen block face")]
        [ProtoMember(4)]
        public bool BlockFaceCentered { get; set; }
        
        [Description("The entity cannot be added if the block where it will be placed contain another entity")]
        [ProtoMember(5)]
        public bool BlockEmptyRequired { get; set; }
        
        // tool logic
        public override IToolImpact Use(IDynamicEntity owner, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

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

                if (BlockEmptyRequired)
                {
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
                }

                //cursor.GlobalPosition = owner.EntityState.PickedBlockPosition;

                //If was not possible to set Item Place do nothing
                //if (!SetNewItemPlace(cubeEntity, owner, moveVector)) return impact;

                var pos = GetPosition(owner);

                if (!pos.Valid)
                    return impact;

                cubeEntity.Position = pos.Position;
                cubeEntity.Rotation = pos.Rotation;

                cursor.AddEntity(cubeEntity, owner.DynamicId);
                    
                impact.Success = true;
            }
            return impact;
        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!owner.EntityState.IsBlockPicked)
                return pos;
            
            Vector3 faceOffset = owner.EntityState.PickedBlockFaceOffset;

            // locate the entity
            if (owner.EntityState.PickPointNormal.Y == 1) // = Put on TOP 
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);

            }
            else if (owner.EntityState.PickPointNormal.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y - 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);
            }
            else //Put on a side
            {
                Vector3I newBlockPos;
                if (BlockFaceCentered == false)
                {
                    newBlockPos = owner.EntityState.PickedBlockPosition;
                    pos.Position = new Vector3D(newBlockPos + faceOffset);
                }
                else
                {
                    newBlockPos = owner.EntityState.NewBlockPosition;
                    pos.Position = new Vector3D(newBlockPos + new Vector3(0.5f - (float)owner.EntityState.PickPointNormal.X / 2, 0.5f, 0.5f - (float)owner.EntityState.PickPointNormal.Z / 2));
                }

                pos.Position += new Vector3D(owner.EntityState.PickPointNormal.X == -1 ? -0.01 : 0,
                                                    0,
                                                    owner.EntityState.PickPointNormal.Z == -1 ? -0.01 : 0);


                var slope = 0d;

                if (owner.EntityState.PickPointNormal.X == -1) slope = -Math.PI / 2;
                if (owner.EntityState.PickPointNormal.X == 1) slope = Math.PI / 2; // ok
                if (owner.EntityState.PickPointNormal.Z == -1) slope = Math.PI; // ok
                if (owner.EntityState.PickPointNormal.Z == 1) slope = 0;

                pos.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)slope);
            }

            pos.Valid = true;

            return pos;
        }
    }
}
