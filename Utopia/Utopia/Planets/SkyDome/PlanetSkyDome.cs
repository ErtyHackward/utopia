using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using SharpDX.Direct3D11;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects;
using S33M3Engines.StatesManager;
using Utopia.GameClock;
using S33M3Engines.Struct;
using S33M3Engines.D3D.Effects.Basics;
using UtopiaContent.Effects.Skydome;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Math;
using Utopia.Planets.Skybox;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;

namespace Utopia.Planets.SkyDome
{
    public class PlanetSkyDome : GameComponent
    {
        #region Private Variables
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        // Dome Mesh building Variables
        private VertexBuffer<VertexPositionNormalTexture> _domeVertexBuffer;
        private VertexBuffer<VertexPositionTexture> _moonVertexBuffer;
        private IndexBuffer<short> _domeIndexBuffer, _moonIndexBuffer;
        private VertexPositionNormalTexture[] _domeVerts;
        short[] _domeIb;
        private int DomeN;
        private int DVSize;
        private int DISize;
        private Clock _clock;

        // Moon Mesh building Variables
        private VertexPositionTexture[] _moonVerts;
        private short[] _moonIb;

        //Dome States variables
        private float _fPhi = 0.0f;
        FTSValue<Vector3> _lightDirection;

        //Drawing Objects
        private ShaderResourceView _skyTex_View, _moonTex_View, _glowTex_View;
        private HLSLPlanetSkyDome _skyDomeEffect;
        private HLSLVertexPositionTexture _posiTextureEffect;

        private Stars _stars;
        #endregion

        #region Public Properties
        public Vector3 LightDirection { get { return _lightDirection.ActualValue; } }
        public ShaderResourceView GlowTex_View { get { return _glowTex_View; } }
        public ShaderResourceView MoonTex_View { get { return _moonTex_View; } }
        public ShaderResourceView SkyTex_View { get { return _skyTex_View; } }
        #endregion

        public PlanetSkyDome(D3DEngine d3dEngine, CameraManager camManager, WorldFocusManager worldFocusManager , Clock clock)
        {
            _worldFocusManager = worldFocusManager;
            _camManager = camManager;
            _clock = clock;
            _d3dEngine = d3dEngine;
        }

        public override void LoadContent()
        {
            //Generate manualy created models
            GenerateDome();
            GenerateMoon();
            BuffersToDevice(); //Create Buffers

            //Init effects
            _skyDomeEffect = new HLSLPlanetSkyDome(_d3dEngine, @"Effects\SkyDome\PlanetSkyDome.hlsl", VertexPosition.VertexDeclaration);
            _posiTextureEffect = new HLSLVertexPositionTexture(_d3dEngine, @"D3D\Effects\Basics\VertexPositionTexture.hlsl", VertexPositionTexture.VertexDeclaration);

            //Init Textures
            _skyTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, @"Textures\SkyDome\skyText.png");
            _moonTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, @"Textures\SkyDome\moon.png");
            _glowTex_View = ShaderResourceView.FromFile(_d3dEngine.Device, @"Textures\SkyDome\moonglow.png");

            _stars.LoadContent();


