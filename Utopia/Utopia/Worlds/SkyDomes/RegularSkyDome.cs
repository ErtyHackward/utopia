using System;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.GameClocks;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D;
using Utopia.Worlds.Weather;
using Ninject;
using Utopia.Resources.Effects.Skydome;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Entities.Managers;
using Utopia.Worlds.SkyDomes.SharedComp;

namespace Utopia.Worlds.SkyDomes
{
    public class RegularSkyDome : SkyDome
    {
        #region Private variables
        // Dome Mesh building Variables
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private IDrawableComponent _skyStars;
        private IDrawableComponent _clouds;
        

        private VertexBuffer<VertexPositionNormalTexture> _domeVertexBuffer;
        private VertexBuffer<VertexPositionTexture> _moonVertexBuffer;
        private IndexBuffer<ushort> _domeIndexBuffer, _moonIndexBuffer;
        private VertexPositionNormalTexture[] _domeVerts;
        ushort[] _domeIb;
        private int DomeN;
        private int DVSize;
        private int DISize;

        // Moon Mesh building Variables
        private VertexPositionTexture[] _moonVerts;
        private ushort[] _moonIb;

        //Drawing Objects
        private ShaderResourceView _skyTex_View, _moonTex_View, _glowTex_View;
        private HLSLPlanetSkyDome _skyDomeEffect;
        private HLSLVertexPositionTexture _posiTextureEffect;
        private int cloudDrawIndex;
        #endregion

        #region Public properties/Variables
        #endregion

        [Inject]
        public IPlayerManager PlayerManager { get; set; }

        /// <summary>
        /// Regular Skydome loading
        /// </summary>
        public RegularSkyDome(D3DEngine d3dEngine, CameraManager<ICameraFocused> camManager, WorldFocusManager worldFocusManager, IClock clock, IWeather weather, [Named("Stars")] IDrawableComponent skyStars, [Named("Clouds")] IDrawableComponent clouds)
            : base(d3dEngine, clock, weather)
        {
            this.IsDefferedLoadContent = true;

            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _clock = clock;
            _skyStars = skyStars;
            _clouds = clouds;

            this.DrawOrders.UpdateIndex(0, 40);
            cloudDrawIndex = this.DrawOrders.AddIndex(989, "Clouds");
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
            _skyDomeEffect = new HLSLPlanetSkyDome(_d3dEngine.Device, ClientSettings.EffectPack + @"SkyDome\PlanetSkyDome.hlsl", VertexPosition.VertexDeclaration);
            _posiTextureEffect = new HLSLVertexPositionTexture(_d3dEngine.Device);

            //Init Textures
            _skyTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\skyText.png");
            _moonTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\moon.png");
            _glowTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, ClientSettings.TexturePack + @"SkyDome\moonglow.png");

            _skyDomeEffect.TerraTexture.Value = _skyTex_View;
            _skyDomeEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UWrapVClamp_MinMagMipLinear);

