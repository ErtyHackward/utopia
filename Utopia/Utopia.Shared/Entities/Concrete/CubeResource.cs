using ProtoBuf;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using System;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class CubeResource : StaticEntity, ITool, IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory entityFactory { get; set; }

        [ProtoMember(1)]
        public byte CubeId { get; private set; }
    
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
        
        public string Description
        {
            get { return "A world Cube"; }
        }

        public void SetCube(byte cubeId, string cubeName)
        {
            CubeId = cubeId;
            Name = cubeName;
        }

        public IToolImpact Use(IDynamicEntity owner, bool runOnServer = false)
        {
            if (owner.EntityState.IsBlockPicked)
            {
                return BlockImpact(owner, runOnServer);
            }

            var impact = new ToolImpact { Success = false };
            impact.Message = "No target selected for use";
            return impact;
        }

        public IToolImpact BlockImpact(IDynamicEntity owner, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new ToolImpact { Success = false };

            if (entity.EntityState.IsBlockPicked)
            {
                //Add new block
                var cursor = LandscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                if (cursor.Read() == WorldConfiguration.CubeId.Air)
                {
                    cursor.Write(CubeId);
                    impact.Success = true;
                    return impact;
                }
            }
            impact.Message = "Pick a cube to use this tool";
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
