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
using UtopiaContent.Effects.Weather;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Math.Noises;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.WorldFocus;

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

        private int cloud_radius_i = 12;
        private float cloud_size = 5;
        private float m_brightness = 0.9f;
        private float m_cloud_y = 140;

        private SimplexNoise _noise;

        private IndexBuffer<ushort> _cloudIB;
        private VertexBuffer<VertexPositionColor> _cloudVB;
        private List<ushort> _indices;
        private List<VertexPositionColor> _vertices;
        private HLSLVertexPositionColor _effect;
        #endregion

        #region Public properties
        #endregion
        public Clouds3D(D3DEngine d3dEngine, CameraManager camManager, IWeather weather, VisualWorldParameters worldParam, WorldFocusManager worldFocusManager)
        {
            _d3dEngine = d3dEngine;
            _worldParam = worldParam;
            _weather = weather;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
        }

        #region Public methods
        public override void Initialize()
        {
            _noise = new SimplexNoise(new Random(262));
            _noise.SetParameters(0.009, SimplexNoise.InflectionMode.ABSFct, SimplexNoise.ResultScale.ZeroToOne);
            _effect = new HLSLVertexPositionColor(_d3dEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);

            _indices = new List<ushort>();
            _vertices = new List<VertexPositionColor>();
        }

        public override void Update(ref GameTime TimeSpend)
        {
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        public override void Draw()
        {
            float m_time = 0;
            Vector2 cloud_speed = new Vector2(100, 0); //Les nuages n'avance que sur un axe, celui des X
            Vector2 m_camera_pos = new Vector2(0, 0); //Position de la caméra en X et Z, sans la composante Y

            // Position of cloud noise origin in world coordinates
            Vector2 world_cloud_origin_pos_f = m_time * cloud_speed;                      //Vitesse * Temps = Offset distance
            // Position of cloud noise origin from the camera
            Vector2 cloud_origin_from_camera_f = world_cloud_origin_pos_f - m_camera_pos; //Je retire la position de ma caméra
            // The center point of drawing in the noise
            Vector2 center_of_drawing_in_noise_f = -cloud_origin_from_camera_f;

            // The integer center point of drawing in the noise
            Location2<int> center_of_drawing_in_noise_i = new Location2<int>((int)(center_of_drawing_in_noise_f.X / cloud_size), (int)(center_of_drawing_in_noise_f.Y / cloud_size));

            // The world position of the integer center point of drawing in the noise
            Vector2 world_center_of_drawing_in_noise_f = new Vector2(center_of_drawing_in_noise_i.X * cloud_size, center_of_drawing_in_noise_i.Z * cloud_size) + world_cloud_origin_pos_f;

            int verticesCount = 0;
            _indices.Clear();
            _vertices.Clear();

            for (int zi = -cloud_radius_i; zi < cloud_radius_i; zi++)
            {
                for (int xi = -cloud_radius_i; xi < cloud_radius_i; xi++)
                {
                    Location2<int> p_in_noise_i = new Location2<int>(xi + center_of_drawing_in_noise_i.X, zi + center_of_drawing_in_noise_i.Z);

                    Vector2 p0 = new Vector2(xi, zi) * cloud_size + world_center_of_drawing_in_noise_f;

                    var noiseResult = _noise.GetNoise2DValue(p_in_noise_i.X * cloud_size / 200, p_in_noise_i.Z * cloud_size / 200, 3, 0.4);

                    if (noiseResult.Value < 0.40) continue;

                    float b = m_brightness;
                    Color c_top = new Color(b * 240, b * 240, b * 255, 128);
                    Color c_side_1 = new Color(b * 230, b * 230, b * 255, 128);
                    Color c_side_2 = new Color(b * 220, b * 220, b * 245, 128);
                    Color c_bottom = new Color(b * 205, b * 205, b * 230, 128);

                    VertexPositionColor[] v = new VertexPositionColor[4]
                    {
                        new VertexPositionColor(new Vector3(0,0,0), c_top),
                        new VertexPositionColor(new Vector3(0,0,0), c_top),
                        new VertexPositionColor(new Vector3(0,0,0), c_top),
                        new VertexPositionColor(new Vector3(0,0,0), c_top)
                    };

                    float rx = cloud_size;
                    float ry = 10;
                    float rz = cloud_size;

                    for (int i = 0; i < 6; i++)
                    {
                        switch (i)
                        {
                            case 0:	// top
                                v[0].Position.X = -rx; v[0].Position.Y = ry; v[0].Position.Z = -rz;
                                v[1].Position.X = -rx; v[1].Position.Y = ry; v[1].Position.Z = rz;
                                v[2].Position.X = rx; v[2].Position.Y = ry; v[2].Position.Z = rz;
                                v[3].Position.X = rx; v[3].Position.Y = ry; v[3].Position.Z = -rz;
                                break;
                            case 1: // back
                                for (int j = 0; j < 4; j++) v[j].Color = c_side_1;
                                v[0].Position.X = -rx; v[0].Position.Y = ry; v[0].Position.Z = -rz;
                                v[1].Position.X = rx; v[1].Position.Y = ry; v[1].Position.Z = -rz;
                                v[2].Position.X = rx; v[2].Position.Y = -ry; v[2].Position.Z = -rz;
                                v[3].Position.X = -rx; v[3].Position.Y = -ry; v[3].Position.Z = -rz;
                                break;
                            case 2: //right
                                for (int j = 0; j < 4; j++) v[j].Color = c_side_2;
                                v[0].Position.X = rx; v[0].Position.Y = ry; v[0].Position.Z = -rz;
                                v[1].Position.X = rx; v[1].Position.Y = ry; v[1].Position.Z = rz;
                                v[2].Position.X = rx; v[2].Position.Y = -ry; v[2].Position.Z = rz;
                                v[3].Position.X = rx; v[3].Position.Y = -ry; v[3].Position.Z = -rz;
                                break;
                            case 3: // front
                                for (int j = 0; j < 4; j++) v[j].Color = c_side_1;
                                v[0].Position.X = rx; v[0].Position.Y = ry; v[0].Position.Z = rz;
                                v[1].Position.X = -rx; v[1].Position.Y = ry; v[1].Position.Z = rz;
                                v[2].Position.X = -rx; v[2].Position.Y = -ry; v[2].Position.Z = rz;
                                v[3].Position.X = rx; v[3].Position.Y = -ry; v[3].Position.Z = rz;
                                break;
                            case 4: // left
                                for (int j = 0; j < 4; j++) v[j].Color = c_side_2;
                                v[0].Position.X = -rx; v[0].Position.Y = ry; v[0].Position.Z = rz;
                                v[1].Position.X = -rx; v[1].Position.Y = ry; v[1].Position.Z = -rz;
                                v[2].Position.X = -rx; v[2].Position.Y = -ry; v[2].Position.Z = -rz;
                                v[3].Position.X = -rx; v[3].Position.Y = -ry; v[3].Position.Z = rz;
                                break;
                            case 5: // bottom
                                for (int j = 0; j < 4; j++) v[j].Color = c_bottom;
                                v[0].Position.X = rx; v[0].Position.Y = -ry; v[0].Position.Z = rz;
                                v[1].Position.X = -rx; v[1].Position.Y = -ry; v[1].Position.Z = rz;
                                v[2].Position.X = -rx; v[2].Position.Y = -ry; v[2].Position.Z = -rz;
                                v[3].Position.X = rx; v[3].Position.Y = -ry; v[3].Position.Z = -rz;
                                break;
                        }

                        Vector3 pos = new Vector3(p0.X, m_cloud_y, p0.Y);

                        for (int j = 0; j < 4; j++)
                        {
                            v[j].Position += pos;
                        }

                        _vertices.AddRange(v);

                        _indices.Add((ushort)(0 + verticesCount));
                        _indices.Add((ushort)(1 + verticesCount));
                        _indices.Add((ushort)(2 + verticesCount));
                        _indices.Add((ushort)(2 + verticesCount));
                        _indices.Add((ushort)(3 + verticesCount));
                        _indices.Add((ushort)(0 + verticesCount));
                        verticesCount += 4;

                    }
                }
            }

            if (_indices.Count == 0) return;
            //Create/Update the Buffer
            if (_cloudIB == null) _cloudIB = new IndexBuffer<ushort>(_d3dEngine, _indices.Count, SharpDX.DXGI.Format.R16_UInt, 10, ResourceUsage.Dynamic);
            _cloudIB.SetData(_indices.ToArray(), true);

            if (_cloudVB == null) _cloudVB = new VertexBuffer<VertexPositionColor>(_d3dEngine, _vertices.Count, VertexPositionColor.VertexDeclaration, PrimitiveTopology.TriangleList, ResourceUsage.Dynamic, 10);
            _cloudVB.SetData(_vertices.ToArray(), true);


            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _effect.Begin();
            _effect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
            _effect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _effect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Translation(world_center_of_drawing_in_noise_f.X, 0, world_center_of_drawing_in_noise_f.Y);

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
        #endregion
    }
}
