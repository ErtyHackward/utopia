using ProtoBuf;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using System;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class CubeResource : Item
    {
        [ProtoMember(1)]
        public byte CubeId { get; private set; }
    
        public override ushort ClassId
        {
            get { return EntityClassId.CubeResource; }
        }

        public DynamicEntity Parent { get; set; }

        public AbstractChunk ParentChunk { get; set; }
        
        public void SetCube(byte cubeId, string cubeName)
        {
            CubeId = cubeId;
            Name = cubeName;
        }

        public override IToolImpact Use(IDynamicEntity owner, bool runOnServer = false)
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
    }

    public class CubeChangedEventArgs : EventArgs
    {
        public Vector3I Position { get; set; }
        public byte Value { get; set; }
        public IDynamicEntity DynamicEntity { get; set; }
    }
}
