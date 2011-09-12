using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Concrete;
using SharpDX;

namespace Utopia.Shared.Chunks.Entities.Voxel
{
    public class PlayerCharacterBody : VoxelEntity
    {


        public PlayerCharacterBody()
        {
            Blocks = new byte[1, 1, 1];
            Blocks[0, 0, 0] = 15;
        }

        public override EntityClassId ClassId
        {
            get { return 0; }
        }

        public override string DisplayName
        {
            get { return ""; }
        }
    }
}
