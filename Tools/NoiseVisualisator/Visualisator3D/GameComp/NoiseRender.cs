using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using SharpDX;
using Samples.CustomShaders;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Maths.Noises;
using S33M3Resources.Structs;
using Samples.RenderStates;
using S33M3DXEngine.RenderStates;
using NoiseVisualisator.Visualisator3D.GameComp;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Sampler;

namespace Samples.GameComp
{
    public class NoiseRender : DrawableGameComponent
    {
        private BoundingBox3D _bbox;
        private NoiseShader _shader;
        private List<VertexPosition4Color> _vertices = new List<VertexPosition4Color>();
        private List<UInt32> _indices = new List<UInt32>();
        private VertexBuffer<VertexPosition4Color> _vertexBuffer;
        private IndexBuffer<UInt32> _indexBuffer;
        private D3DEngine _engine;
        private INoise3 _noise;
        private Matrix _chunkworldPosition;
        private bool[, ,] ChunkCube;
        private SharedFrameCB _sharedCB;
        private ICamera _camera;

        public enum CubeFaces : byte
        {
            Back = 0,
            Front = 1,
            Bottom = 2,
            Top = 3,
            Left = 4,
            Right = 5
        }

        public NoiseRender(D3DEngine engine, INoise3 noise, SharedFrameCB sharedCB, ICamera camera)
        {
            _sharedCB = sharedCB;
            _engine = engine;
            _noise = noise;
            _camera = camera;
        }

        public override void Initialize()
        {
            _shader = ToDispose(new NoiseShader(_engine.Device, _sharedCB.CBPerFrame));

            _chunkworldPosition = Matrix.Identity;

            _bbox = new BoundingBox3D(_engine, new Vector3(NoiseSizeResultX, NoiseSizeResultY, NoiseSizeResultZ), ToDispose(new HLSLVertexPositionColor(_engine.Device)), Colors.Yellow);

            base.Initialize();
        }

        int NoiseSizeResultX = 128;
        int NoiseSizeResultY = 128;
        int NoiseSizeResultZ = 128;
        double SolidCubeThreshold = 0.5;
        public override void LoadContent(DeviceContext Context = null)
        {
            //Genereate the 3D Landscape.
            double[] result = NoiseSampler.NoiseSampling(_noise, new Vector3I(NoiseSizeResultX , NoiseSizeResultY , NoiseSizeResultZ ),
                                                            0.0, 0.42, NoiseSizeResultX,
                                                            0.0, 0.42, NoiseSizeResultY,
                                                            0.0, 0.42, NoiseSizeResultZ);

            GenerateChunkMeshFromNoise(result);

            if (_indices.Count > 0)
            {

                _vertexBuffer = new VertexBuffer<VertexPosition4Color>(_engine.Device, _vertices.Count, VertexPosition4Color.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "VertexBuffer");
                _vertexBuffer.SetData(Context, _vertices.ToArray());

                _indexBuffer = new IndexBuffer<UInt32>(_engine.Device, _indices.Count, SharpDX.DXGI.Format.R32_UInt, "IndexBuffer");
                _indexBuffer.SetData(Context, _indices.ToArray());
            }
            else
            {
                Console.WriteLine("No point to render !");
            }
        }

        private void GenerateChunkMeshFromNoise(double[] result)
        {
            ChunkCube = new bool[NoiseSizeResultX, NoiseSizeResultY, NoiseSizeResultZ];

            double MinNoise = double.MaxValue;
            double MaxNoise = double.MinValue;

            int indexId = 0;
            for (int x = 0; x < NoiseSizeResultX; x++)
            {
                for (int z = 0; z < NoiseSizeResultZ; z++)
                {
                    for (int y = 0; y < NoiseSizeResultY; y++)
                    {
                        double val = result[indexId];

                        if (val < MinNoise) MinNoise = val;
                        if (val > MaxNoise) MaxNoise = val;

                        if (val > SolidCubeThreshold)
                        {
                            ChunkCube[x, y, z] = true;
                        }
                        indexId ++;
                    }
                }
            }

            Console.WriteLine("Min Value : " + MinNoise.ToString() + " Max Value : " + MaxNoise.ToString());

            CreateCubeMeshes();
        }

