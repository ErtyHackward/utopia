using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Worlds.GameClocks;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Struct;
using SharpDX.Direct3D11;
using UtopiaContent.Effects.Skydome;
using S33M3Engines.D3D.Effects.Basics;
using SharpDX;
using S33M3Engines.Shared.Math;
using SharpDX.Direct3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Maths;
using Utopia.Worlds.SkyDomes.SharedComp;
using Utopia.Worlds.Weather;
using Utopia.Shared.World;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using Ninject;
using Utopia.Settings;

namespace Utopia.Worlds.SkyDomes
{
    public class RegularSkyDome : SkyDome
    {
        #region Private variables
        // Dome Mesh building Variables
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private IDrawableComponent _skyStars;
        private IDrawableComponent _clouds;

        private VertexBuffer<VertexPositionNormalTexture> _domeVertexBuffer;
        private VertexBuffer<VertexPositionTexture> _moonVertexBuffer;
        private IndexBuffer<short> _domeIndexBuffer, _moonIndexBuffer;
        private VertexPositionNormalTexture[] _domeVerts;
        short[] _domeIb;
        private int DomeN;
        private int DVSize;
        private int DISize;

        // Moon Mesh building Variables
        private VertexPositionTexture[] _moonVerts;
        private short[] _moonIb;

        //Drawing Objects
        private ShaderResourceView _skyTex_View, _moonTex_View, _glowTex_View;
        private HLSLPlanetSkyDome _skyDomeEffect;
        private HLSLVertexPositionTexture _posiTextureEffect;

        #endregion

        #region Public properties/Variables
        #endregion

        /// <summary>
        /// Regular Skydome loading
        /// </summary>
        public RegularSkyDome(D3DEngine d3dEngine, CameraManager camManager, WorldFocusManager worldFocusManager, IClock clock, IWeather weather, [Named("Stars")] IDrawableComponent skyStars, [Named("Clouds")] IDrawableComponent clouds)
            : base(d3dEngine, clock, weather)
        {
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _clock = clock;
            _skyStars = skyStars;
            _clouds = clouds;
        }

        #region Public Methods
        public override void Initialize()
        {
            //Initializate the Stars displayer
            _skyStars.Initialize();

            //initialize Clouds displayer
            _clouds.Initialize();

            //Generate manualy created models
            GenerateDome();
            GenerateMoon();
            BuffersToDevice(); //Create Buffers

            //Init effects
            _skyDomeEffect = new HLSLPlanetSkyDome(_d3dEngine, ClientSettings.EffectPack + @"SkyDome\PlanetSkyDome.hlsl", VertexPosition.VertexDeclaration);
            _posiTextureEffect = new HLSLVertexPositionTexture(_d3dEngine, @"D3D\Effects\Basics\VertexPositionTexture.hlsl", VertexPositionTexture.VertexDeclaration);

            //Init Textures
            _skyTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\skyText.png");
            _moonTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\moon.png");
            _glowTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\moonglow.png");

            _skyDomeEffect.TerraTexture.Value = _skyTex_View;
            _skyDomeEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UWrapVClamp_MinMagMipLinear);

            _posiTextureEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);

