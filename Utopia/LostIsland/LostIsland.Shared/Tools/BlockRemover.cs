using System;
using System.Collections.Generic;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    public abstract class BlockRemover : Entity, ITool
    {
        private readonly ILandscapeManager2D _landscapeManager;

        protected HashSet<byte> RemoveableCubeIds = new HashSet<byte>();

        protected BlockRemover(ILandscapeManager2D landscapeManager2D)
        {
            _landscapeManager = landscapeManager2D;
        }

        public IToolImpact Use(IDynamicEntity owner, bool runOnServer = false)
        {
            if (owner.EntityState.IsPickingActive)
            {
                if (owner.EntityState.IsEntityPicked)
                {
                    return EntityImpact(owner);
                }
                else
                {
                    return BlockImpact(owner);
                }
            }
            else
            {
                var impact = new ToolImpact { Success = false };
                impact.Message = "No target selected for use";
                return impact;
            }
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }

        public Utopia.Shared.Entities.Inventory.EquipmentSlotType AllowedSlots
        {
            get
            {
                return Utopia.Shared.Entities.Inventory.EquipmentSlotType.LeftHand | Utopia.Shared.Entities.Inventory.EquipmentSlotType.RightHand;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int MaxStackSize
        {
            get { return 1; }
        }

        public string UniqueName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string StackType
        {
            get { return "BlockRemover"; }
        }

        private IToolImpact BlockImpact(IDynamicEntity owner)
        {
            var impact = new ToolImpact { Success = false };
            var cursor = _landscapeManager.GetCursor(owner.EntityState.PickedBlockPosition);
            byte cube = cursor.Read();
            if (cube != CubeId.Air)
            {
                //Check static entity impact of the Block removal.
                //Get the chunk
                var chunk = _landscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                IBlockLinkedEntity entity;
                Entity removedEntity;
                for (int entityId = chunk.Entities.Data.Count - 1; entityId >= 0; entityId--)
                {
                    entity = chunk.Entities.Data[entityId] as IBlockLinkedEntity;

                    if (entity != null)
                    {
                        //If the linkedCube entity is removed, then remove the entity also.
                        if (entity.LinkedCube == owner.EntityState.PickedBlockPosition)
                        {
                            Console.WriteLine("Remove Entity from Collection");
                            chunk.Entities.RemoveByArrayIndex(entityId, owner.EntityId, out removedEntity);
                            // Add entity on ground or in Inventory
                            //TOTO Enity picking ??
                        }
                    }
                }

                //change the Block to AIR
                cursor.Write(CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.
                impact.Success = true;

                //If the Tool Owner is a player, then Add the resource removed into the inventory
                var character = owner as CharacterEntity;
                if (character != null)
                {
                    var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
                    adder.CubeId = cube;
                    character.Inventory.PutItem(adder);
                }
                return impact;
            }
            impact.Message = "Cannot remove Air block !";
            return impact;
        }

        private IToolImpact EntityImpact(IDynamicEntity owner)
        {
            var impact = new ToolImpact { Success = false };
            //var cursor = _landscapeManager.GetCursor(Parent.EntityState.PickedBlockPosition);
            //byte cube = cursor.Read();
            //if (cube != 0)
            //{
            //    cursor.Write(0);
            //    impact.Success = true;

            //    var character = Parent as CharacterEntity;
            //    if (character != null)
            //    {
            //        var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
            //        adder.CubeId = cube;

            //        character.Inventory.PutItem(adder);
            //    }

            //    return impact;
            //}
            return impact;
        }

    }
}
