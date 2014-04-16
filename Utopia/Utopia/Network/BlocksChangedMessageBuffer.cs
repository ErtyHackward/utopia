using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Net.Messages;

namespace Utopia.Network
{
    public class BlocksChangedMessageBuffer
    {
        private List<Vector3I> _blockPositions = new List<Vector3I>();
        private List<byte> _blockValues = new List<byte>();
        private List<BlockTag> _tags = new List<BlockTag>();

        private float cumulatedElapsedTime;

        public void Add(BlocksChangedMessage msg)
        {
            _blockPositions.AddRange(msg.BlockPositions);
            _blockValues.AddRange(msg.BlockValues);
            if (msg.Tags != null)
            {
                _tags.AddRange(msg.Tags);
            }
            else
            {
                for(int i = 0; i < msg.BlockPositions.Length; i++)
                {
                    _tags.Add(null);
                }
            }
        }

        public BlocksChangedMessage Flush(float elapsedTime, bool forced)
        {
            if(_blockPositions.Count == 0) return null;

            cumulatedElapsedTime += elapsedTime;

            if (cumulatedElapsedTime > 0.032 || forced)
            {
                BlocksChangedMessage bufferedMessage = new BlocksChangedMessage()
                {
                    BlockPositions = _blockPositions.ToArray(),
                    BlockValues = _blockValues.ToArray(),
                    Tags = _tags.ToArray(),
                };

                _blockPositions.Clear();
                _blockValues.Clear();
                _tags.Clear();
                cumulatedElapsedTime = 0;

                return bufferedMessage;
            }
            return null;
        }

    }
}