            _skyDomeEffect.TerraTexture.Value = _skyTex_View;
            _skyDomeEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UWrapVClamp_MinMagMipLinear);

            _posiTextureEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _stars = new Stars(_d3dEngine, _camManager,_clock);
            _stars.Initialize();

            _lightDirection = new FTSValue<Vector3>() { Value = new Vector3(100.0f, 100.0f, 100.0f) };

            base.Initialize();
        }

        #region Updates
        public override void Update(ref GameTime TimeSpend)
        {
            _lightDirection.BackUpValue();

            _lightDirection.Value = this.GetDirection();
            _lightDirection.Value.Normalize();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _lightDirection.ValueInterp = Vector3.Lerp(_lightDirection.ValuePrev, _lightDirection.Value, interpolation_ld);
            _lightDirection.ValueInterp.Normalize();
        }

        //Get sunray direction
        private Vector3 GetDirection()
        {
            float y = (float)Math.Cos(_clock.ClockTime);
            float x = (float)(Math.Sin(_clock.ClockTime) * Math.Cos(this._fPhi));
            float z = (float)(Math.Sin(_clock.ClockTime) * Math.Sin(this._fPhi));

            return new Vector3(x, y, z);
        }
        #endregion

        #region Drawing Dome + Moon + MoonLight
        public override void DrawDepth0()
        {
            if (_camManager.ActiveCamera.WorldPosition.Y <= 300)
            {
                DrawingDome();
            }

            _stars.DrawDepth0();

            if (_camManager.ActiveCamera.WorldPosition.Y <= 300)
            {
                DrawingMoon();
            }

        }

        private void DrawingDome()
        {
            Matrix World = Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterOnFocus(ref World, ref World);

            //Set States.
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullFront, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _skyDomeEffect.Begin();
            _skyDomeEffect.CBPerDraw.Values.ViewProj = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _skyDomeEffect.CBPerDraw.Values.CameraWorldPosition = _camManager.ActiveCamera.WorldPosition.AsVector3();
            _skyDomeEffect.CBPerDraw.Values.time = _clock.ClockTimeNormalized;
            _skyDomeEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _skyDomeEffect.CBPerDraw.Values.LightDirection = LightDirection;
            _skyDomeEffect.CBPerDraw.IsDirty = true;
            _skyDomeEffect.Apply();

            _domeVertexBuffer.SetToDevice(0);
            _domeIndexBuffer.SetToDevice(0);

            _d3dEngine.Context.DrawIndexed(_domeIb.Length, 0, 0);
        }

        private void DrawingMoon()
        {
            float alpha = (float)Math.Abs(Math.Sin(_clock.ClockTime + (float)Math.PI / 2.0f));
            //Set States.
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled);

            Matrix World = Matrix.Scaling(2f, 2f, 2f) * Matrix.RotationX(_clock.ClockTime + (float)Math.PI / 2.0f) *
                            Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                            Matrix.Translation(LightDirection.X * 1900, LightDirection.Y * 1900, LightDirection.Z * 1900) *
                            Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterOnFocus(ref World, ref World);

            _posiTextureEffect.Begin();
            _posiTextureEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _posiTextureEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
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
                    Matrix.RotationX(_clock.ClockTime + (float)Math.PI / 2.0f) *
                    Matrix.RotationY(-_fPhi + (float)Math.PI / 2.0f) *
                    Matrix.Translation(LightDirection.X * 1700, LightDirection.Y * 1700, LightDirection.Z * 1700) *
                    Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X, -(float)_camManager.ActiveCamera.WorldPosition.Y, (float)_camManager.ActiveCamera.WorldPosition.Z);

            _worldFocusManager.CenterOnFocus(ref World, ref World);

            _posiTextureEffect.CBPerDraw.Values.World = Matrix.Transpose(World);
            _posiTextureEffect.CBPerDraw.IsDirty = true;

            _posiTextureEffect.DiffuseTexture.Value = _glowTex_View;
            _posiTextureEffect.DiffuseTexture.IsDirty = true;

            _posiTextureEffect.Apply();

            _d3dEngine.Context.DrawIndexed(_moonIb.Length, 0, 0);

        }

        #endregion

        #region Private Methods
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

            ////Compute Normals
            //for (int i = 0; i < _domeIb.Length / 3; i++)
            //{
            //    Vector3 firstvec = _domeVerts[ib[i * 3 + 1]].Position - _domeVerts[ib[i * 3]].Position;
            //    Vector3 secondvec = _domeVerts[ib[i * 3]].Position - _domeVerts[ib[i * 3 + 2]].Position;
            //    Vector3 normal = Vector3.Cross(firstvec, secondvec);
            //    normal.Normalize();
            //    _domeVerts[ib[i * 3]].Normal += normal;
            //    _domeVerts[ib[i * 3 + 1]].Normal += normal;
            //    _domeVerts[ib[i * 3 + 2]].Normal += normal;
            //}
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

        private void BuffersToDevice()
        {
            //Copy Dome to graphic buffers
            //SkyDome
            _domeIndexBuffer = new IndexBuffer<short>(_d3dEngine, _domeIb.Length, SharpDX.DXGI.Format.R16_UInt);
            _domeIndexBuffer.SetData(_domeIb);
            _domeVertexBuffer = new VertexBuffer<VertexPositionNormalTexture>(_d3dEngine, _domeVerts.Length, VertexPositionNormalTexture.VertexDeclaration, PrimitiveTopology.TriangleList);
            _domeVertexBuffer.SetData(_domeVerts);

            //Moon
            _moonVertexBuffer = new VertexBuffer<VertexPositionTexture>(_d3dEngine, _moonVerts.Length, VertexPositionTexture.VertexDeclaration, PrimitiveTopology.TriangleList);
            _moonVertexBuffer.SetData(_moonVerts);
            _moonIndexBuffer = new IndexBuffer<short>(_d3dEngine, _moonIb.Length, SharpDX.DXGI.Format.R16_UInt);
            _moonIndexBuffer.SetData(_moonIb);
        }
        #endregion

        public override void UnloadContent()
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

            _stars.UnloadContent();

            base.UnloadContent();
        }
    }

}

