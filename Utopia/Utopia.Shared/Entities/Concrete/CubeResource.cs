using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using System;
using S33M3Resources.Structs;

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
            get { return EquipmentSlotType.LeftHand; }
            set { throw new NotSupportedException(); }
        }

        public int MaxStackSize
        {
            get { return 999; }
        }

        public string UniqueName
        {
            get { return DisplayName; }
            set { throw new NotSupportedException(); }
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
            get { return Cubes.CubeId.GetCubeTypeName(CubeId); }
        }

        public string Description
        {
            get { return Cubes.CubeId.GetCubeDescription(CubeId); }
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
                    var character = owner as CharacterEntity;

                    var cursor = LandscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    var cube = cursor.Read();
                    if (cube != Cubes.CubeId.Air)
                    {
                        var chunk = LandscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                        if (character != null)
                        {
                            foreach (var cubeEntity in chunk.Entities.EnumerateFast())
                            {
                                IBlockLinkedEntity cubeBlockLinkedEntity = cubeEntity as IBlockLinkedEntity;
                                if (cubeBlockLinkedEntity != null && cubeBlockLinkedEntity.LinkedCube == owner.EntityState.PickedBlockPosition)
                                {
                                    var adder = (IItem)Factory.CreateEntity(cubeEntity.ClassId);
                                    if (cubeEntity is IGrowEntity)
                                    {
                                        ((IGrowEntity)adder).GrowPhase = ((IGrowEntity)cubeEntity).GrowPhase;
                                    }
                                    character.Inventory.PutItem(adder);
                                }
                            }
                        }

                        chunk.Entities.RemoveAll<IBlockLinkedEntity>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition);

                        //change the Block to AIR
                        cursor.Write(Cubes.CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.
                        OnCubeChanged(new CubeChangedEventArgs { DynamicEntity = owner, Position = cursor.GlobalPosition, Value = Cubes.CubeId.Air });
                        
                        impact.Success = true;
                        //If the Tool Owner is a player, then Add the resource removed into the inventory
                        if (character != null)
                        {
                            var adder = Factory.CreateEntity<CubeResource>();
                            adder.CubeId = cube;
                            character.Inventory.PutItem(adder);
                        }
                        return impact;
                    }
                }
                else
                {

                    var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                    if (cursor.Read() == Cubes.CubeId.Air)
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

        private IToolImpact EntityImpact(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer = false)
        {
            var impact = new ToolImpact { Success = false };

            EntityLink entity = owner.EntityState.PickedEntityLink;
            IChunkLayout2D chunk = LandscapeManager.GetChunk(entity.ChunkPosition);
            IStaticEntity entityRemoved;
            chunk.Entities.RemoveById(entity.Tail[0], owner.DynamicId, out entityRemoved);
            
            var character = owner as CharacterEntity;
            if (character != null)
            {
                var adder = (IItem)Factory.CreateEntity(entityRemoved.ClassId);
                if (entityRemoved is IGrowEntity)
                {
                    ((IGrowEntity)adder).GrowPhase = ((IGrowEntity)entityRemoved).GrowPhase;
                }
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
