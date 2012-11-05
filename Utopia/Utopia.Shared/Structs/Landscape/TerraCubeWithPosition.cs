﻿using Utopia.Shared.Structs.Landscape;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Structs
{
    public struct TerraCubeWithPosition
    {
        public TerraCube Cube;
        public Vector3I Position;
        public CubeProfile CubeProfile;

        public static TerraCubeWithPosition DefaultValue = default(TerraCubeWithPosition);

        public TerraCubeWithPosition(Vector3I pos, TerraCube cube, WorldConfiguration configuration)
        {
            CubeProfile = configuration.CubeProfiles[cube.Id];
            Position = pos;
            Cube = cube;
        }

        public TerraCubeWithPosition(Vector3I pos, byte cubeId, WorldConfiguration configuration)
        {
            CubeProfile = configuration.CubeProfiles[cubeId];
            Position = pos;
            Cube = new TerraCube(cubeId);
        }
    }
}
