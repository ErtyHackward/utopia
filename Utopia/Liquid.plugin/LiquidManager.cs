using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Utopia.PlugIn;
using System.ComponentModel.Composition;
using Utopia.Planets.Terran.Cube;
using Utopia.Planets.Terran;
using Utopia.Planets.Terran.World;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.Flooding;
using S33M3Engines.D3D;
using S33M3Engines.TypeExtension;
using S33M3Engines.Maths;
using Liquid.plugin.LiquidsContent.Effects;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D11;
using Utopia.Planets.Terran.Chunk;
using Utopia.Univers;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;

namespace Liquid.plugin
{
    [Export(typeof(IUniversePlugin))]
    public class LiquidManager : IUniversePlugin
    {
        private Liquid _liquid;
        private Terra _terra;

        public HLSLLiquid LiquidEffect;

        private string _name = "Liquids";
        private string _version = "1.00";
        private long FloodingSpeed = (long)(Stopwatch.Frequency / 5);
        private long FloodingSpeedTex = (long)(Stopwatch.Frequency / 30);
        private long previousTime, currentTime, previousTimeTex, currentTimeTex;
        private long timeAccumulator, timeAccumulatorTex;
        private Universe _universe;

        public float TextureAnimationOffset = 0;
        // The Activated flooding cubes !
        public List<WaterPool> WaterPools = new List<WaterPool>();

        public LiquidManager()
        {
        }

        public string PluginName
        {
            get { return _name; }
        }

        public string PluginVersion
        {
            get { return _version; }
        }

        public void Initialize(Universe universe)
        {
            _universe = universe;
            _terra = universe.Planet.Terra;
            _liquid = new Liquid(this, _terra);

            _terra.DrawLiquid = DrawLiquid;

            _terra.Player.RefreshHeadUnderWater = RefreshHeadUnderWater;
        }

        TerraCube _headCube;
        int _headCubeIndex;
        private void RefreshHeadUnderWater()
        {
            int EyeWorldX = MathHelper.Fastfloor(_terra.Player.CameraWorldPosition.X);
            int EyeWorldY = MathHelper.Fastfloor(_terra.Player.CameraWorldPosition.Y);
            int EyeWorldZ = MathHelper.Fastfloor(_terra.Player.CameraWorldPosition.Z);

            if (_terra.World.Landscape.SafeIndexY(EyeWorldX, EyeWorldY, EyeWorldZ, out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _terra.World.Landscape.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    if (_headCube.Id == CubeId.WaterSource)
                    {
                        if (_terra.World.Landscape.Cubes[_terra.World.Landscape.FastIndex(_headCubeIndex, EyeWorldY, IdxRelativeMove.Y_Plus1)].Id == CubeId.Air)
                        {
                            float minV, MaxV;

                            Liquid il = (Liquid)_liquid;
                            float noiseOffsetX0Z0 = (float)MathHelper.Lerp(0f, MathHelper.Pi, (float)il.Noise.GetNoise2DValue(EyeWorldX, EyeWorldZ, 1, 1).Value);
                            float noiseOffsetX1Z0 = (float)MathHelper.Lerp(0f, MathHelper.Pi, (float)il.Noise.GetNoise2DValue(EyeWorldX + 1, EyeWorldZ, 1, 1).Value);
                            float noiseOffsetX0Z1 = (float)MathHelper.Lerp(0f, MathHelper.Pi, (float)il.Noise.GetNoise2DValue(EyeWorldX, EyeWorldZ + 1, 1, 1).Value);
                            float noiseOffsetX1Z1 = (float)MathHelper.Lerp(0f, MathHelper.Pi, (float)il.Noise.GetNoise2DValue(EyeWorldX + 1, EyeWorldZ + 1, 1, 1).Value);

                            if (noiseOffsetX0Z0 < noiseOffsetX1Z0)
                            {
                                minV = noiseOffsetX0Z0;
                                MaxV = noiseOffsetX1Z0;
                            }
                            else
                            {
                                minV = noiseOffsetX1Z0;
                                MaxV = noiseOffsetX0Z0;
                            }

                            float noiseX = MathHelper.Lerp(minV, MaxV, (float)_terra.Player.CameraWorldPosition.X - EyeWorldX);

                            if (noiseOffsetX0Z1 < noiseOffsetX1Z1)
                            {
                                minV = noiseOffsetX0Z1;
                                MaxV = noiseOffsetX1Z1;
                            }
                            else
                            {
                                minV = noiseOffsetX1Z1;
                                MaxV = noiseOffsetX0Z1;
                            }

                            float noiseZ = MathHelper.Lerp(minV, MaxV, (float)_terra.Player.CameraWorldPosition.X - EyeWorldX);

                            if (noiseX < noiseZ)
                            {
                                minV = noiseX;
                                MaxV = noiseZ;
                            }
                            else
                            {
                                minV = noiseZ;
                                MaxV = noiseX;
                            }

                            float noise = MathHelper.Lerp(minV, MaxV, (float)_terra.Player.CameraWorldPosition.Z - EyeWorldZ);

                            float value = (float)MathHelper.Clamp((Math.Sin(noise + il.WaveGlobalOffset.ActualValue) + 1) / 2, 0.05, 0.95); //Offseting the Y

                            if (EyeWorldY + 1 - value < _terra.Player.CameraWorldPosition.Y) _terra.Player.HeadInsideWater = false;
                            else _terra.Player.HeadInsideWater = true;
                        }
                        else
                        {
                            _terra.Player.HeadInsideWater = true;
                        }
                    }
                    else
                    {
                        _terra.Player.HeadInsideWater = true;
                    }
                }
                else
                {
                    _terra.Player.HeadInsideWater = false;
                }
            }
        }

