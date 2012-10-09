using System;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Entities;
using Utopia.Shared.Settings;
using S33M3DXEngine.Threading;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;
using System.Linq;
using System.Collections.Generic;

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

            _lightPropagateSteps = 8;
            _lightDecreaseStep = (byte)(256 / _lightPropagateSteps);
        }

        #region Public methods
        //Create the LightSources for a specific range
        public void CreateChunkLightSources(VisualChunk chunk)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            CreateLightSources(chunk);
            chunk.State = ChunkState.LightsSourceCreated;
        }

        //Create the LightSources for a specific range
        public void PropagateInnerChunkLightSources(VisualChunk chunk)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            PropagatesLightSources(chunk);
            chunk.IsOutsideLightSourcePropagated = false;
            chunk.State = ChunkState.InnerLightsSourcePropagated;
        }

        public void PropagateOutsideChunkLightSources(VisualChunk chunk)
        {
            //If my Chunk is a border chunk, then don't propagate surrounding chunk light
            if (chunk.IsBorderChunk == false)
            {
                PropagatesBorderLightSources(chunk);
                chunk.IsOutsideLightSourcePropagated = true;
            }

            chunk.State = ChunkState.OuterLightSourcesProcessed;
        }

        #endregion

        #region Private methods

        //Light Source Creation ============================================================================================================

        //Create the landscape for the chunk
        private void CreateLightSources(VisualChunk chunk)
        {
            Range3I cubeRange = chunk.CubeRange;
            CreateLightSources(ref cubeRange);
        }

        //Create light source on a specific Cube Range (not specific to a single chunk)
        public void CreateLightSources(ref Range3I cubeRange)
        {
            int index;
            bool blockLight = false;
            int maxSunLight;
            CubeProfile cubeprofile;

            for (int X = cubeRange.Position.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Position.Z; Z < cubeRange.Max.Z; Z++)
                {
                    blockLight = false;
                    maxSunLight = 255;
                    index = _cubesHolder.Index(X, cubeRange.Max.Y - 1, Z);
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Position.Y; Y--)
                    {
                        if (Y != cubeRange.Max.Y - 1) index -= _cubesHolder.MoveY;

                        //Create SunLight LightSources from AIR blocs
                        cubeprofile = GameSystemSettings.Current.Settings.CubesProfile[_cubesHolder.Cubes[index].Id];
                        if ((!blockLight && cubeprofile.IsBlockingLight)) blockLight = true; //If my block is blocking light, stop sunlight propagation !
                        if (!blockLight)
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.A = (byte)maxSunLight;
                        }
                        else _cubesHolder.Cubes[index].EmissiveColor.A = 0;

                        if (cubeprofile.IsEmissiveColorLightSource)
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.R = GameSystemSettings.Current.Settings.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.R;
                            _cubesHolder.Cubes[index].EmissiveColor.G = GameSystemSettings.Current.Settings.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.G;
                            _cubesHolder.Cubes[index].EmissiveColor.B = GameSystemSettings.Current.Settings.CubesProfile[_cubesHolder.Cubes[index].Id].EmissiveColor.B;
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

            ////Recreate the light sources from the entities on the impacted chunks
            //foreach (var chunk in impactedChunks)
            //{
            //    CreateEntityLightSources(chunk);
            //}

            //Find all chunk from the Cube range !
            foreach (VisualChunk chunk in WorldChunk.Chunks)
            {
                if ((chunk.CubeRange.Max.X < cubeRange.Position.X) || (chunk.CubeRange.Position.X > cubeRange.Max.X)) continue;
                if ((chunk.CubeRange.Max.Y < cubeRange.Position.Y) || (chunk.CubeRange.Position.Y > cubeRange.Max.Y)) continue;
                CreateEntityLightSources(chunk);
            }
        }

        public void CreateEntityLightSources(VisualChunk chunk)
        {
            foreach (ILightEmitterEntity LightingEntity in chunk.Entities.Enumerate<ILightEmitterEntity>())
            {
                //Get the Cube where is located the entity
                Vector3D entityWorldPosition = ((IEntity)LightingEntity).Position;
                Vector3I entityBlockPosition = new Vector3I(MathHelper.Fastfloor(entityWorldPosition.X),
                                                            MathHelper.Fastfloor(entityWorldPosition.Y),
                                                            MathHelper.Fastfloor(entityWorldPosition.Z));

                //Get big array index of this cube
                int index = _cubesHolder.Index(ref entityBlockPosition);
                _cubesHolder.Cubes[index].EmissiveColor.R = LightingEntity.EmittedLightColor.R;
                _cubesHolder.Cubes[index].EmissiveColor.G = LightingEntity.EmittedLightColor.G;
                _cubesHolder.Cubes[index].EmissiveColor.B = LightingEntity.EmittedLightColor.B;
            }
        }

        //Light Propagation =================================================================================================================
        private void PropagatesLightSources(VisualChunk chunk)
        {
            Range3I cubeRangeWithOffset = chunk.CubeRange;
            PropagateLightSources(ref cubeRangeWithOffset, false);
            PropagateLightInsideStaticEntities(chunk);
        }

        private void PropagatesBorderLightSources(VisualChunk chunk)
        {
            //Get surrending cubes from this chunk
            Range3I cubeRangeWithBorder = chunk.CubeRange;
            cubeRangeWithBorder.Position.X--;
            cubeRangeWithBorder.Position.Z--;
            cubeRangeWithBorder.Size.X += 2;
            cubeRangeWithBorder.Size.Z += 2;

            foreach (var BorderCube in cubeRangeWithBorder.AllExclude(chunk.CubeRange))
            {
                PropagateLightSourcesForced(BorderCube, chunk);
            }

            PropagateLightInsideStaticEntities(chunk);
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        //Will force lighting from cubes passed, even if not Alpha is not 255 = borderAsLightSource = true
        private void PropagateLightSourcesForced(Vector3I cubePosition, VisualChunk chunk)
        {
            CubeProfile cubeprofile;
            int index = _cubesHolder.Index(ref cubePosition);
            TerraCube cube = _cubesHolder.Cubes[index];

            cubeprofile = GameSystemSettings.Current.Settings.CubesProfile[cube.Id];
            if (cubeprofile.IsBlockingLight && !cubeprofile.IsEmissiveColorLightSource) return;
            PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.A, LightComponent.SunLight, true, index);

            if (cube.EmissiveColor.R > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.R, LightComponent.Red, true, index);
            if (cube.EmissiveColor.G > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.G, LightComponent.Green, true, index);
            if (cube.EmissiveColor.B > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.B, LightComponent.Blue, true, index);
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        public void PropagateLightSources(ref Range3I cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false)
        {
            CubeProfile cubeprofile;
            bool borderchunk = false;
            int index;

            TerraCube cube;

            //Foreach Blocks in the Range
            for (int X = cubeRange.Position.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Position.Z; Z < cubeRange.Max.Z; Z++)
                {
                    index = _cubesHolder.Index(X, cubeRange.Max.Y - 1, Z);
                    for (int Y = cubeRange.Max.Y - 1; Y >= cubeRange.Position.Y; Y--)
                    {
                        if (X == cubeRange.Position.X || X == cubeRange.Max.X || Z == cubeRange.Position.Z || Z == cubeRange.Max.Z) borderchunk = true;
                        else borderchunk = false;

                        if (Y != cubeRange.Max.Y - 1) index -= _cubesHolder.MoveY;

                        cube = _cubesHolder.Cubes[index];
                        cubeprofile = GameSystemSettings.Current.Settings.CubesProfile[cube.Id];

                        if (cube.EmissiveColor.A == 255 || (borderAsLightSource && borderchunk)) PropagateLight(X, Y, Z, cube.EmissiveColor.A, LightComponent.SunLight, true, index);
                        if (cube.EmissiveColor.R > 0 || (borderAsLightSource && borderchunk)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.R, LightComponent.Red, true, index);
                        if (cube.EmissiveColor.G > 0 || (borderAsLightSource && borderchunk)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.G, LightComponent.Green, true, index);
                        if (cube.EmissiveColor.B > 0 || (borderAsLightSource && borderchunk)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.B, LightComponent.Blue, true, index);
                    }
                }
            }
            if (withRangeEntityPropagation) PropagateLightInsideStaticEntities(ref cubeRange);
        }

        //Propagate lights Algo.
        private void PropagateLight(int X, int Y, int Z, int LightValue, LightComponent lightComp, bool isLightSource, int index)
        {
            CubeProfile cubeprofile;
            TerraCube cube;

            if (!isLightSource)
            {
                if (LightValue <= 0) return; // No reason to propate "no light";

                if (X < _visualWorldParameters.WorldRange.Position.X || X >= _visualWorldParameters.WorldRange.Max.X || Z < _visualWorldParameters.WorldRange.Position.Z || Z >= _visualWorldParameters.WorldRange.Max.Z || Y < 0 || Y >= _visualWorldParameters.WorldRange.Max.Y)
                {
                    return;
                }
                //Avoid to be outside the Array ==> Trick to remove the need to check for border limit, but could lead to small graphical artifact with lighting !
                index = _cubesHolder.MakeIndexSafe(index);
                //End Inlining ===============================================================================================

                //End propagation ?
                cube = _cubesHolder.Cubes[index];
                cubeprofile = GameSystemSettings.Current.Settings.CubesProfile[cube.Id];
                if (cubeprofile.IsBlockingLight) return;      // Do nothing if my block don't let the light pass !
                switch (lightComp)
                {
                    case LightComponent.SunLight:
                        if (cube.EmissiveColor.A >= LightValue && isLightSource == false) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.A = (byte)LightValue;
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

        //Propagate the light inside the chunk entities
        private void PropagateLightInsideStaticEntities(ref Range3I cubeRange)
        {
            VisualChunk chunk;
            //Find all chunk from the Cube range !
            for (int i = 0; i < WorldChunk.Chunks.Length; i++)
            {
                chunk = WorldChunk.Chunks[i];
                if ((chunk.CubeRange.Max.X < cubeRange.Position.X) || (chunk.CubeRange.Position.X > cubeRange.Max.X)) continue;
                if ((chunk.CubeRange.Max.Y < cubeRange.Position.Y) || (chunk.CubeRange.Position.Y > cubeRange.Max.Y)) continue;
                PropagateLightInsideStaticEntities(chunk);
            }
        }

        //Propagate the light inside the chunk entities
        private void PropagateLightInsideStaticEntities(VisualChunk chunk)
        {
            foreach (var pair in chunk.VisualVoxelEntities)
            {
                foreach (var voxelEntity in pair.Value)
                {
                    //Find the Cube where the entity is placed, and assign its color to the entity
                    voxelEntity.BlockLight = _cubesHolder.Cubes[_cubesHolder.Index(MathHelper.Fastfloor(voxelEntity.Entity.Position.X), MathHelper.Fastfloor(voxelEntity.Entity.Position.Y), MathHelper.Fastfloor(voxelEntity.Entity.Position.Z))].EmissiveColor;
                }
            }
        }
        #endregion

    }
}