            base.Initialize();
        }

        public override void Dispose()
        {
            _posiTextureEffect.Dispose();
            _skyDomeEffect.Dispose();

            _domeVertexBuffer.Dispose();
            _domeIndexBuffer.Dispose();

            _moonVertexBuffer.Dispose();
            _moonIndexBuffer.Dispose();

            _skyTex_View.Dispose();
            _moonTex_View.Dispose();
            _glowTex_View.Dispose();

            base.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _clouds.Update(ref TimeSpend);
            RefreshSunColor();
            base.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _clouds.Interpolation(ref interpolation_hd, ref interpolation_ld);
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw(int Index)
        {
            DrawingDome();
            DrawingMoon();
            _skyStars.Draw(Index);
            _clouds.Draw(Index);
        }
        #endregion

        #region Private Methods
        private void RefreshSunColor()
        {
            float SunColorBase;
            if (_clock.ClockTime.ClockTimeNormalized <= 0.2083944 || _clock.ClockTime.ClockTimeNormalized > 0.9583824) // Between 23h00 and 05h00 => Dark night
            {
                SunColorBase = 0.05f;
            }
            else
            {
                if (_clock.ClockTime.ClockTimeNormalized > 0.2083944 && _clock.ClockTime.ClockTimeNormalized <= 0.4166951) // Between 05h00 and 10h00 => Go to Full Day
                {
                    SunColorBase = MathHelper.FullLerp(0.05f, 1, 0.2083944, 0.4166951, _clock.ClockTime.ClockTimeNormalized);
                }
                else
                {
                    if (_clock.ClockTime.ClockTimeNormalized > 0.4166951 && _clock.ClockTime.ClockTimeNormalized <= 0.6666929) // Between 10h00 and 16h00 => Full Day
                    {
                        SunColorBase = 1f;
                    }
                    else
                    {
                        SunColorBase = MathHelper.FullLerp(1, 0.05f, 0.6666929, 0.9583824, _clock.ClockTime.ClockTimeNormalized); //Go to Full night
                    }
                }
            }

            base._sunColor.X = SunColorBase;
            base._sunColor.Y = SunColorBase;
            base._sunColor.Z = SunColorBase;
        }

        private void GenerateDome()
        {
            DomeN = 32;
            int scale = 2000; // (Terran.TerraWorld.ChunkGridSize * Terran.TerraWorld.Chunksize) / 2;
            int Latitude = DomeN / 2;
            int Longitude = DomeN;
            DVSize = Longitude * Latitude;
            DISize = (Longitude - 1) * (Latitude - 1) * 2;
            DVSize *= 2;
            DISize *= 2;

            _domeVerts = new VertexPositionNormalTexture[DVSize + 4];

            float minY = float.MaxValue;

            // Fill Vertex Buffer
            int DomeIndex = 0;
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)Longitude - 1.0f)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = MathHelper.Pi * j / (Latitude - 1);

                    _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
                    _domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY)) * scale;
                    _domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ) * scale;
                    _domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY)) * scale;

                    if (_domeVerts[DomeIndex].Position.Y < minY) minY = _domeVerts[DomeIndex].Position.Y;

                    _domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    _domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }

            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(Longitude - 1)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = (MathHelper.Pi * 2.0) - (MathHelper.Pi * j / (Latitude - 1));

                    _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
                    _domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY)) * scale;
                    _domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ) * scale;
                    _domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY)) * scale;

                    if (_domeVerts[DomeIndex].Position.Y < minY) minY = _domeVerts[DomeIndex].Position.Y;

                    _domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    _domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }

            short BottomIndexMM, BottomIndexMP, BottomIndexPP, BottomIndexPM;
            //Closing the Dome Bottom !
            BottomIndexMM = (short)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = -scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = -scale;
            DomeIndex++;

            BottomIndexMP = (short)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = -scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = +scale;
            DomeIndex++;

            BottomIndexPM = (short)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = +scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = -scale;
            DomeIndex++;

            BottomIndexPP = (short)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = +scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = +scale;

            // Fill index buffer
            _domeIb = new short[(DISize * 3) + 6];
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    _domeIb[index++] = (short)(i * Latitude + j);
                    _domeIb[index++] = (short)((i + 1) * Latitude + j);
                    _domeIb[index++] = (short)((i + 1) * Latitude + j + 1);

                    _domeIb[index++] = (short)((i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (short)(i * Latitude + j + 1);
                    _domeIb[index++] = (short)(i * Latitude + j);
                }
            }
            short Offset = (short)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    _domeIb[index++] = (short)(Offset + i * Latitude + j);
                    _domeIb[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (short)(Offset + (i + 1) * Latitude + j);

                    _domeIb[index++] = (short)(Offset + i * Latitude + j + 1);
                    _domeIb[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (short)(Offset + i * Latitude + j);
                }
            }

            //Closing the Dome Bottom !
            _domeIb[index++] = (BottomIndexMM);
            _domeIb[index++] = (BottomIndexMP);
            _domeIb[index++] = (BottomIndexPM);
            _domeIb[index++] = (BottomIndexPM);
            _domeIb[index++] = (BottomIndexMP);
            _domeIb[index++] = (BottomIndexPP);

        }

        private void DrawingDome()
        {
            Matrix World = Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            //Set States.
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullFront, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _skyDomeEffect.Begin();
            _skyDomeEffect.CBPerDraw.Values.ViewProj = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _skyDomeEffect.CBPerDraw.Values.CameraWorldPosition = _camManager.ActiveCamera.WorldPosition.AsVector3();
            _skyDomeEffect.CBPerDraw.Values.time = _clock.ClockTime.ClockTimeNormalized;
            _skyDomeEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _skyDomeEffect.CBPerDraw.Values.LightDirection = LightDirection;
            _skyDomeEffect.CBPerDraw.IsDirty = true;
            _skyDomeEffect.Apply();

            _domeVertexBuffer.SetToDevice(0);
            _domeIndexBuffer.SetToDevice(0);

            _d3dEngine.Context.DrawIndexed(_domeIb.Length, 0, 0);
        }

        private void GenerateMoon()
        {
            float MoonScale = 100;
            _moonVerts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(MoonScale,-MoonScale,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-MoonScale,-MoonScale,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(-MoonScale,MoonScale,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(MoonScale,MoonScale,0),
                                new Vector2(1,0))
                        };

            _moonIb = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        private void DrawingMoon()
        {
            float alpha = (float)Math.Abs(Math.Sin(_clock.ClockTime.Time + (float)Math.PI / 2.0f));
            //Set States.
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled);

            Matrix World = Matrix.Scaling(2f, 2f, 2f) * Matrix.RotationX(_clock.ClockTime.Time + (float)Math.PI / 2.0f) *
                            Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                            Matrix.Translation(LightDirection.X * 1900, LightDirection.Y * 1900, LightDirection.Z * 1900) *
                            Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            _posiTextureEffect.Begin();
            _posiTextureEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _posiTextureEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View_focused);
            _posiTextureEffect.CBPerFrame.IsDirty = true;
            _posiTextureEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            if (LightDirection.Y > 0)
            {
                _posiTextureEffect.CBPerFrame.Values.Alpha = alpha;
            }
            else
            {
                _posiTextureEffect.CBPerFrame.Values.Alpha = 0;
            }
            _posiTextureEffect.CBPerDraw.IsDirty = true;
            _posiTextureEffect.DiffuseTexture.Value = _moonTex_View;
            _posiTextureEffect.DiffuseTexture.IsDirty = true;
            _posiTextureEffect.Apply();

            _moonIndexBuffer.SetToDevice(0);
            _moonVertexBuffer.SetToDevice(0);
            _d3dEngine.Context.DrawIndexed(_moonIb.Length, 0, 0);

            //Draw moonLight
            World = Matrix.Scaling(6f, 6f, 6f) *
                    Matrix.RotationX(_clock.ClockTime.Time + (float)Math.PI / 2.0f) *
                    Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                    Matrix.Translation(LightDirection.X * 1700, LightDirection.Y * 1700, LightDirection.Z * 1700) *
                    Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            _posiTextureEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _posiTextureEffect.CBPerDraw.IsDirty = true;

            _posiTextureEffect.DiffuseTexture.Value = _glowTex_View;
            _posiTextureEffect.DiffuseTexture.IsDirty = true;

            _posiTextureEffect.Apply();

            _d3dEngine.Context.DrawIndexed(_moonIb.Length, 0, 0);

        }

        private void BuffersToDevice()
        {
            //Copy Dome to graphic buffers
            //SkyDome
            _domeIndexBuffer = new IndexBuffer<short>(_d3dEngine, _domeIb.Length, SharpDX.DXGI.Format.R16_UInt, "_domeIndexBuffer");
            _domeIndexBuffer.SetData(_domeIb);
            _domeVertexBuffer = new VertexBuffer<VertexPositionNormalTexture>(_d3dEngine, _domeVerts.Length, VertexPositionNormalTexture.VertexDeclaration, PrimitiveTopology.TriangleList, "_domeVertexBuffer");
            _domeVertexBuffer.SetData(_domeVerts);

            //Moon
            _moonVertexBuffer = new VertexBuffer<VertexPositionTexture>(_d3dEngine, _moonVerts.Length, VertexPositionTexture.VertexDeclaration, PrimitiveTopology.TriangleList, "_moonVertexBuffer");
            _moonVertexBuffer.SetData(_moonVerts);
            _moonIndexBuffer = new IndexBuffer<short>(_d3dEngine, _moonIb.Length, SharpDX.DXGI.Format.R16_UInt, "_moonIndexBuffer");
            _moonIndexBuffer.SetData(_moonIb);
        }
        #endregion
    }
}