        //Handling Block change arround a liquid block ==> Can trigger the reactivation of this water block for flooding !!
        //Called on Event ! ("Block Changed")
        public bool EntityBlockReplaced(ref Location3<int> cubeCoordinates, ref TerraCube newCube)
        {
            WaterPool waterPool = new WaterPool();

            //If the new block is a flooding cube
            if (RenderCubeProfile.CubesProfile[newCube.Id].IsFlooding)
            {
                FloodingData floodingData;

                //if (newCube.Type == CubeType.WaterSource && cubeCoordinates.Y > TerraWorld.SeaLevel) return false; // Cannot place Sea water above seaLevel !!

                floodingData = new FloodingData();
                floodingData.CubeLocation = cubeCoordinates;

                if (newCube.Id == CubeId.WaterSource) //Treat Water see as inifinite liquid
                {
                    newCube.MetaData1 = (byte)cubeCoordinates.Y;
                    floodingData.FloodingPower = RenderCubeProfile.CubesProfile[newCube.Id].FloodingPropagationPower;
                    newCube.MetaData2 = (byte)floodingData.FloodingPower;
                    waterPool.FloodData.Enqueue(floodingData);
                }
                else //Treat All other flooding type as finite liquid
                {
                    floodingData.FloodingPower = RenderCubeProfile.CubesProfile[newCube.Id].FloodingPropagationPower;
                    newCube.MetaData1 = (int)TerraFlooding.FloodDirection.Undefined;
                    newCube.MetaData2 = (byte)floodingData.FloodingPower;
                    waterPool.FloodData.Enqueue(floodingData);
                }
            }
            else
            {
                _liquid.Activate(ref cubeCoordinates, ref newCube, ref waterPool);
            }

            if (waterPool.FloodData.Count > 0) WaterPools.Add(waterPool);

            return true;
        }

        //Handling the Update of the Liquid Flooding
        //Called every frame
        public void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
            LiquidFloodingUpdate();
            LiquidTextureAnimation();
            _liquid.Update(ref TimeSpend);
        }

        public void Draw()
        {
        }

        public void DrawLiquid()
        {
            Matrix worldFocus = Matrix.Identity;

            LiquidEffect.Begin();
            LiquidEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_terra.Game.ActivCamera.ViewProjection3D);
            LiquidEffect.CBPerFrame.Values.dayTime = _terra.GameClock.ClockTimeNormalized2;
            LiquidEffect.CBPerFrame.Values.fogdist = ((LandscapeBuilder.Worldsize.X) / 2) - 48;

            if (_terra.Player.HeadInsideWater)
            {
                LiquidEffect.CBPerFrame.Values.SunColor = new Vector3(_terra.SunColorBase / 2, _terra.SunColorBase / 2, _terra.SunColorBase / 2);
            }
            else
            {
                LiquidEffect.CBPerFrame.Values.SunColor = new Vector3(_terra.SunColorBase, _terra.SunColorBase, _terra.SunColorBase);
            }
            LiquidEffect.CBPerFrame.Values.WaveGlobalOffset = _liquid.WaveGlobalOffset.ActualValue;
            LiquidEffect.CBPerFrame.Values.BackBufferSize = new Vector2(_terra.Game.ViewPort.Width, _terra.Game.ViewPort.Height);
            LiquidEffect.CBPerFrame.Values.LiquidOffset = TextureAnimationOffset;
            LiquidEffect.CBPerFrame.IsDirty = true;

            LiquidEffect.SolidBackBuffer.Value = _terra.Game.D3dEngine.StaggingBackBuffer;
            LiquidEffect.SolidBackBuffer.IsDirty = true;

            if (_terra.Player.HeadInsideWater)
            {
                StatesRepository.ApplyStates(Utopia.GameDXStates.DXStates.Rasters.CullNone, Utopia.GameDXStates.DXStates.Blenders.Disabled);
            }
            else
            {
                StatesRepository.ApplyStates(Utopia.GameDXStates.DXStates.Rasters.Default, Utopia.GameDXStates.DXStates.Blenders.Disabled);
            }

#if DEBUG
            if (_universe.Game.DebugDisplay == 2) StatesRepository.ApplyStates(Utopia.GameDXStates.DXStates.Rasters.Wired, Utopia.GameDXStates.DXStates.Blenders.Disabled);
#endif

