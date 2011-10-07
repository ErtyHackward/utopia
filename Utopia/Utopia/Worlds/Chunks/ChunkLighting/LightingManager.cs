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
using Utopia.Shared.World;
using Utopia.Settings;
using Utopia.Entities;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Interfaces;

namespace Utopia.Worlds.Chunks.ChunkLighting
{
    public class LightingManager : ILightingManager
    {
        #region Private variable
        private SingleArrayChunkContainer _cubesHolder;

        private VisualWorldParameters _visualWorldParameters;

        private byte _lightPropagateSteps;
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
        public byte LightPropagateSteps
        {
            get { return _lightPropagateSteps; }
        }

        public IWorldChunks WorldChunk { get; set; }

        #endregion

        public LightingManager(SingleArrayChunkContainer cubesHolder, VisualWorldParameters visualWorldParameters)
        {
            _cubesHolder = cubesHolder;
            _visualWorldParameters = visualWorldParameters;

            _lightPropagateSteps = (byte)ClientSettings.Current.Settings.GraphicalParameters.LightPropagateSteps;
            _lightDecreaseStep = (byte)(256 / _lightPropagateSteps);
        }

        #region Public methods
        //Create the LightSources for a specific range
        public void CreateChunkLightSources(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
             WorkQueue.DoWorkInThread(new WorkItemCallback(CreateLightSources_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
        }

        //Create the LightSources for a specific range
        public void PropagateChunkLightSources(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            WorkQueue.DoWorkInThread(new WorkItemCallback(PropagatesLightSources_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
        }

        #endregion

        #region Private methods
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

            PropagateLightInsideStaticEntities(chunk);

            chunk.State = ChunkState.LandscapeLightsPropagated;
            chunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }


        //Can only be done if surrounding chunks have their landscape initialized !
        public void PropagateLightSources(ref Range<int> cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false)
        {
            VisualCubeProfile cubeprofile;
            bool borderchunk = false;
            int index;
            //Foreach Blocks in the Range
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
                            if (_cubesHolder.Cubes[index].EmissiveColor.R > 0) PropagateLight(X,Y,Z, _cubesHolder.Cubes[index].EmissiveColor.R, LightComponent.Red, true, index);
                            if (_cubesHolder.Cubes[index].EmissiveColor.G > 0) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.G, LightComponent.Green, true, index);
                            if (_cubesHolder.Cubes[index].EmissiveColor.B > 0) PropagateLight(X, Y, Z, _cubesHolder.Cubes[index].EmissiveColor.B, LightComponent.Blue, true, index);
                        }
                    }
                }
            }
            if (withRangeEntityPropagation) PropagateLightInsideStaticEntities(ref cubeRange);
        }

        //Propagate the light inside the chunk entities
        private void PropagateLightInsideStaticEntities(ref Range<int> cubeRange)
        {
            VisualChunk chunk;
            //Find all chunk from the Cube range !
            for (int i = 0; i < WorldChunk.Chunks.Length; i++)
            {
                chunk = WorldChunk.Chunks[i];
                if ((chunk.CubeRange.Max.X < cubeRange.Min.X) || (chunk.CubeRange.Min.X > cubeRange.Max.X))continue;
                if ((chunk.CubeRange.Max.Y < cubeRange.Min.Y) || (chunk.CubeRange.Min.Y > cubeRange.Max.Y))continue;
                PropagateLightInsideStaticEntities(chunk);
            }
        }

        //Propagate the light inside the chunk entities
        private void PropagateLightInsideStaticEntities(VisualChunk chunk)
        {
            IVisualEntity vertexEntity;
            for (int i = 0; i < chunk.VisualSpriteEntities.Count; i++)
            {
                vertexEntity = chunk.VisualSpriteEntities[i];
                //Find the Cube below entity, and assign its color to the entity
                chunk.VisualSpriteEntities[i].color = _cubesHolder.Cubes[_cubesHolder.Index(MathHelper.Fastfloor(vertexEntity.SpriteEntity.Position.X), MathHelper.Fastfloor(vertexEntity.SpriteEntity.Position.Y), MathHelper.Fastfloor(vertexEntity.SpriteEntity.Position.Z))].EmissiveColor;
            }
        }

        private void PropagateLight(int X, int Y, int Z, int LightValue, LightComponent lightComp, bool isLightSource, int index)
        {
            VisualCubeProfile cubeprofile;
            TerraCube cube;

            if (!isLightSource)
            {
                if (LightValue <= 0) return; // No reason to propate "no light";

                if (X < _visualWorldParameters.WorldRange.Min.X || X >= _visualWorldParameters.WorldRange.Max.X || Z < _visualWorldParameters.WorldRange.Min.Z || Z >= _visualWorldParameters.WorldRange.Max.Z || Y < 0 || Y >= _visualWorldParameters.WorldRange.Max.Y)
                {
                    return;
                }
                //Avoid to be outside the Array ==> Trick to remove the need to check for border limit, but could lead to small graphical artifact with lighting !
                index = _cubesHolder.MakeIndexSafe(index);
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
                PropagateLight(X, Y + 1, Z, LightValue - _lightDecreaseStep, lightComp, false, index + _cubesHolder.MoveY);
                PropagateLight(X, Y - 1, Z, LightValue - _lightDecreaseStep, lightComp, false, index - _cubesHolder.MoveY);
            }

            //X + 1, Y, Z
            PropagateLight(X + 1, Y, Z, LightValue - _lightDecreaseStep, lightComp, false, index + _cubesHolder.MoveX);
            //X, Y, Z + 1
            PropagateLight(X, Y, Z + 1, LightValue - _lightDecreaseStep, lightComp, false, index + _cubesHolder.MoveZ);
            //X - 1, Y, Z
            PropagateLight(X - 1, Y, Z, LightValue - _lightDecreaseStep, lightComp, false, index - _cubesHolder.MoveX);
            //X, Y, Z - 1
            PropagateLight(X, Y, Z - 1, LightValue - _lightDecreaseStep, lightComp, false, index - _cubesHolder.MoveZ);
        }
        #endregion

    }
}