        private void CreateCubeMeshes()
        {
            GenerateCubesFace(CubeFaces.Front);
            GenerateCubesFace(CubeFaces.Back);
            GenerateCubesFace(CubeFaces.Bottom);
            GenerateCubesFace(CubeFaces.Top);
            GenerateCubesFace(CubeFaces.Left);
            GenerateCubesFace(CubeFaces.Right);
        }

        private void GenerateCubesFace(CubeFaces cubeFace)
        {
            bool neightborCube;

            for (int x = 0; x < NoiseSizeResultX; x++)
            {
                for (int z = 0; z < NoiseSizeResultZ; z++)
                {
                    for (int y = 0; y < NoiseSizeResultY; y++)
                    {
                        if (ChunkCube[x, y, z] == false) continue;
                        //Check to see if the face needs to be generated or not !
                        //Border Chunk test ! ==> Don't generate faces that are "border" chunks
                        //BorderChunk value is true if the chunk is at the border of the visible world.
                        switch (cubeFace)
                        {
                            case CubeFaces.Back:
                                if (z - 1 < 0)  neightborCube = false;
                                else neightborCube = ChunkCube[x, y, z - 1];
                                break;
                            case CubeFaces.Front:
                                if (z + 1 >= NoiseSizeResultZ) neightborCube = false;
                                else neightborCube = ChunkCube[x, y, z + 1];
                                break;
                            case CubeFaces.Bottom:
                                if (y - 1 < 0) neightborCube = false;
                                else neightborCube = ChunkCube[x, y - 1, z];
                                break;
                            case CubeFaces.Top:
                                if (y + 1 >= NoiseSizeResultY) neightborCube = false;
                                else neightborCube = ChunkCube[x, y + 1, z];
                                break;
                            case CubeFaces.Left:
                                if (x - 1 < 0) neightborCube = false;
                                else neightborCube = ChunkCube[x - 1, y, z];
                                break;
                            case CubeFaces.Right:
                                if (x + 1 >= NoiseSizeResultX) neightborCube = false;
                                else neightborCube = ChunkCube[x + 1, y, z];
                                break;
                            default:
                                throw new NullReferenceException();
                        }

                        if (neightborCube == false)
                        {
                            GenCubeFace(cubeFace, new Vector3I(x, y, z));
                        }
                    }
                }
            }
        }

