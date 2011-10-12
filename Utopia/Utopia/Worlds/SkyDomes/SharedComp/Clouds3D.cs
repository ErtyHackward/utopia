using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using S33M3Engines.Cameras;
using Utopia.Worlds.Weather;
using Utopia.Shared.Chunks;
using Utopia.Settings;
using SharpDX.Direct3D11;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Math.Noises;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.WorldFocus;
using S33M3Engines.Struct;
using Utopia.Worlds.GameClocks;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class Clouds3D : DrawableGameComponent
    {
        #region Private Variables
        private D3DEngine _d3dEngine;
        private VisualWorldParameters _worldParam;
        private IWeather _weather;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private IClock _worldclock;

        private int _cloudMap_size;
        private float _cloud_size = 40;
        private float _cloud_Height = 5;
        private float _brightness = 0.9f;
        private float _cloudLayerHeight = 140;

        private SimplexNoise _noise;

        private IndexBuffer<ushort> _cloudIB;
        private VertexBuffer<VertexPositionColor> _cloudVB;
        private ushort[] _indices;
        private VertexPositionColor[] _vertices;
        private int _nbrIndices, _nbrVertices;
        private int _maxNbrIndices, _maxNbrVertices;
        private HLSLVertexPositionColor _effect;
        private FTSValue<Vector2> _cloud_MapOffset;
        private Color _topFace, _side1Face, _side2Face, _bottomFace;

        private VertexPositionColor[] _faces;

        private int _cloudMapSize;
        private bool[] _cloudMap;
        #endregion

        #region Public properties
        #endregion

        public Clouds3D(D3DEngine d3dEngine, CameraManager camManager, IWeather weather, VisualWorldParameters worldParam, WorldFocusManager worldFocusManager, IClock worldclock)
        {
            _d3dEngine = d3dEngine;
            _worldParam = worldParam;
            _weather = weather;
            _camManager = camManager;
            _worldclock = worldclock;
            _worldFocusManager = worldFocusManager;
            _cloud_MapOffset = new FTSValue<Vector2>();
            _cloudMap_size = (int)(worldParam.WorldVisibleSize.X / _cloud_size * 4);

            //Create a virtual Cloud map of 1024 * 1024 size !
            _cloudMapSize = 1024;
            _cloudMap = new bool[_cloudMapSize * _cloudMapSize];
        }

        #region Public methods
        public override void Initialize()
        {
            _noise = new SimplexNoise(new Random());
            _noise.SetParameters(0.075, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
            _effect = new HLSLVertexPositionColor(_d3dEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);

            _maxNbrIndices = 15000;
            _maxNbrVertices = 10000;
            _indices = new ushort[_maxNbrIndices];
            _vertices = new VertexPositionColor[_maxNbrVertices];
            _nbrIndices = 0;
            _nbrVertices = 0;

            _topFace = new Color(_brightness * 240, _brightness * 240, _brightness * 255, 200);
            _side1Face = new Color(_brightness * 230, _brightness * 230, _brightness * 255, 200);
            _side2Face = new Color(_brightness * 220, _brightness * 220, _brightness * 245, 200);
            _bottomFace = new Color(_brightness * 205, _brightness * 205, _brightness * 230, 200);

            _faces = new VertexPositionColor[4];

            CreateCloudMap();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _cloud_MapOffset.BackUpValue();

            _cloud_MapOffset.Value.X += TimeSpend.ElapsedGameTimeInS_LD * _weather.Wind.WindFlow.X * 7;
            _cloud_MapOffset.Value.Y += TimeSpend.ElapsedGameTimeInS_LD * _weather.Wind.WindFlow.Z * 7;
            //_weather.Wind.WindFlow.X
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            Vector2.Lerp(ref _cloud_MapOffset.ValuePrev, ref _cloud_MapOffset.Value, interpolation_ld, out _cloud_MapOffset.ValueInterp);
        }

        public override void Draw(int Index)
        {

            Vector2 m_camera_pos = new Vector2((float)_camManager.ActiveCamera.WorldPosition.X, (float)_camManager.ActiveCamera.WorldPosition.Z); //Position de la caméra en X et Z, sans la composante Y

            Vector2 CloudsMapOffset = _cloud_MapOffset.ValueInterp - m_camera_pos;                      //Speed * Time = Distance
            Vector2 CloudsMapOffsetWithCamera = -(CloudsMapOffset - m_camera_pos); //Je retire la position de ma caméra, pour compenser le mouvement de la caméra
            Location2<int> center_of_drawing_in_noise_i = new Location2<int>((int)(CloudsMapOffsetWithCamera.X / _cloud_size), (int)(CloudsMapOffsetWithCamera.Y / _cloud_size));
            Vector2 world_center_of_drawing_in_noise_f = new Vector2(center_of_drawing_in_noise_i.X * _cloud_size, center_of_drawing_in_noise_i.Z * _cloud_size) + CloudsMapOffset;

            int _cloudMapXIndex, _cloudMapZIndex;
            _nbrIndices = 0;
            _nbrVertices = 0;

            _brightness = _worldclock.ClockTime.SmartTimeInterpolation(0.2f);

            //Recompute the color taking into accound the current day time
            _topFace.R = (byte)(_brightness * 240);
            _topFace.G = (byte)(_brightness * 240);
            _topFace.B = (byte)(_brightness * 255);

            _side1Face.R = (byte)(_brightness * 230);
            _side1Face.G = (byte)(_brightness * 230);
            _side1Face.B = (byte)(_brightness * 255);

            _side2Face.R = (byte)(_brightness * 220);
            _side2Face.G = (byte)(_brightness * 220);
            _side2Face.B = (byte)(_brightness * 245);

            _bottomFace.R = (byte)(_brightness * 205);
            _bottomFace.G = (byte)(_brightness * 205);
            _bottomFace.B = (byte)(_brightness * 230);


            for (int zi = -_cloudMap_size; zi < _cloudMap_size; zi++)
            {
                for (int xi = -_cloudMap_size; xi < _cloudMap_size; xi++)
                {


                    Location2<int> p_in_noise_i = new Location2<int>(xi + center_of_drawing_in_noise_i.X, zi + center_of_drawing_in_noise_i.Z);

                    Vector2 p0 = new Vector2(xi, zi) * _cloud_size + world_center_of_drawing_in_noise_f;

                    _cloudMapXIndex = MathHelper.Mod(p_in_noise_i.X, _cloudMapSize);
                    _cloudMapZIndex = MathHelper.Mod(p_in_noise_i.Z, _cloudMapSize);
                    if (!_cloudMap[_cloudMapXIndex + (_cloudMapZIndex*_cloudMapSize)]) continue;
                    //var noiseResult = _noise.GetNoise2DValue(p_in_noise_i.X, p_in_noise_i.Z, 2, 0.9);
                    //float noiseValue = MathHelper.FullLerp(0, 1, noiseResult);

                    //if (noiseValue > 0.3) continue;

                    float rx = _cloud_size / 2;
                    float ry = _cloud_Height;
                    float rz = _cloud_size / 2;

                    for (int i = 0; i < 6; i++)
                    {
                        switch (i)
                        {
                            case 0:	// top
                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _topFace;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _topFace;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = ry; _faces[2].Position.Z = rz; _faces[2].Color = _topFace;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = ry; _faces[3].Position.Z = -rz; _faces[3].Color = _topFace;
                                break;
                            case 1: // back
                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _side1Face;
                                _faces[1].Position.X = rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = -rz; _faces[1].Color = _side1Face;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = -rz; _faces[2].Color = _side1Face;
                                _faces[3].Position.X = -rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = -rz; _faces[3].Color = _side1Face;
                                break;
                            case 2: //right
                                _faces[0].Position.X = rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _side2Face;
                                _faces[1].Position.X = rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _side2Face;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = rz; _faces[2].Color = _side2Face;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = -rz; _faces[3].Color = _side2Face;
                                break;
                            case 3: // front
                                _faces[0].Position.X = rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = rz; _faces[0].Color = _side1Face;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _side1Face;
                                _faces[2].Position.X = -rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = rz; _faces[2].Color = _side1Face;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = rz; _faces[3].Color = _side1Face;
                                break;
                            case 4: // left
                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = rz; _faces[0].Color = _side2Face;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = -rz; _faces[1].Color = _side2Face;
                                _faces[2].Position.X = -rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = -rz; _faces[2].Color = _side2Face;
                                _faces[3].Position.X = -rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = rz; _faces[3].Color = _side2Face;
                                break;
                            case 5: // bottom
                                _faces[0].Position.X = rx; _faces[0].Position.Y = -ry; _faces[0].Position.Z = rz; _faces[0].Color = _bottomFace;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = -ry; _faces[1].Position.Z = rz; _faces[1].Color = _bottomFace;
                                _faces[2].Position.X = -rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = -rz; _faces[2].Color = _bottomFace;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = -rz; _faces[3].Color = _bottomFace;
                                break;
                        }

                        if (_nbrIndices >= _maxNbrIndices)
                        {
                            //Resize Arrays to have them bigger
                            _maxNbrIndices += 36 * 100; //Add the posibility to store 100 clouds
                            _maxNbrVertices += 24 * 100; //Add the posibility to store 100 clouds
                            Array.Resize<ushort>(ref _indices, _maxNbrIndices);
                            Array.Resize<VertexPositionColor>(ref _vertices, _maxNbrVertices);
                        }

                        //Translate the local coordinate into world coordinate
                        Vector3 pos = new Vector3(p0.X, _cloudLayerHeight, p0.Y);

                        for (int j = 0; j < 4; j++)
                        {
                            _faces[j].Position += pos;
                            _vertices[_nbrVertices] = _faces[j];
                            _nbrVertices++;
                        }

                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 2); //2 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 3); //1 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 4); //0 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 4); //0 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 1); //3 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 2); //2 + verticesCount);
                        _nbrIndices++;
                    }
                }
            }

            if (_nbrIndices == 0) return;
            //Create/Update the Buffer
            if (_cloudIB == null) _cloudIB = new IndexBuffer<ushort>(_d3dEngine, _nbrIndices, SharpDX.DXGI.Format.R16_UInt, "_cloudIB" ,10, ResourceUsage.Dynamic);
            _cloudIB.SetData(_indices, 0, _nbrIndices, true);

            if (_cloudVB == null) _cloudVB = new VertexBuffer<VertexPositionColor>(_d3dEngine, _nbrVertices, VertexPositionColor.VertexDeclaration, PrimitiveTopology.TriangleList, "_cloudVB", ResourceUsage.Dynamic, 10);
            _cloudVB.SetData(_vertices, 0 , _nbrVertices, true);

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effect.Begin();
            _effect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View_focused);
            _effect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _effect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Identity;

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref world, ref world);

            _effect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply();

            //Set the buffer to the graphical card
            _cloudIB.SetToDevice(0);
            _cloudVB.SetToDevice(0);

            //Draw
            _d3dEngine.Context.DrawIndexed(_cloudIB.IndicesCount, 0, 0);
        }

        public override void Dispose()
        {
            if (_effect != null) _effect.Dispose();
            if (_cloudIB != null) _cloudIB.Dispose();
            if (_cloudVB != null) _cloudVB.Dispose();
        }
        #endregion

        #region private methods
        private void CreateCloudMap()
        {
            int arrayIndex;
            for (int x = 0; x < _cloudMapSize; x++)
            {
                for (int z = 0; z < _cloudMapSize; z++)
                {
                    //Get Array index
                    arrayIndex = x + (z * _cloudMapSize);
                    var noiseResult = _noise.GetNoise2DValue(x, z, 2, 0.9);
                    float noiseValue = MathHelper.FullLerp(0, 1, noiseResult);
                    if (noiseValue > 0.3) 
                        _cloudMap[arrayIndex] = false;
                    else 
                        _cloudMap[arrayIndex] = true;
                }
            }
        }
        #endregion
    }
}
