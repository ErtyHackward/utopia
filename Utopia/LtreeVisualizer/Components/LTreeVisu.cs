using LtreeVisualizer.DataPipe;
using LtreeVisualizer.Shadder;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3Resources.Primitives;
using S33M3Resources.VertexFormats;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Structs.Landscape;

namespace LtreeVisualizer.Components
{
    public class LTreeVisu : DrawableGameComponent
    {
        private TreeBluePrint _newTemplate;
        private TreeLSystem _treeSystem = new TreeLSystem();
        private Vector3[] vertexCube;
        private short[] indicesCube;

        private VertexBuffer<VertexPosition3Color> _vb;
        private IndexBuffer<short> _ib;

        private HLSLLTree _shader;

        private List<VertexPosition3Color> _letreeVertexCollection = new List<VertexPosition3Color>();
        private List<short> _letreeIndexCollection = new List<short>();

        private bool _bufferDirty = true;

        private CameraManager<ICamera> _cameraManager;

        public LTreeVisu(CameraManager<ICamera> cameraManager)
        {
            _cameraManager = cameraManager;
        }

        public override void Initialize()
        {
            //Create the mesh from the result.
            Generator.Cube(1, out vertexCube, out indicesCube);
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _shader = new HLSLLTree(context.Device, @"Shadder\LTreeVisu.hlsl", VertexPosition3Color.VertexDeclaration);

            _vb = new VertexBuffer<VertexPosition3Color>(context.Device, 16, VertexPosition3Color.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "VB", AutoResizePerc: 10);
            _ib = new IndexBuffer<short>(context.Device, 32, SharpDX.DXGI.Format.R16_UInt, "IB");
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            if (Pipe.MessagesQueue.TryDequeue(out _newTemplate))
            {
                RefreshBuffers(_newTemplate);
            }
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_bufferDirty)
            {
                _vb.SetData(context, _letreeVertexCollection.ToArray());
                _ib.SetData(context, _letreeIndexCollection.ToArray());

                _bufferDirty = false;
            }

            _shader.Begin(context);
            _shader.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
            _shader.CBPerFrame.IsDirty = true;

            _shader.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Identity);
            _shader.CBPerDraw.IsDirty = true;
            _shader.Apply(context);

            _vb.SetToDevice(context, 0);
            _ib.SetToDevice(context, 0);

            context.DrawIndexed(_ib.IndicesCount, 0, 0);
        }

        private void RefreshBuffers(TreeBluePrint treeTemplate)
        {
            FastRandom rnd = new FastRandom(0);
            //Generate the list of Tree points.
            List<BlockWithPosition> result = _treeSystem.Generate(rnd, new S33M3Resources.Structs.Vector3I(), treeTemplate);

            _letreeVertexCollection = new List<VertexPosition3Color>();
            _letreeIndexCollection = new List<short>();
            
            //For each block
            foreach (BlockWithPosition block in result)
            {
                int vertexOffset = _letreeVertexCollection.Count;
                //Create the 24 vertex + 36 Index data per cube !
                for (int i = 0; i < vertexCube.Length; i++)
                {
                    _letreeVertexCollection.Add(new VertexPosition3Color(vertexCube[i], Color.GreenYellow));
                }

                foreach (var index in indicesCube)
                {
                    _letreeIndexCollection.Add((short)(index + vertexOffset));
                }

            }
            _bufferDirty = true;

        }
    }
}