        //private List<int> _indices = new List<int>();
        //private VertexBuffer<VertexPosition4Color> _vertexBuffer;
        private void GenCubeFace(CubeFaces cubeFace, Vector3I cubePosi)
        {
            int verticeCubeOffset = _vertices.Count;
            int indiceCubeOffset = _indices.Count;
            ByteColor newColor = Colors.Gray;
            ByteColor LightGray = Colors.LightGray;
            ByteColor DarkGray = Colors.DarkGray;
            Vector4 cubePosition = new Vector4(cubePosi.X, cubePosi.Y, cubePosi.Z, 1);

            Vector4 topLeft;
            Vector4 topRight;
            Vector4 bottomLeft;
            Vector4 bottomRight;

            switch (cubeFace)
            {
                case CubeFaces.Back:
                    topLeft = cubePosition + new Vector4(1, 1, 0, 0);
                    topRight = cubePosition + new Vector4(0, 1, 0, 0);
                    bottomLeft = cubePosition + new Vector4(1, 0, 0, 0);
                    bottomRight = cubePosition + new Vector4(0, 0, 0, 0);

                    _vertices.Add(new VertexPosition4Color(topRight, newColor));
                    _vertices.Add(new VertexPosition4Color(topLeft, newColor));
                    _vertices.Add(new VertexPosition4Color(bottomRight, newColor));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, newColor));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));

                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                case CubeFaces.Front:
                    topLeft = cubePosition + new Vector4(0, 1, 1, 0);
                    topRight = cubePosition + new Vector4(1, 1, 1, 0);
                    bottomLeft = cubePosition + new Vector4(0, 0, 1, 0);
                    bottomRight = cubePosition + new Vector4(1, 0, 1, 0);

                    _vertices.Add(new VertexPosition4Color(topLeft, newColor));
                    _vertices.Add(new VertexPosition4Color(topRight, newColor));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, newColor));
                    _vertices.Add(new VertexPosition4Color(bottomRight, newColor));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));

                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                case CubeFaces.Bottom:
                    topLeft = cubePosition + new Vector4(0, 0, 1, 0);
                    topRight = cubePosition + new Vector4(1, 0, 1, 0);
                    bottomLeft = cubePosition + new Vector4(0, 0, 0, 0);
                    bottomRight = cubePosition + new Vector4(1, 0, 0, 0);

                    _vertices.Add(new VertexPosition4Color(topLeft, LightGray));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, LightGray));
                    _vertices.Add(new VertexPosition4Color(topRight, LightGray));
                    _vertices.Add(new VertexPosition4Color(bottomRight, LightGray));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));

                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                case CubeFaces.Top:

                    topLeft = cubePosition + new Vector4(0, 1, 0, 0);
                    topRight = cubePosition + new Vector4(1, 1, 0, 0);
                    bottomLeft = cubePosition + new Vector4(0, 1, 1, 0);
                    bottomRight = cubePosition + new Vector4(1, 1, 1, 0);

                    _vertices.Add(new VertexPosition4Color(topLeft, LightGray));
                    _vertices.Add(new VertexPosition4Color(bottomRight, LightGray));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, LightGray));
                    _vertices.Add(new VertexPosition4Color(topRight, LightGray));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                case CubeFaces.Left:
                    topLeft = cubePosition + new Vector4(0, 1, 0, 0);
                    bottomRight = cubePosition + new Vector4(0, 0, 1, 0);
                    bottomLeft = cubePosition + new Vector4(0, 0, 0, 0);
                    topRight = cubePosition + new Vector4(0, 1, 1, 0);

                    _vertices.Add(new VertexPosition4Color(topLeft, DarkGray));
                    _vertices.Add(new VertexPosition4Color(topRight, DarkGray));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, DarkGray));
                    _vertices.Add(new VertexPosition4Color(bottomRight, DarkGray));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));

                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                case CubeFaces.Right:
                    topLeft = cubePosition + new Vector4(1, 1, 1, 0);
                    topRight = cubePosition + new Vector4(1, 1, 0, 0);
                    bottomLeft = cubePosition + new Vector4(1, 0, 1, 0);
                    bottomRight = cubePosition + new Vector4(1, 0, 0, 0);

                    _vertices.Add(new VertexPosition4Color(topRight, DarkGray));
                    _vertices.Add(new VertexPosition4Color(topLeft, DarkGray));
                    _vertices.Add(new VertexPosition4Color(bottomLeft, DarkGray));
                    _vertices.Add(new VertexPosition4Color(bottomRight, DarkGray));

                    _indices.Add((UInt32)(0 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(3 + verticeCubeOffset));

                    _indices.Add((UInt32)(1 + verticeCubeOffset));
                    _indices.Add((UInt32)(2 + verticeCubeOffset));
                    _indices.Add((UInt32)(0 + verticeCubeOffset));

                    verticeCubeOffset += 4;
                    break;
                default:
                    throw new NullReferenceException();
            }
        }

        public override void Draw(DeviceContext context,int index)
        {
            if (_indexBuffer == null) return;

            _bbox.Draw(context, _camera.View, _camera.Projection3D);

            RenderStatesRepo.ApplyStates(DXRenderStates.Rasters.Default, DXRenderStates.Blenders.Disabled, DXRenderStates.DepthStencils.DepthEnabled);

            _shader.Begin(context);
            _shader.CBPerDraw.Values.World = Matrix.Transpose(_chunkworldPosition);
            _shader.CBPerDraw.IsDirty = true;
            _shader.Apply(context);

            _vertexBuffer.SetToDevice(context, 0);
            _indexBuffer.SetToDevice(context, 0);

            _engine.ImmediateContext.DrawIndexed(_indexBuffer.IndicesCount, 0, 0);
        }

    }
}
