using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Chunks;
using S33M3Engines.Threading;
using Amib.Threading;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Worlds.Chunks.ChunkLighting
{
    public class LightingManager : ILightingManager
    {
        #region Private variable
        private SingleArrayChunkContainer _cubesHolder;

        private delegate object CreateLightSourcesDelegate(object Params);
        private CreateLightSourcesDelegate _createLightSourcesDelegate;

        private delegate object PropagateLightSourcesDelegate(object Params);
        private PropagateLightSourcesDelegate _propagateLightSourcesDelegate;

        private byte _LightPropagateSteps;
        private byte _lightDecreaseStep;
        private enum LightComponent
        {
            SunLight,
            Red,
            Green,
            Blue
        }

        #endregion

        #region Public variable
        #endregion

        public LightingManager(SingleArrayChunkContainer cubesHolder)
        {
            _cubesHolder = cubesHolder;

            _LightPropagateSteps = 8;
            _lightDecreaseStep = (byte)(256 / _LightPropagateSteps);
        }

        #region Public methods
        //Create the LightSources for a specific range
        public void CreateChunkLightSources(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            if (Async)
            {
                WorkQueue.DoWorkInThread(new WorkItemCallback(CreateLightSources_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            }
            else
            {
                _createLightSourcesDelegate.Invoke(chunk);
            }
        }

        //Create the LightSources for a specific range
        public void PropagateChunkLightSources(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            if (Async)
            {
                WorkQueue.DoWorkInThread(new WorkItemCallback(PropagatesLightSources_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            }
            else
            {
                _propagateLightSourcesDelegate.Invoke(chunk);
            }
        }

        #endregion

        #region Private methods
        private void Intialize()
        {
            _createLightSourcesDelegate = new CreateLightSourcesDelegate(CreateLightSources_threaded);
            _createLightSourcesDelegate = new CreateLightSourcesDelegate(PropagatesLightSources_threaded);
        }

        //Create the landscape for the chunk
        private object CreateLightSources_threaded(object Params)
        {
            VisualChunk chunk = (VisualChunk)Params;

            Range<int> cubeRange = chunk.CubeRange;

            CreateLightSources(ref cubeRange);

            chunk.State = ChunkState.LandscapeLightsSourceCreated;
            chunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }

        public void CreateLightSources(ref Range<int> cubeRange)
        {
            int index;
            bool blockLight = false;
            int maxSunLight;
            VisualCubeProfile cubeprofile;

            for (int X = cubeRange.Min.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Min.Z; Z < cubeRange.Max.Z; Z++)
                {
                    blockLight = false;
                    maxSunLight = 255;
                    index = _cubesHolder.Index(X, cubeRange.Max.Y - 1, Z);
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Min.Y; Y--)
                    {
                        if (Y != cubeRange.Max.Y - 1) index -= _cubesHolder.MoveY;

                        //Create SunLight LightSources from AIR blocs
                        cubeprofile = VisualCubeProfile.CubesProfile[_cubesHolder.Cubes[index].Id];
                        if ((!blockLight && cubeprofile.IsBlockingLight)) blockLight = true; //If my block is blocking light, stop sunlight propagation !
                        if (cubeprofile.IsFlooding)
                        {
                            maxSunLight -= 32;
                            maxSunLight = Math.Max(maxSunLight, 0);
                        }
                        if (!blockLight)
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.SunLight = (byte)maxSunLight;
                        }
                        else _cubesHolder.Cubes[index].EmissiveColor.SunLight = 0;

                        if (cubeprofile.IsEmissiveColorLightSource)
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.R = VisualCubeProfile.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.R;
                            _cubesHolder.Cubes[index].EmissiveColor.G = VisualCubeProfile.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.G;
                            _cubesHolder.Cubes[index].EmissiveColor.B = VisualCubeProfile.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.B;
                        }
                        else
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.R = 0;
                            _cubesHolder.Cubes[index].EmissiveColor.G = 0;
                            _cubesHolder.Cubes[index].EmissiveColor.B = 0;
                        }
                    }
                }
            }
        }


        //Create the landscape for the chunk
        private object PropagatesLightSources_threaded(object Params)
        {
            VisualChunk chunk = (VisualChunk)Params;

            bool borderAsLightSource = false;
            if (chunk.LightPropagateBorderOffset.X != 0 || chunk.LightPropagateBorderOffset.Z != 0) borderAsLightSource = true;
            Range<int> cubeRangeWithOffset = chunk.CubeRange;
            if (chunk.LightPropagateBorderOffset.X > 0) cubeRangeWithOffset.Max.X += chunk.LightPropagateBorderOffset.X;
            else cubeRangeWithOffset.Min.X += chunk.LightPropagateBorderOffset.X;

            if (chunk.LightPropagateBorderOffset.Z > 0) cubeRangeWithOffset.Max.Z += chunk.LightPropagateBorderOffset.Z;
            else cubeRangeWithOffset.Min.Z += chunk.LightPropagateBorderOffset.Z;

            PropagateLightSources(ref cubeRangeWithOffset, borderAsLightSource);

            chunk.State = ChunkState.LandscapeLightsPropagated;
            chunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }


        //Can only be done if surrounding chunks have their landscape initialized !
        public void PropagateLightSources(ref Range<int> cubeRange, bool borderAsLightSource = false)
        {
            VisualCubeProfile cubeprofile;
            bool borderchunk = false;
            int index;
            //Foreach Blocks in the chunks
            for (int X = cubeRange.Min.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Min.Z; Z < cubeRange.Max.Z; Z++)
                {
                    index = _cubesHolder.Index(X, cubeRange.Max.Y - 1, Z);
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Min.Y; Y--)
                    {
                        if (X == cubeRange.Min.X || X == cubeRange.Max.X - 1 || Z == cubeRange.Min.Z || Z == cubeRange.Max.Z - 1) borderchunk = true;
                        else borderchunk = false;

                        if (Y != cubeRange.Max.Y - 1) index -= _cubesHolder.MoveY;

                        cubeprofile = VisualCubeProfile.CubesProfile[_cubesHolder.Cubes[index].Id];
                        if (cubeprofile.IsBlockingLight && !cubeprofile.IsEmissiveColorLightSource) continue;

                        if (_cubesHolder.Cubes[index].EmissiveColor.SunLight == 255 || (borderAsLightSource && borderchunk)) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.SunLight, LightComponent.SunLight, true, index);
                        if (cubeprofile.IsEmissiveColorLightSource || (borderAsLightSource && borderchunk))
                        {
                            if (_cubesHolder.Cubes[index].EmissiveColor.R > 0) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.R, LightComponent.Red, true, index);
                            if (_cubesHolder.Cubes[index].EmissiveColor.G > 0) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.G, LightComponent.Green, true, index);
                            if (_cubesHolder.Cubes[index].EmissiveColor.B > 0) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.B, LightComponent.Blue, true, index);
                        }
                    }
                }
            }
        }

        private void PropagateLight(int X, int Y, int Z, int LightValue, LightComponent lightComp, bool isLightSource, int index)
        {
            VisualCubeProfile cubeprofile;
            TerraCube cube;

            if (!isLightSource)
            {
                if (LightValue <= 0) return; // No reason to propate "no light";

                index = _cubesHolder.ValidateIndex(index);

                //End Inlining ===============================================================================================

                //End propagation ?
                cube = _cubesHolder.Cubes[index];
                cubeprofile = VisualCubeProfile.CubesProfile[cube.Id];
                if (cubeprofile.IsBlockingLight) return;      // Do nothing if my block don't let the light pass !
                switch (lightComp)
                {
                    case LightComponent.SunLight:
                        if (cube.EmissiveColor.SunLight >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.SunLight = (byte)LightValue;
                        break;
                    case LightComponent.Red:
                        if (cube.EmissiveColor.R >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.R = (byte)LightValue;
                        break;
                    case LightComponent.Green:
                        if (cube.EmissiveColor.G >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.G = (byte)LightValue;
                        break;
                    case LightComponent.Blue:
                        if (cube.EmissiveColor.B >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.B = (byte)LightValue;
                        break;
                }
            }

            //Don't propagate if I don't let the light going through me

            //Call the 6 surrounding blocks !
            if (!isLightSource || lightComp != LightComponent.SunLight)
            {
                PropagateLight(X, Y + 1, Z, LightValue - _lightDecreaseStep, lightComp, false, index += _cubesHolder.MoveY);
                PropagateLight(X, Y - 1, Z, LightValue - _lightDecreaseStep, lightComp, false, index -= _cubesHolder.MoveY);
            }

            PropagateLight(X + 1, Y, Z, LightValue - _lightDecreaseStep, lightComp, false, index += _cubesHolder.MoveX);
            PropagateLight(X, Y, Z + 1, LightValue - _lightDecreaseStep, lightComp, false, index += _cubesHolder.MoveZ);
            PropagateLight(X - 1, Y, Z, LightValue - _lightDecreaseStep, lightComp, false, index -= _cubesHolder.MoveX);
            PropagateLight(X, Y, Z - 1, LightValue - _lightDecreaseStep, lightComp, false, index -= _cubesHolder.MoveZ);

        }




        #endregion


        
    }
}
