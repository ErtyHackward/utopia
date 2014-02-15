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
            return new ToolImpact { Message = "This action is not allowed by design" };
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            ToolImpact impact = null;

            if (!CanDoBlockAction(owner, ref impact))
                return impact;
            
            return BlockImpact(owner);
        }

        public IToolImpact BlockImpact(IDynamicEntity owner, bool runOnServer = false)
        {
            var entity = owner;
            var impact = new ToolImpact();

            if (entity.EntityState.IsBlockPicked)
            {
                //Do Dynamic entity collision testing (Cannot place a block if a dynamic entity intersect.
                var blockBB = new BoundingBox(entity.EntityState.NewBlockPosition, entity.EntityState.NewBlockPosition + Vector3.One);
                foreach (var dynEntity in EntityFactory.DynamicEntityManager.EnumerateAround(entity.EntityState.NewBlockPosition))
                {
                    var dynBB = new BoundingBox(dynEntity.Position.AsVector3(), dynEntity.Position.AsVector3() + dynEntity.DefaultSize);
                    if (blockBB.Intersects(ref dynBB))
                    {
                        impact.Message = "Cannot place a block where someone is standing";
                        return impact;
                    }
                }

                // Get the chunk where the entity will be added and check if another block static entity is present inside this block
                var workingchunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.NewBlockPosition);
                foreach (var staticEntity in workingchunk.Entities.OfType<IBlockLocationRoot>())
                {
                    if (staticEntity.BlockLocationRoot == entity.EntityState.NewBlockPosition)
                    {
                        impact.Message = "There is something there, remove it first " + staticEntity.BlockLocationRoot;
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
