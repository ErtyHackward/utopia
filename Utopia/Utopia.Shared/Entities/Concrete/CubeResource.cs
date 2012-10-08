using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using System;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Entities.Concrete
{
    public class CubeResource : StaticEntity, ITool, IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory Factory { get; set; }

        public byte CubeId { get; set; }
    
        public EquipmentSlotType AllowedSlots
        {
            get { return EquipmentSlotType.Hand; }
            set { throw new NotSupportedException(); }
        }

        public int MaxStackSize
        {
            get { return 999; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.CubeResource; }
        }

        public DynamicEntity Parent { get; set; }

        public AbstractChunk ParentChunk { get; set; }
        
        public string StackType
        {
            get
            {
                return "CubeResource" + CubeId; //effectively this.getType().Name + cubeid , so blockadder1 blockadder2 etc ...
            }
        }
        
        public override string DisplayName
        {
            get { return RealmConfiguration.CubeProfiles[CubeId].Name; }
        }

        public string Description
        {
            get { return RealmConfiguration.CubeProfiles[CubeId].Description; }
        }

        public static event EventHandler<CubeChangedEventArgs> CubeChanged;

        public static void OnCubeChanged(CubeChangedEventArgs e)
        {
            var handler = CubeChanged;
            if (handler != null) handler(null, e);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);
            CubeId = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(CubeId);
        }

        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer = false)
        {
            if (owner.EntityState.IsBlockPicked)
            {
                return BlockImpact(owner, useMode, runOnServer);
            }

            if (owner.EntityState.IsEntityPicked)
            {
                return EntityImpact(owner, useMode, runOnServer);
            }

            var impact = new ToolImpact { Success = false };
            impact.Message = "No target selected for use";
            return impact;
        }

        public IToolImpact BlockImpact(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new ToolImpact { Success = false };

            if (entity.EntityState.IsBlockPicked)
            {
                if (useMode == ToolUseMode.LeftMouse)
                {
                    //Remove block and all attached entities to this blocks !
                    var character = owner as CharacterEntity;

                    var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    var cube = cursor.Read();
                    if (cube != RealmConfiguration.CubeId.Air)
                    {
                        var chunk = LandscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                        if (character != null)
                        {
                            foreach (var cubeEntity in chunk.Entities.EnumerateFast())
                            {
                                IBlockLinkedEntity cubeBlockLinkedEntity = cubeEntity as IBlockLinkedEntity;
                                if (cubeBlockLinkedEntity != null && cubeBlockLinkedEntity.LinkedCube == owner.EntityState.PickedBlockPosition)
                                {
                                    //Insert in the inventory the entity that will be removed !
                                    var adder = (IItem)Factory.CreateEntity(cubeEntity.ClassId);
                                    character.Inventory.PutItem(adder);
                                }
                            }
                        }

                        //Removed all entities from collection that where linked to this removed cube !
                        chunk.Entities.RemoveAll<IBlockLinkedEntity>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition);

                        //change the Block to AIR
                        cursor.Write(RealmConfiguration.CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.
                        OnCubeChanged(new CubeChangedEventArgs { DynamicEntity = owner, Position = cursor.GlobalPosition, Value = RealmConfiguration.CubeId.Air });
                        
                        impact.Success = true;

                        return impact;
                    }
                }
                else
                {
                    //Add new block
                    var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                    if (cursor.Read() == RealmConfiguration.CubeId.Air)
                    {
                        cursor.Write(CubeId);
                        OnCubeChanged(new CubeChangedEventArgs { DynamicEntity = owner, Position = cursor.GlobalPosition, Value = CubeId });
                        impact.Success = true;
                        return impact;
                    }
                }
            }
            impact.Message = "Pick a cube to use this tool";
            return impact;
        }

        //Entity impect when a player equiped with a cube click on the entity => Will remove the entity and place it to the inventory.
        private IToolImpact EntityImpact(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer = false)
        {
            var impact = new ToolImpact { Success = false };

            EntityLink entity = owner.EntityState.PickedEntityLink;
            IChunkLayout2D chunk = LandscapeManager.GetChunk(entity.ChunkPosition);
            IStaticEntity entityRemoved;

            //Remove the entity from chunk
            chunk.Entities.RemoveById(entity.Tail[0], owner.DynamicId, out entityRemoved);
            
            var character = owner as CharacterEntity;
            if (character != null && entityRemoved != null)
            {
                //Create a new entity of the same clicked one and place it into the inventory
                var adder = (IItem)Factory.CreateEntity(entityRemoved.ClassId);
                character.Inventory.PutItem(adder);
            }
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }

    public class CubeChangedEventArgs : EventArgs
    {
        public Vector3I Position { get; set; }
        public byte Value { get; set; }
        public IDynamicEntity DynamicEntity { get; set; }
    }
}
