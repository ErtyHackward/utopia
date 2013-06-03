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
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;

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
            _lightDecreaseStep = (byte)(159 / (_lightPropagateSteps - 1));
        }

        #region Public methods
        public void CreateChunkLightSources(VisualChunk chunk)
        {
            CreateLightSources(chunk);
            chunk.State = ChunkState.LightsSourceCreated;
        }

        public void PropagateInnerChunkLightSources(VisualChunk chunk)
        {
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
            CreateLightSources(ref cubeRange, chunk.BlockData.ChunkMetaData.ChunkMaxHeightBuilt);
        }

        //Create light source on a specific Cube Range (not specific to a single chunk)
        public void CreateLightSources(ref Range3I cubeRange, byte maxHeight = 0)
        {
            int index;
            bool blockLight = false;
            int SunLight;
            BlockProfile blockProfile;

            int maxheight = maxHeight == 0 ? cubeRange.Max.Y - 1 : maxHeight;

            for (int X = cubeRange.Position.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Position.Z; Z < cubeRange.Max.Z; Z++)
                {
                    blockLight = false;
                    SunLight = 255;
                    index = _cubesHolder.Index(X, maxheight, Z);

                    for (int Y = maxheight; Y >= cubeRange.Position.Y; Y--)
                    {
                        //Create SunLight LightSources from AIR blocs
                        blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[_cubesHolder.Cubes[index].Id];
                        if ((!blockLight && blockProfile.IsBlockingLight)) blockLight = true; //If my block is blocking light, stop sunlight propagation !
                        if (!blockLight)
                        {
                            SunLight -= blockProfile.LightAbsorbed;
                            if (SunLight < 0)
                            {
                                SunLight = 0;
                                _cubesHolder.Cubes[index].IsSunLightSource = true;
                            }
                            else
                            {
                                _cubesHolder.Cubes[index].IsSunLightSource = true;
                            }
                            _cubesHolder.Cubes[index].EmissiveColor.A = (byte)SunLight;

                        }
                        else
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.A = 0;
                            _cubesHolder.Cubes[index].IsSunLightSource = false;
                        }


                        if (blockProfile.IsEmissiveColorLightSource)
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.R = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[_cubesHolder.Cubes[index].Id].EmissiveColor.R;
                            _cubesHolder.Cubes[index].EmissiveColor.G = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[_cubesHolder.Cubes[index].Id].EmissiveColor.G;
                            _cubesHolder.Cubes[index].EmissiveColor.B = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[_cubesHolder.Cubes[index].Id].EmissiveColor.B;
                        }
                        else
                        {
                            _cubesHolder.Cubes[index].EmissiveColor.R = 0;
                            _cubesHolder.Cubes[index].EmissiveColor.G = 0;
                            _cubesHolder.Cubes[index].EmissiveColor.B = 0;
                        }

                        index -= _cubesHolder.MoveY;
                    }
                }
            }

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
                Vector3I entityBlockPosition = new Vector3I(MathHelper.Floor(entityWorldPosition.X),
                                                            MathHelper.Floor(entityWorldPosition.Y),
                                                            MathHelper.Floor(entityWorldPosition.Z));

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
            PropagateLightSources(ref cubeRangeWithOffset, false, maxHeight: chunk.BlockData.ChunkMetaData.ChunkMaxHeightBuilt);
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
            BlockProfile blockProfile;
            int index = _cubesHolder.Index(ref cubePosition);
            TerraCube cube = _cubesHolder.Cubes[index];

            blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.Id];
            if (blockProfile.IsBlockingLight && !blockProfile.IsEmissiveColorLightSource) return;
            PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.A, LightComponent.SunLight, true, index);

            if (cube.EmissiveColor.R > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.R, LightComponent.Red, true, index);
            if (cube.EmissiveColor.G > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.G, LightComponent.Green, true, index);
            if (cube.EmissiveColor.B > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.B, LightComponent.Blue, true, index);
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        public void PropagateLightSources(ref Range3I cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false, byte maxHeight = 0)
        {
            BlockProfile blockProfile;
            int index;

            TerraCube cube;

            int maxheight = maxHeight == 0 ? cubeRange.Max.Y - 1 : maxHeight;

            //Foreach Blocks in the Range
            for (int X = cubeRange.Position.X; X < cubeRange.Max.X; X++)
            {
                for (int Z = cubeRange.Position.Z; Z < cubeRange.Max.Z; Z++)
                {
                    index = _cubesHolder.Index(X, maxheight, Z);

                    for (int Y = maxheight; Y >= cubeRange.Position.Y; Y--)
                    {
                        cube = _cubesHolder.Cubes[index];
                        blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.Id];

                        if (cube.IsSunLightSource || (borderAsLightSource)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.A, LightComponent.SunLight, true, index);
                        if (cube.EmissiveColor.R > 0 || (borderAsLightSource)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.R, LightComponent.Red, true, index);
                        if (cube.EmissiveColor.G > 0 || (borderAsLightSource)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.G, LightComponent.Green, true, index);
                        if (cube.EmissiveColor.B > 0 || (borderAsLightSource)) 
                            PropagateLight(X, Y, Z, cube.EmissiveColor.B, LightComponent.Blue, true, index);

                        index -= _cubesHolder.MoveY;
                    }
                }
            }
            if (withRangeEntityPropagation) PropagateLightInsideStaticEntities(ref cubeRange);
        }

        //Propagate lights Algo.
        private void PropagateLight(int X, int Y, int Z, int LightValue, LightComponent lightComp, bool isLightSource, int index)
        {

            BlockProfile blockProfile;
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
                blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.Id];
                if (blockProfile.IsBlockingLight) return;      // Do nothing if my block don't let the light pass !
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

            byte lightAttenuation = LightValue == 255 ? (byte)96 : _lightDecreaseStep;

            //Call the 6 surrounding blocks !
            if (!isLightSource || lightComp != LightComponent.SunLight)
            {
                PropagateLight(X, Y + 1, Z, LightValue - lightAttenuation, lightComp, false, index + _cubesHolder.MoveY);
                PropagateLight(X, Y - 1, Z, LightValue - lightAttenuation, lightComp, false, index - _cubesHolder.MoveY);
            }

            //X + 1, Y, Z
            PropagateLight(X + 1, Y, Z, LightValue - lightAttenuation, lightComp, false, index + _cubesHolder.MoveX);
            //X, Y, Z + 1
            PropagateLight(X, Y, Z + 1, LightValue - lightAttenuation, lightComp, false, index + _cubesHolder.MoveZ);
            //X - 1, Y, Z
            PropagateLight(X - 1, Y, Z, LightValue - lightAttenuation, lightComp, false, index - _cubesHolder.MoveX);
            //X, Y, Z - 1
            PropagateLight(X, Y, Z - 1, LightValue - lightAttenuation, lightComp, false, index - _cubesHolder.MoveZ);
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
            foreach (var voxelEntity in chunk.AllEntities())
            {
                if (false /* voxelEntity.Entity is BlockLinkedItem*/)
                {
                    voxelEntity.BlockLight = _cubesHolder.Cubes[_cubesHolder.Index(((BlockLinkedItem)voxelEntity.Entity).BlockLocationRoot)].EmissiveColor;
                }
                else
                {
                    //Find the Cube where the entity is placed, and assign its color to the entity
                    voxelEntity.BlockLight = _cubesHolder.Cubes[_cubesHolder.Index(MathHelper.Floor(voxelEntity.Entity.Position.X), MathHelper.Floor(voxelEntity.Entity.Position.Y), MathHelper.Floor(voxelEntity.Entity.Position.Z))].EmissiveColor;
                }   
            }
        }
        #endregion

    }
}
