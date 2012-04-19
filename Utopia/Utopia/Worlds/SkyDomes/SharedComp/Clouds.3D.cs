using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Buffers;
using S33M3CoreComponents.Maths.Noises;
using S33M3Resources.VertexFormats;
using S33M3Resources.Structs;
using SharpDX;
using UtopiaContent.Effects.Weather;
using Utopia.Effects.Shared;
using Utopia.Components;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.Settings;
using Utopia.Shared.GameDXStates;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using SharpDX.Direct3D;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    //3D Clouds stuff
    public partial class Clouds
    {
        #region Private variables
        //3D Clouds specialized Variables
        private int _cloudMap_size;
        private float _cloud_size = 40;
        private float _cloud_Height = 5;
        private float _brightness = 0.9f;
        private float _cloudLayerHeight = 140;

        private SimplexNoise _noise;

        private IndexBuffer<ushort> _cloudIB3D;
        private VertexBuffer<VertexPosition3Color> _cloudVB;
        private ushort[] _indices;
        private VertexPosition3Color[] _vertices;
        private int _nbrIndices, _nbrVertices;
        private int _maxNbrIndices, _maxNbrVertices;
        private HLSLClouds3D _effect;
        private FTSValue<Vector2> _cloud_MapOffset;
        private ByteColor _topFace, _side1Face, _side2Face, _bottomFace;

        private VertexPosition3Color[] _faces;

        private int _cloudMapSize;
        private int _cloudMapSizeSquare;
        private bool[] _cloudMap3D;
        private SharedFrameCB _sharedCB;
        #endregion

        #region Public properties/variables
        #endregion

        #region Public methods
        public void LateInitialization(SharedFrameCB sharedCB)
        {
            _sharedCB = sharedCB;
        }
        #endregion

        #region private methods
        private void Initialize3D()
        {
            _cloud_MapOffset = new FTSValue<Vector2>();
            _cloudMap_size = (int)(_worldParam.WorldVisibleSize.X / _cloud_size * 4);

            //Create a virtual Cloud map of 1024 * 1024 size !
            _cloudMapSize = 1024;
            _cloudMapSizeSquare = _cloudMapSize * _cloudMapSize;
            _cloudMap3D = new bool[_cloudMapSizeSquare];

            _noise = new SimplexNoise(new Random());
            _noise.SetParameters(0.075, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
            _effect = new HLSLClouds3D(_d3dEngine.Device, ClientSettings.EffectPack + @"Weather\Clouds3D.hlsl", _sharedCB.CBPerFrame);
            _effect.SamplerBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipPoint);

            _maxNbrIndices = 15000;
            _maxNbrVertices = 10000;
            _indices = new ushort[_maxNbrIndices];
            _vertices = new VertexPosition3Color[_maxNbrVertices];
            _nbrIndices = 0;
            _nbrVertices = 0;

            _topFace = new ByteColor((byte)_brightness * 240, (byte)_brightness * 240, (byte)_brightness * 255, (byte)200);
            _side1Face = new ByteColor((byte)_brightness * 230, (byte)_brightness * 230, (byte)_brightness * 255, (byte)200);
            _side2Face = new ByteColor((byte)_brightness * 220, (byte)_brightness * 220, (byte)_brightness * 245, (byte)200);
            _bottomFace = new ByteColor((byte)_brightness * 205, (byte)_brightness * 205, (byte)_brightness * 230, (byte)200);

            _faces = new VertexPosition3Color[4];

            CreateCloudMap();
        }

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
                        _cloudMap3D[arrayIndex] = false;
                    else
                        _cloudMap3D[arrayIndex] = true;
                }
            }
        }

        private void Update3D(GameTime timeSpend)
        {
            _cloud_MapOffset.BackUpValue();

            _cloud_MapOffset.Value.X += timeSpend.ElapsedGameTimeInS_LD * _weather.Wind.WindFlow.X * 7;
            _cloud_MapOffset.Value.Y += timeSpend.ElapsedGameTimeInS_LD * _weather.Wind.WindFlow.Z * 7;
        }

        private void Interpolation3D(double interpolation_hd, float interpolation_ld, long timePassed)
        {
            Vector2.Lerp(ref _cloud_MapOffset.ValuePrev, ref _cloud_MapOffset.Value, interpolation_ld, out _cloud_MapOffset.ValueInterp);
        }

        private void Draw3D(DeviceContext context, int index)
        {
            Vector2 m_camera_pos = new Vector2((float)_camManager.ActiveCamera.WorldPosition.X, (float)_camManager.ActiveCamera.WorldPosition.Z); //Position de la caméra en X et Z, sans la composante Y

            Vector2 CloudsMapOffset = _cloud_MapOffset.ValueInterp - m_camera_pos;                      //Speed * Time = Distance
            Vector2 CloudsMapOffsetWithCamera = -(CloudsMapOffset - m_camera_pos); //Je retire la position de ma caméra, pour compenser le mouvement de la caméra
            Vector2I center_of_drawing_in_noise_i = new Vector2I((int)(CloudsMapOffsetWithCamera.X / _cloud_size), (int)(CloudsMapOffsetWithCamera.Y / _cloud_size));
            Vector2 world_center_of_drawing_in_noise_f = new Vector2(center_of_drawing_in_noise_i.X * _cloud_size, center_of_drawing_in_noise_i.Y * _cloud_size) + CloudsMapOffset;

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
                    Vector2I p_in_noise_i = new Vector2I(xi + center_of_drawing_in_noise_i.X, zi + center_of_drawing_in_noise_i.Y);

                    Vector2 p0 = new Vector2(xi, zi) * _cloud_size + world_center_of_drawing_in_noise_f;

                    _cloudMapXIndex = MathHelper.Mod(p_in_noise_i.X, _cloudMapSize);
                    _cloudMapZIndex = MathHelper.Mod(p_in_noise_i.Y, _cloudMapSize);
                    if (!_cloudMap3D[_cloudMapXIndex + (_cloudMapZIndex * _cloudMapSize)]) continue;
                    //var noiseResult = _noise.GetNoise2DValue(p_in_noise_i.X, p_in_noise_i.Z, 2, 0.9);
                    //float noiseValue = MathHelper.FullLerp(0, 1, noiseResult);

                    //if (noiseValue > 0.3) continue;

                    float rx = _cloud_size / 2;
                    float ry = _cloud_Height;
                    float rz = _cloud_size / 2;

                    int neightBCloudIndex;

                    for (int i = 0; i < 6; i++)
                    {
                        switch (i)
                        {
                            case 0:	// top, always draw
                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _topFace;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _topFace;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = ry; _faces[2].Position.Z = rz; _faces[2].Color = _topFace;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = ry; _faces[3].Position.Z = -rz; _faces[3].Color = _topFace;
                                break;
                            case 1: // back
                                neightBCloudIndex = MathHelper.Mod(p_in_noise_i.X, _cloudMapSize) + ((MathHelper.Mod(p_in_noise_i.Y - 1, _cloudMapSize)) * _cloudMapSize);
                                if (neightBCloudIndex < 0 || neightBCloudIndex >= _cloudMapSizeSquare || _cloudMap3D[neightBCloudIndex]) continue;

                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _side1Face;
                                _faces[1].Position.X = rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = -rz; _faces[1].Color = _side1Face;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = -rz; _faces[2].Color = _side1Face;
                                _faces[3].Position.X = -rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = -rz; _faces[3].Color = _side1Face;
                                break;
                            case 2: //right
                                neightBCloudIndex = MathHelper.Mod(p_in_noise_i.X + 1, _cloudMapSize) + ((MathHelper.Mod(p_in_noise_i.Y, _cloudMapSize)) * _cloudMapSize);
                                if (neightBCloudIndex < 0 || neightBCloudIndex >= _cloudMapSizeSquare || _cloudMap3D[neightBCloudIndex]) continue;
                                _faces[0].Position.X = rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = -rz; _faces[0].Color = _side2Face;
                                _faces[1].Position.X = rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _side2Face;
                                _faces[2].Position.X = rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = rz; _faces[2].Color = _side2Face;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = -rz; _faces[3].Color = _side2Face;
                                break;
                            case 3: // front
                                neightBCloudIndex = MathHelper.Mod(p_in_noise_i.X, _cloudMapSize) + ((MathHelper.Mod(p_in_noise_i.Y + 1, _cloudMapSize)) * _cloudMapSize);
                                if (neightBCloudIndex < 0 || neightBCloudIndex >= _cloudMapSizeSquare || _cloudMap3D[neightBCloudIndex]) continue;
                                _faces[0].Position.X = rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = rz; _faces[0].Color = _side1Face;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = rz; _faces[1].Color = _side1Face;
                                _faces[2].Position.X = -rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = rz; _faces[2].Color = _side1Face;
                                _faces[3].Position.X = rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = rz; _faces[3].Color = _side1Face;
                                break;
                            case 4: // left
                                neightBCloudIndex = MathHelper.Mod(p_in_noise_i.X - 1, _cloudMapSize) + ((MathHelper.Mod(p_in_noise_i.Y, _cloudMapSize)) * _cloudMapSize);
                                if (neightBCloudIndex < 0 || neightBCloudIndex >= _cloudMapSizeSquare || _cloudMap3D[neightBCloudIndex]) continue;
                                _faces[0].Position.X = -rx; _faces[0].Position.Y = ry; _faces[0].Position.Z = rz; _faces[0].Color = _side2Face;
                                _faces[1].Position.X = -rx; _faces[1].Position.Y = ry; _faces[1].Position.Z = -rz; _faces[1].Color = _side2Face;
                                _faces[2].Position.X = -rx; _faces[2].Position.Y = -ry; _faces[2].Position.Z = -rz; _faces[2].Color = _side2Face;
                                _faces[3].Position.X = -rx; _faces[3].Position.Y = -ry; _faces[3].Position.Z = rz; _faces[3].Color = _side2Face;
                                break;
                            case 5: // bottom, always draw
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
                            Array.Resize<VertexPosition3Color>(ref _vertices, _maxNbrVertices);
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
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 4); //0 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 3); //1 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 4); //0 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 2); //2 + verticesCount);
                        _nbrIndices++;
                        _indices[_nbrIndices] = (ushort)(_nbrVertices - 1); //3 + verticesCount);
                        _nbrIndices++;
                    }
                }
            }

            if (_nbrIndices == 0) return;
            //Create/Update the Buffer
            if (_cloudIB3D == null) _cloudIB3D = new IndexBuffer<ushort>(_d3dEngine.Device, _nbrIndices, SharpDX.DXGI.Format.R16_UInt, "_cloudIB", 10, ResourceUsage.Dynamic);
            _cloudIB3D.SetData(_d3dEngine.ImmediateContext, _indices, 0, _nbrIndices, true);

            if (_cloudVB == null) _cloudVB = new VertexBuffer<VertexPosition3Color>(_d3dEngine.Device, _nbrVertices, VertexPosition3Color.VertexDeclaration, PrimitiveTopology.TriangleList, "_cloudVB", ResourceUsage.Dynamic, 10);
            _cloudVB.SetData(_d3dEngine.ImmediateContext, _vertices, 0, _nbrVertices, true);

            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

            _effect.Begin(_d3dEngine.ImmediateContext);
            _effect.SolidBackBuffer.Value = _solidBackBuffer.SolidStaggingBackBuffer;

            Matrix world = Matrix.Identity;

            _worldFocusManager.CenterTranslationMatrixOnFocus(ref world, ref world);

            _effect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(_d3dEngine.ImmediateContext);

            //Set the buffer to the graphical card
            _cloudIB3D.SetToDevice(_d3dEngine.ImmediateContext, 0);
            _cloudVB.SetToDevice(_d3dEngine.ImmediateContext, 0);

            //Draw
            _d3dEngine.ImmediateContext.DrawIndexed(_cloudIB3D.IndicesCount, 0, 0);
        }

        private void BeforeDispose3D()
        {
            if (_effect != null) _effect.Dispose();
            if (_cloudIB3D != null) _cloudIB3D.Dispose();
            if (_cloudVB != null) _cloudVB.Dispose();
        }
        #endregion
    }
}
