using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    public class CubeResource : StaticEntity, ITool
    {
        private readonly ILandscapeManager2D _landscapeManager;
        
        public byte CubeId { get; set; }
    
        public EquipmentSlotType AllowedSlots
        {
            get { return EquipmentSlotType.LeftHand | EquipmentSlotType.RightHand; }
            set { throw new System.NotSupportedException(); }
        }

        public int MaxStackSize
        {
            get { return 999; }
        }

        public string UniqueName
        {
            get { return DisplayName; }
            set { throw new System.NotSupportedException(); }
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.CubeResource; }
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
        
        public CubeResource(ILandscapeManager2D landscapeManager)
        {
            _landscapeManager = landscapeManager;
        }

        public override string DisplayName
        {
            get { return Utopia.Shared.Cubes.CubeId.GetCubeTypeName(CubeId); }
        }

        public string Description
        {
            get { return Utopia.Shared.Cubes.CubeId.GetCubeDescription(CubeId); }
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);
            CubeId = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(CubeId);
        }

        public IToolImpact Use(IDynamicEntity owner, byte useMode, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new ToolImpact { Success = false };

            if (entity.EntityState.IsBlockPicked)
            {
                if (useMode == 0)
                {
                    var cursor = _landscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);
                    var cube = cursor.Read();
                    if (cube != Utopia.Shared.Cubes.CubeId.Air)
                    {
                        var chunk = _landscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);
                        
                        chunk.Entities.RemoveAll<IBlockLinkedEntity>(e => e.LinkedCube == owner.EntityState.PickedBlockPosition);

                        //change the Block to AIR
                        cursor.Write(Utopia.Shared.Cubes.CubeId.Air); //===> Need to do this AFTER Because this will trigger chunk Rebuilding in the Client ... need to change it.
                        impact.Success = true;
                        return impact;
                    }
                }
                else
                {

                    var cursor = _landscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                    if (cursor.Read() == Utopia.Shared.Cubes.CubeId.Air)
                    {
                        cursor.Write(CubeId);
                        impact.Success = true;
                        return impact;
                    }
                }
            }
            impact.Message = "Pick a cube to use this tool";
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }



    }
}
