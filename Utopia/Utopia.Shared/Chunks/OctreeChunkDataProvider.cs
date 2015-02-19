using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using S33M3Resources.Structs;

namespace Utopia.Shared.Chunks
{
    public class OctreeChunkDataProvider : ChunkDataProvider
    {
        private readonly OctreeNode _rootNode = new OctreeNode();

        public override Vector3I ChunkSize {
            get { return new Vector3I(16, 16, 16); }
            set { throw new NotSupportedException(); } 
        }
        public override ChunkColumnInfo[] ColumnsInfo { get; set; }

        public override ChunkColumnInfo GetColumnInfo(int inChunkPositionX, int inChunkPositionZ)
        {
            throw new NotImplementedException();
        }

        public override ChunkMetaData ChunkMetaData { get; set; }

        public override byte[] GetBlocksBytes()
        {
            throw new NotImplementedException();
        }

        public override byte GetBlock(Vector3I position)
        {
            OctreeNode node = _rootNode;
            int sizeFactor = 16;
            while (node.HasChilds)
            {
                node = node.Childs[GetArrayIndexFromPos(position, sizeFactor)];
                sizeFactor /= 2;
            }
            return node.Value;
        }

        public override IEnumerable<KeyValuePair<Vector3I, BlockTag>> GetTags()
        {
            throw new NotImplementedException();
        }

        public override BlockTag GetTag(Vector3I inChunkPosition)
        {
            throw new NotImplementedException();
        }

        public override void SetBlock(Vector3I position, byte blockValue, BlockTag tag = null, uint sourceDynamicId = 0)
        {
            OctreeNode node = _rootNode;
            int sizeFactor = 16;
            while (node.HasChilds)
            {
                node = node.Childs[GetArrayIndexFromPos(position, sizeFactor)];
                sizeFactor /= 2;
            }

            if (node.Value == blockValue)
                return;

            // split until the end

            bool needMerge = true;

            var prevValue = node.Value;

            while (sizeFactor >= 2)
            {
                node.Split(prevValue);
                node = node.Childs[GetArrayIndexFromPos(position, sizeFactor)];
                sizeFactor /= 2;
                needMerge = false;
            }
            
            node.Value = blockValue;
            
            if (!needMerge)
                return;

            // merge

            var inheritance = new List<OctreeNode>();
            inheritance.Add(_rootNode);

            node = _rootNode;

            while (node.HasChilds)
            {
                node = node.Childs[GetArrayIndexFromPos(position, sizeFactor)];
                inheritance.Add(node);
                sizeFactor /= 2;
            }

            for (int i = inheritance.Count - 1; i >= 0; i--)
            {
                if (inheritance[i].Childs.All(n => !n.HasChilds && n.Value == blockValue))
                {
                    inheritance[i].Value = blockValue;
                    inheritance[i].Childs = null;
                }
            }

        }

        public override void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null, uint sourceDynamicId = 0)
        {
            throw new NotImplementedException();
        }

        public override void SetBlockBytes(byte[] bytes, IEnumerable<KeyValuePair<Vector3I, BlockTag>> tags = null)
        {
            throw new NotImplementedException();
        }

        public override object WriteSyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public int GetArrayIndexFromPos(Vector3I position, int sizeFactor)
        {
            return (position.x % sizeFactor >= sizeFactor / 2 ? 1 : 0) +
                   (position.z % sizeFactor >= sizeFactor / 2 ? 2 : 0) +
                   (position.y % sizeFactor >= sizeFactor / 2 ? 4 : 0);
        }
    }

    public class OctreeNode
    {
        public OctreeNode[] Childs;
        public byte Value;

        public bool HasChilds
        {
            get { return Childs != null; }
        }

        public void Split(byte value)
        {
            Childs = new OctreeNode[8];
            for (int i = 0; i < 8; i++)
            {
                Childs[i] = new OctreeNode();
                Childs[i].Value = value;
            }
        }
    }

}
