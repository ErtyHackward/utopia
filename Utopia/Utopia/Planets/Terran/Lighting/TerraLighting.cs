using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Chunk;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Maths;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;

namespace Utopia.Planets.Terran.Lighting
{
    public static class TerraLighting
    {
        public static byte LightPropagateSteps = 8;

        private static byte lightDecreaseStep = (byte)(256 / LightPropagateSteps);

        private enum LightComponent
        {
            SunLight,
            Red,
            Green,
            Blue
        }

        public static void SetLightSources(LandScape landscape, ref Range<int> cubeRange)
        {
            int index;
            bool blockLight = false;
            int maxSunLight;
            RenderCubeProfile cubeprofile;

            for (int X = cubeRange.Min.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Min.Z; Z < cubeRange.Max.Z; Z++)
                {
                    blockLight = false;
                    maxSunLight = 255;
                    index = -1;
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Min.Y; Y--)
                    {
                        if (index == -1)
                        {
                            index = MathHelper.Mod(X, LandscapeBuilder.Worldsize.X) + MathHelper.Mod(Z, LandscapeBuilder.Worldsize.Z) * LandscapeBuilder.Worldsize.X + Y * LandscapeBuilder.Worldsize.X * LandscapeBuilder.Worldsize.Z;
                        }
                        else
                        {
                            index = landscape.FastIndex(index, Y + 1, IdxRelativeMove.Y_Minus1);
                        }

                        //Create SunLight LightSources from AIR blocs
                        cubeprofile = RenderCubeProfile.CubesProfile[landscape.Cubes[index].Id];
                        if ((!blockLight && cubeprofile.IsBlockingLight)) blockLight = true; //If my block is blocking light, stop sunlight propagation !
                        if (cubeprofile.IsFlooding)
                        {
                            maxSunLight -= 32;
                            maxSunLight = Math.Max(maxSunLight, 0);
                        }
                        if (!blockLight)
                        {
                            landscape.Cubes[index].EmissiveColor.SunLight = (byte)maxSunLight;
                        }
                        else landscape.Cubes[index].EmissiveColor.SunLight = 0;

                        if (cubeprofile.IsEmissiveColorLightSource)
                        {
                            landscape.Cubes[index].EmissiveColor.R = RenderCubeProfile.CubesProfile[landscape.Cubes[index].Id].EmissiveColor.R;
                            landscape.Cubes[index].EmissiveColor.G = RenderCubeProfile.CubesProfile[landscape.Cubes[index].Id].EmissiveColor.G;
                            landscape.Cubes[index].EmissiveColor.B = RenderCubeProfile.CubesProfile[landscape.Cubes[index].Id].EmissiveColor.B;
                        }
                        else
                        {
                            landscape.Cubes[index].EmissiveColor.R = 0;
                            landscape.Cubes[index].EmissiveColor.G = 0;
                            landscape.Cubes[index].EmissiveColor.B = 0;
                        }
                    }
                }
            }
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        public static void PropagateLightSource(TerraWorld _terraWorld, ref Range<int> cubeRange, bool borderAsLightSource = false)
        {
            RenderCubeProfile cubeprofile;
            bool borderchunk = false;
            int index;
            //Foreach Blocks in the chunks
            for (int X = cubeRange.Min.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Min.Z; Z < cubeRange.Max.Z; Z++)
                {
                    index = -1;
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Min.Y; Y--)
                    {
                        if (X == cubeRange.Min.X || X == cubeRange.Max.X - 1 || Z == cubeRange.Min.Z || Z == cubeRange.Max.Z - 1) borderchunk = true;
                        else borderchunk = false;

                        if (index == -1)
                        {
                            index = MathHelper.Mod(X, LandscapeBuilder.Worldsize.X) + MathHelper.Mod(Z, LandscapeBuilder.Worldsize.Z) * LandscapeBuilder.Worldsize.X + Y * LandscapeBuilder.Worldsize.X * LandscapeBuilder.Worldsize.Z;
                        }
                        else
                        {
                            index = _terraWorld.Landscape.FastIndex(index, Y + 1, IdxRelativeMove.Y_Minus1);
                        }

                        cubeprofile = RenderCubeProfile.CubesProfile[_terraWorld.Landscape.Cubes[index].Id];
                        if (cubeprofile.IsBlockingLight && !cubeprofile.IsEmissiveColorLightSource) continue;

                        if (_terraWorld.Landscape.Cubes[index].EmissiveColor.SunLight == 255 || (borderAsLightSource && borderchunk)) PropagateLight(_terraWorld, X, Y, Z, _terraWorld.Landscape.Cubes[index].EmissiveColor.SunLight, LightComponent.SunLight, true, index, IdxRelativeMove.None);
                        if (cubeprofile.IsEmissiveColorLightSource || (borderAsLightSource && borderchunk))
                        {
                            if (_terraWorld.Landscape.Cubes[index].EmissiveColor.R > 0) PropagateLight(_terraWorld, X, Y, Z, _terraWorld.Landscape.Cubes[index].EmissiveColor.R, LightComponent.Red, true, index, IdxRelativeMove.None);
                            if (_terraWorld.Landscape.Cubes[index].EmissiveColor.G > 0) PropagateLight(_terraWorld, X, Y, Z, _terraWorld.Landscape.Cubes[index].EmissiveColor.G, LightComponent.Green, true, index, IdxRelativeMove.None);
                            if (_terraWorld.Landscape.Cubes[index].EmissiveColor.B > 0) PropagateLight(_terraWorld, X, Y, Z, _terraWorld.Landscape.Cubes[index].EmissiveColor.B, LightComponent.Blue, true, index, IdxRelativeMove.None);
                        }
                    }
                }
            }
        }

        private static void PropagateLight(TerraWorld _terraWorld, int X, int Y, int Z, int LightValue, LightComponent lightComp, bool isLightSource, int baseIndex, IdxRelativeMove relativeMove)
        {
            RenderCubeProfile cubeprofile;
            TerraCube cube;
            int index = baseIndex;

            if (!isLightSource)
            {
                if (LightValue <= 0) return; // No reason to propate "no light";

                //if (!landscape.Index(X, Y, Z, true, out index)) return; // Cube out of border !
                //Inlining the Fct for perf. purpose
                if (X < _terraWorld.WorldRange.Min.X || X >= _terraWorld.WorldRange.Max.X || Z < _terraWorld.WorldRange.Min.Z || Z >= _terraWorld.WorldRange.Max.Z || Y < 0 || Y >= _terraWorld.WorldRange.Max.Y)
                {
                    return;
                }

                if (relativeMove != IdxRelativeMove.None)
                {
                    index = _terraWorld.Landscape.FastIndex(baseIndex, X, Y, Z, relativeMove, false);
                }

                //End Inlining ===============================================================================================

                //End propagation ?
                cube = _terraWorld.Landscape.Cubes[index];
                cubeprofile = RenderCubeProfile.CubesProfile[cube.Id];
                if (cubeprofile.IsBlockingLight) return;      // Do nothing if my block don't let the light pass !
                switch (lightComp)
                {
                    case LightComponent.SunLight:
                        if (cube.EmissiveColor.SunLight >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _terraWorld.Landscape.Cubes[index].EmissiveColor.SunLight = (byte)LightValue;
                        break;
                    case LightComponent.Red:
                        if (cube.EmissiveColor.R >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _terraWorld.Landscape.Cubes[index].EmissiveColor.R = (byte)LightValue;
                        break;
                    case LightComponent.Green:
                        if (cube.EmissiveColor.G >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _terraWorld.Landscape.Cubes[index].EmissiveColor.G = (byte)LightValue;
                        break;
                    case LightComponent.Blue:
                        if (cube.EmissiveColor.B >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _terraWorld.Landscape.Cubes[index].EmissiveColor.B = (byte)LightValue;
                        break;
                }

            }
            //else
            //{
            //    cubeprofile = new RenderCubeProfile();
            //}

            //Don't propagate if I don't let the light going through me

            //Call the 6 surrounding blocks !
            if (!isLightSource || lightComp != LightComponent.SunLight)
            {
                PropagateLight(_terraWorld, X, Y + 1, Z, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.Y_Plus1);
                PropagateLight(_terraWorld, X, Y - 1, Z, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.Y_Minus1);
            }

            PropagateLight(_terraWorld, X + 1, Y, Z, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.X_Plus1);
            PropagateLight(_terraWorld, X, Y, Z + 1, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.Z_Plus1);
            PropagateLight(_terraWorld, X - 1, Y, Z, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.X_Minus1);
            PropagateLight(_terraWorld, X, Y, Z - 1, LightValue - lightDecreaseStep, lightComp, false, index, IdxRelativeMove.Z_Minus1);

        }

    }
}