            _posiTextureEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear);

            base.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {
            _skyStars.LoadContent(context);
            _clouds.LoadContent(context);
        }

        public override void BeforeDispose()
        {
            ((Clouds)_clouds).BeforeDispose();
            _clouds.Dispose();

            _posiTextureEffect.Dispose();
            _skyDomeEffect.Dispose();

            _domeVertexBuffer.Dispose();
            _domeIndexBuffer.Dispose();

            _moonVertexBuffer.Dispose();
            _moonIndexBuffer.Dispose();

            _skyTex_View.Dispose();
            _moonTex_View.Dispose();
            _glowTex_View.Dispose();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            _clouds.FTSUpdate(timeSpend);
            RefreshSunColor();
            base.FTSUpdate(timeSpend);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _clouds.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
            base.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(DeviceContext context, int index)
        {
            if (index == cloudDrawIndex)
            {
                if(PlayerManager.IsHeadInsideWater == false) _clouds.Draw(context, index);
            }
            else
            {
                DrawingDome(context);
                DrawingMoon(context);
                if (PlayerManager.IsHeadInsideWater == false) _skyStars.Draw(context, index);
            }
        }
        #endregion

        #region Private Methods
        private void RefreshSunColor()
        {
            float SunColorBase = _clock.ClockTime.SmartTimeInterpolation();

            base._sunColor.Red = SunColorBase;
            base._sunColor.Green = SunColorBase;
            base._sunColor.Blue = SunColorBase;
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

            ushort BottomIndexMM, BottomIndexMP, BottomIndexPP, BottomIndexPM;
            //Closing the Dome Bottom !
            BottomIndexMM = (ushort)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = -scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = -scale;
            DomeIndex++;

            BottomIndexMP = (ushort)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = -scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = +scale;
            DomeIndex++;

            BottomIndexPM = (ushort)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = +scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = -scale;
            DomeIndex++;

            BottomIndexPP = (ushort)DomeIndex;
            _domeVerts[DomeIndex] = new VertexPositionNormalTexture();
            _domeVerts[DomeIndex].Position.X = +scale;
            _domeVerts[DomeIndex].Position.Y = minY;
            _domeVerts[DomeIndex].Position.Z = +scale;

            // Fill index buffer
            _domeIb = new ushort[(DISize * 3) + 6];
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    _domeIb[index++] = (ushort)(i * Latitude + j);
                    _domeIb[index++] = (ushort)((i + 1) * Latitude + j);
                    _domeIb[index++] = (ushort)((i + 1) * Latitude + j + 1);

                    _domeIb[index++] = (ushort)((i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (ushort)(i * Latitude + j + 1);
                    _domeIb[index++] = (ushort)(i * Latitude + j);
                }
            }
            ushort Offset = (ushort)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    _domeIb[index++] = (ushort)(Offset + i * Latitude + j);
                    _domeIb[index++] = (ushort)(Offset + (i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (ushort)(Offset + (i + 1) * Latitude + j);

                    _domeIb[index++] = (ushort)(Offset + i * Latitude + j + 1);
                    _domeIb[index++] = (ushort)(Offset + (i + 1) * Latitude + j + 1);
                    _domeIb[index++] = (ushort)(Offset + i * Latitude + j);
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

        private void DrawingDome(DeviceContext context)
        {
            Matrix World = Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.ValueInterp.X, -(float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Y, (float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            //Set States.
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            _skyDomeEffect.Begin(context);
            _skyDomeEffect.CBPerDraw.Values.ViewProj = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _skyDomeEffect.CBPerDraw.Values.CameraWorldPosition = _camManager.ActiveCamera.WorldPosition.ValueInterp.AsVector3();
            _skyDomeEffect.CBPerDraw.Values.time = _clock.ClockTime.ClockTimeNormalized;
            _skyDomeEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _skyDomeEffect.CBPerDraw.Values.LightDirection = LightDirection;
            _skyDomeEffect.CBPerDraw.Values.HeadUnderWater = PlayerManager.IsHeadInsideWater ? 1.0f : 0.0f;
            _skyDomeEffect.CBPerDraw.IsDirty = true;
            _skyDomeEffect.Apply(context);

            _domeVertexBuffer.SetToDevice(context,0);
            _domeIndexBuffer.SetToDevice(context,0);

            context.DrawIndexed(_domeIb.Length, 0, 0);
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
            _moonIb = new ushort[] { 0, 2, 1, 2, 0, 3 };
        }

        private void DrawingMoon(DeviceContext context)
        {
            float alpha = (float)Math.Abs(Math.Sin(_clock.ClockTime.ClockTimeNormalized + (float)Math.PI / 2.0f));
            //Set States.
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled);

            Matrix World = Matrix.Scaling(2f, 2f, 2f) * Matrix.RotationX(_clock.ClockTime.ClockTimeNormalized + (float)Math.PI / 2.0f) *
                            Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                            Matrix.Translation(LightDirection.X * 1900, LightDirection.Y * 1900, LightDirection.Z * 1900) *
                            Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.ValueInterp.X, -(float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Y, (float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            _posiTextureEffect.Begin(context);
            _posiTextureEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
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
            _posiTextureEffect.Apply(context);

            _moonIndexBuffer.SetToDevice(context,0);
            _moonVertexBuffer.SetToDevice(context,0);
            context.DrawIndexed(_moonIb.Length, 0, 0);

            //Draw moonLight
            World = Matrix.Scaling(6f, 6f, 6f) *
                    Matrix.RotationX(_clock.ClockTime.ClockTimeNormalized + (float)Math.PI / 2.0f) *
                    Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                    Matrix.Translation(LightDirection.X * 1700, LightDirection.Y * 1700, LightDirection.Z * 1700) *
                    Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.ValueInterp.X, -(float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Y, (float)_camManager.ActiveCamera.WorldPosition.ValueInterp.Z);

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref World, ref World);

            _posiTextureEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _posiTextureEffect.CBPerDraw.IsDirty = true;

            _posiTextureEffect.DiffuseTexture.Value = _glowTex_View;
            _posiTextureEffect.DiffuseTexture.IsDirty = true;

            _posiTextureEffect.Apply(context);

            context.DrawIndexed(_moonIb.Length, 0, 0);
        }

        private void BuffersToDevice()
        {
            //Copy Dome to graphic buffers
            //SkyDome
            _domeIndexBuffer = new IndexBuffer<ushort>(_d3dEngine.Device, _domeIb.Length, "_domeIndexBuffer");
            _domeIndexBuffer.SetData(_d3dEngine.ImmediateContext, _domeIb);
            _domeVertexBuffer = new VertexBuffer<VertexPositionNormalTexture>(_d3dEngine.Device, _domeVerts.Length, PrimitiveTopology.TriangleList, "_domeVertexBuffer");
            _domeVertexBuffer.SetData(_d3dEngine.ImmediateContext,_domeVerts);

            //Moon
            _moonVertexBuffer = new VertexBuffer<VertexPositionTexture>(_d3dEngine.Device, _moonVerts.Length,  PrimitiveTopology.TriangleList, "_moonVertexBuffer");
            _moonVertexBuffer.SetData(_d3dEngine.ImmediateContext,_moonVerts);
            _moonIndexBuffer = new IndexBuffer<ushort>(_d3dEngine.Device, _moonIb.Length, "_moonIndexBuffer");
            _moonIndexBuffer.SetData(_d3dEngine.ImmediateContext,_moonIb);
        }
        #endregion
    }
}