            TerraChunk chunk;
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize; chunkIndice++)
            {
                chunk = _terra.World.Chunks[chunkIndice];
                if (chunk.Ready2Draw && !chunk.FrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    if (chunk.LiquidCubeVB != null)
                    {
                        MathHelper.CenterOnFocus(ref chunk.World, ref worldFocus, ref _terra.Game.WorldFocus);
                        LiquidEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        LiquidEffect.CBPerDraw.Values.popUpYOffset = chunk.PopUpYOffset;
                        LiquidEffect.CBPerDraw.IsDirty = true;

                        LiquidEffect.Apply();
                        chunk.DrawLiquidFaces();
                    }
                }
            }
        }

        public void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _liquid.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public void LoadContent()
        {
            LiquidEffect = new HLSLLiquid(_terra.Game, @"PlugIns/Effects/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration);
            LiquidEffect.TerraTexture.Value = _terra.Terra_View;
            LiquidEffect.SamplerBackBuffer.Value = StatesRepository.GetSamplerState(Utopia.GameDXStates.DXStates.Samplers.UVWrap_MinMagMipPoint);
            LiquidEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(Utopia.GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);



            _liquid.LoadContent();
        }

        public void UnloadContent()
        {
            LiquidEffect.Dispose();

            _liquid.UnloadContent();
        }

        #region Flooding propagation management

        private void LiquidTextureAnimation()
        {
            //Start Tempo
            currentTimeTex = Stopwatch.GetTimestamp();
            timeAccumulatorTex += currentTimeTex - previousTimeTex;
            previousTimeTex = currentTimeTex;

            if (timeAccumulatorTex < FloodingSpeedTex) return;
            timeAccumulatorTex = 0;

            TextureAnimationOffset -= 0.03f;
            if (TextureAnimationOffset > 1) TextureAnimationOffset = 0;
            if (TextureAnimationOffset < 0) TextureAnimationOffset = 1;
        }

        public void LiquidFloodingUpdate(bool processWithoutTimerWaterPools = false)
        {
            //Could put a timer here on the flowing algo !
            // TEMPO
            if (WaterPools.Count == 0)
            {
                timeAccumulator = 0;
                previousTime = Stopwatch.GetTimestamp();
                return; //No flowings !
            }

            if (!processWithoutTimerWaterPools)
            {
                //Start Tempo
                currentTime = Stopwatch.GetTimestamp();
                timeAccumulator += currentTime - previousTime;
                previousTime = currentTime;

                if (timeAccumulator < FloodingSpeed) return;
                timeAccumulator = 0;
                // END TEMPO
            }

            WaterPool waterPool;

            for (int wPoolId = 0; wPoolId < WaterPools.Count; wPoolId++)
            {
                waterPool = WaterPools[wPoolId];

                if (processWithoutTimerWaterPools && waterPool.GlobalState != WaterPoolState.WithoutTimer) continue;

                FloodingData floodingCubePosition;
                int NbrBlockToProcess;

                NbrBlockToProcess = Math.Min(waterPool.FloodData.Count, 100);

                //Console.WriteLine(waterPool.ToString());

                waterPool.ProcessState = WaterPoolProcessState.QueueProcessINIT;

            WithoutTimeLabel:

                for (int i = 0; i < NbrBlockToProcess; i++)
                {
                    floodingCubePosition = waterPool.FloodData.Dequeue();
                    if (floodingCubePosition.FloodingPower > 0 || waterPool.DryingPool)
                    {
                        PropagateCubeFlow(_terra.World.Landscape, floodingCubePosition, waterPool);
                    }
                }

                if (waterPool.FloodData.Count > 0 && processWithoutTimerWaterPools) goto WithoutTimeLabel;

                if (waterPool.FloodData.Count == 0)
                    waterPool.SendSourcesInQueue(_terra.World.Landscape);
            }

            //Clean UP WaterPools
            WaterPools.RemoveAll(x => x.FloodData.Count == 0);

        }

        private void PropagateCubeFlow(LandScape landscape, FloodingData cubeFloodData, WaterPool waterPool)
        {
            TerraCube workingCube;
            workingCube = landscape.Cubes[landscape.Index(cubeFloodData.CubeLocation.X, cubeFloodData.CubeLocation.Y, cubeFloodData.CubeLocation.Z)];
            if (waterPool.DryingPool == false)
            {
                _liquid.Propagate(ref workingCube, cubeFloodData, waterPool);
            }
            else
            {
                _liquid.DryUp(ref workingCube, cubeFloodData, waterPool);
            }
        }

        #endregion

    }
}
