using System.Linq;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using System;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [EditorHideAttribute]
    public class CubeResource : Item, ITool
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

        public override IToolImpact Put(IDynamicEntity owner)
        {
            // don't allow to put out the cube resource
            return new ToolImpact();
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            if (owner.EntityState.IsBlockPicked)
            {
                return BlockImpact(owner);
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
                //Do Dynamic entity collision testing (Cannot place a block if a dynamic entity intersect.
                var blockBB = new BoundingBox(entity.EntityState.NewBlockPosition, entity.EntityState.NewBlockPosition + Vector3.One);
                foreach (var dynEntity in EntityFactory.DynamicEntityManager.EnumerateAround(entity.EntityState.NewBlockPosition))
                {
                    var dynBB = new BoundingBox(dynEntity.Position.AsVector3(), dynEntity.Position.AsVector3() + dynEntity.DefaultSize);
                    if (blockBB.Intersects(ref dynBB)) return impact;
                }

                // Get the chunk where the entity will be added and check if another block static entity is present inside this block
                var workingchunk = LandscapeManager.GetChunk(owner.EntityState.NewBlockPosition);
                foreach (IBlockLocationRoot staticEntity in workingchunk.Entities.Entities.Values.Where(e => e is IBlockLocationRoot))
                {
                    if (staticEntity.BlockLocationRoot == entity.EntityState.NewBlockPosition)
                    {
                        // IBlockLocationRoot Entity already present at this location
                        return impact;
                    }
                }
                
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
