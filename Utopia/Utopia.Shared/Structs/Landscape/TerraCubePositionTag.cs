using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Structs.Landscape
{
    public class TerraCubePositionTag
    {
        public TerraCube Cube;
        public Vector3I Position;
        public CubeProfile CubeProfile;
        public BlockTag Tag;

        public static TerraCubePositionTag DefaultValue = default(TerraCubePositionTag);

        public TerraCubePositionTag(Vector3I pos, TerraCube cube, BlockTag tag)
        {
            CubeProfile = GameSystemSettings.Current.Settings.CubesProfile[cube.Id];
            Position = pos;
            Cube = cube;
            Tag = tag;
        }

        public TerraCubePositionTag(Vector3I pos, byte cubeId, BlockTag tag)
        {
            CubeProfile = GameSystemSettings.Current.Settings.CubesProfile[cubeId];
            Position = pos;
            Cube = new TerraCube(cubeId);
            Tag = tag;
        }
    }
}
