﻿using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Cubes;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Structs
{
    public struct TerraCubeWithPosition
    {
        public TerraCube Cube;
        public Vector3I Position;
        public CubeProfile CubeProfile;

        public static TerraCubeWithPosition DefaultValue = default(TerraCubeWithPosition);

        public TerraCubeWithPosition(Vector3I pos, TerraCube cube)
        {
            CubeProfile = GameSystemSettings.Current.Settings.CubesProfile[cube.Id];
            Position = pos;
            Cube = cube;
        }
    }
}
