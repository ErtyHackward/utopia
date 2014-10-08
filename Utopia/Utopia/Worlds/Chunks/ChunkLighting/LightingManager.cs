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
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Worlds.Chunks.ChunkLighting
{
    public class LightingManager : ILightingManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private SingleArrayChunkContainer _cubesHolder;

        private VisualWorldParameters _visualWorldParameters;

        private byte _lightPropagateSteps;
        private byte _lightDecreaseStep;

        private Vector3I _worldRange;
        private Vector3I _worldRangeMax;

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

        public IWorldChunks2D WorldChunk { get; set; }

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
            // speed optimization
            _worldRange = _visualWorldParameters.WorldRange.Position;
            _worldRangeMax = _visualWorldParameters.WorldRange.Max;

            PropagatesLightSources(chunk);
            chunk.IsOutsideLightSourcePropagated = false;
            chunk.State = ChunkState.InnerLightsSourcePropagated;
        }

        public void PropagateOutsideChunkLightSources(VisualChunk chunk)
        {
            // speed optimization
            _worldRange = _visualWorldParameters.WorldRange.Position;
            _worldRangeMax = _visualWorldParameters.WorldRange.Max;

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
                Vector3I entityBlockPosition = ((IEntity)LightingEntity).Position.ToCubePosition();

                //Get big array index of this cube
                int index = _cubesHolder.Index(ref entityBlockPosition);
                _cubesHolder.Cubes[index].EmissiveColor.R = LightingEntity.EmittedLightColor.R;
                _cubesHolder.Cubes[index].EmissiveColor.G = LightingEntity.EmittedLightColor.G;
                _cubesHolder.Cubes[index].EmissiveColor.B = LightingEntity.EmittedLightColor.B;
            }

            //Create the light sources from entities present on surrending chunks, with a block positionned inside me
            if (chunk.FourSurroundingChunks != null)
            {
                foreach (var surrendingChunk in chunk.FourSurroundingChunks)
                {
                    //Propagate the light from light entities linked to border !
                    foreach (ILightEmitterEntity LightingEntity in surrendingChunk.OutOfChunkLightSourceStaticEntities)
                    {
                        //Get the Cube where is located the entity
                        Vector3I entityBlockPosition = LightingEntity.Position.ToCubePosition();
                        if (chunk.CubeRange.Contains(entityBlockPosition))
                        {
                            //Get big array index of this cube
                            int index = _cubesHolder.Index(ref entityBlockPosition);
                            _cubesHolder.Cubes[index].EmissiveColor.R = LightingEntity.EmittedLightColor.R;
                            _cubesHolder.Cubes[index].EmissiveColor.G = LightingEntity.EmittedLightColor.G;
                            _cubesHolder.Cubes[index].EmissiveColor.B = LightingEntity.EmittedLightColor.B;
                        }
                    }
                }
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

            var test = cubeRangeWithBorder.AllExclude(chunk.CubeRange);
            foreach (var BorderCube in cubeRangeWithBorder.AllExclude(chunk.CubeRange))
            {
                PropagateLightSourcesForced(BorderCube, chunk);
            }

            //Propagate the light from Entities located in chunks around me, but that have a light source block inside my chunk !
            foreach (var surrendingChunk in chunk.FourSurroundingChunks)
            {
                //Propagate the light from light entities linked to border !
                foreach (ILightEmitterEntity LightingEntity in surrendingChunk.OutOfChunkLightSourceStaticEntities)
                {
                    //Get the Cube where is located the entity
                    Vector3I entityBlockPosition = LightingEntity.Position.ToCubePosition();
                    if (chunk.CubeRange.Contains(entityBlockPosition))
                    {
                        PropagateLightSourcesForced(entityBlockPosition, chunk);
                    }
                }
            }

            PropagateLightInsideStaticEntities(chunk);
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        //Will force lighting from cubes passed, even if not Alpha is not 255 = borderAsLightSource = true
        private void PropagateLightSourcesForced(Vector3I cubePosition, VisualChunk chunk)
        {
            int index = _cubesHolder.Index(ref cubePosition);
            TerraCube cube = _cubesHolder.Cubes[index];

            PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.A, LightComponent.SunLight, true, index);

            if (cube.EmissiveColor.R > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.R, LightComponent.Red, true, index);
            if (cube.EmissiveColor.G > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.G, LightComponent.Green, true, index);
            if (cube.EmissiveColor.B > 0) PropagateLight(cubePosition.X, cubePosition.Y, cubePosition.Z, cube.EmissiveColor.B, LightComponent.Blue, true, index);
        }

        //Can only be done if surrounding chunks have their landscape initialized !
        public void PropagateLightSources(ref Range3I cubeRange, bool borderAsLightSource = false, bool withRangeEntityPropagation = false, byte maxHeight = 0)
        {
            // speed optimization
            _worldRange = _visualWorldParameters.WorldRange.Position;
            _worldRangeMax = _visualWorldParameters.WorldRange.Max;

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
                if (LightValue <= 0) 
                    return; // No reason to propate "no light";

                if (X < _worldRange.X || X >= _worldRangeMax.X || Z < _worldRange.Z || Z >= _worldRangeMax.Z || Y < 0 || Y >= _worldRangeMax.Y)
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
                        if (cube.EmissiveColor.A >= LightValue) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.A = (byte)LightValue;
                        break;
                    case LightComponent.Red:
                        if (cube.EmissiveColor.R >= LightValue) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.R = (byte)LightValue;
                        break;
                    case LightComponent.Green:
                        if (cube.EmissiveColor.G >= LightValue) return;   // Do nothing because my block color is already above the proposed one !   
                        _cubesHolder.Cubes[index].EmissiveColor.G = (byte)LightValue;
                        break;
                    case LightComponent.Blue:
                        if (cube.EmissiveColor.B >= LightValue) return;   // Do nothing because my block color is already above the proposed one !   
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

            PropagateLight(X + 1, Y, Z, LightValue - lightAttenuation, lightComp, false, _cubesHolder.FastIndex(index, X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1));
            //X, Y, Z + 1
            PropagateLight(X, Y, Z + 1, LightValue - lightAttenuation, lightComp, false, _cubesHolder.FastIndex(index, Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1));
            //X - 1, Y, Z
            PropagateLight(X - 1, Y, Z, LightValue - lightAttenuation, lightComp, false, _cubesHolder.FastIndex(index, X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1));
            //X, Y, Z - 1
            PropagateLight(X, Y, Z - 1, LightValue - lightAttenuation, lightComp, false, _cubesHolder.FastIndex(index, Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1));
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

                voxelEntity.BlockLight = _cubesHolder.Cubes[_cubesHolder.Index(voxelEntity.Entity.Position.ToCubePosition())].EmissiveColor;
            }
        }
        #endregion

    }
}
