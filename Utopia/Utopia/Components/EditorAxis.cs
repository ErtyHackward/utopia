using System;
using System.Collections.Generic;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using SharpDX;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Buffers;
using S33M3Engines.D3D.Effects.Basics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Resources.ModelComp;
using Utopia.Shared.Structs;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;

namespace Utopia.Components
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class EditorAxis : DrawableGameComponent
    {
        float _axisSize = 1f;
        D3DEngine _d3dEngine;
        HLSLVertexPositionColor _wrappedEffect;
        //Buffer _vertexBuffer;
        VertexBuffer<VertexPositionColor> _vertexBuffer;
        MouseState _prevState;

        int _renderRasterId;

        private Matrix _transform;
        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        private Matrix _projection;
        private Matrix _view;

        public EditorAxis(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            Transform = Matrix.Identity;

            var aspect = d3dEngine.ViewPort.Width / d3dEngine.ViewPort.Height;
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 0.5f, 100);
            _view = Matrix.LookAtLH(new Vector3(0,0,5), new Vector3(0,0,0), Vector3.UnitY);
        }

        public override void LoadContent()
        {
            _renderRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.None, FillMode = FillMode.Solid });

            _wrappedEffect = new HLSLVertexPositionColor(_d3dEngine, @"D3D\Effects\Basics\VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);

            CreateVertexBuffer();

            base.LoadContent();
        }

        private void CreateVertexBuffer()
        {
            // Create the 3 Lines

            var ptList = new List<VertexPositionColor>();
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(1f, 0, 0) * _axisSize, Color.Red).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 1f, 0) * _axisSize, Color.Green).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 0, 1f) * _axisSize, Color.Blue).PointsList);

            _vertexBuffer = new VertexBuffer<VertexPositionColor>(_d3dEngine, 6, VertexPositionColor.VertexDeclaration, PrimitiveTopology.LineList, "Axis_vertexBuffer");
            _vertexBuffer.SetData(ptList.ToArray());
        }

        private float RotateX;
        private float RotateY;
        private float Scale = 1f;
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime timeSpend)
        {
            
            var mouseState = Mouse.GetState();

            var dx = ((float)mouseState.X - _prevState.X) / 100;
            var dy = ((float)mouseState.Y - _prevState.Y) / 100;
            

            if (mouseState.RightButton == ButtonState.Pressed && _prevState.X != 0 && _prevState.Y != 0)
            {
                RotateX += dx;
                RotateY += dy;
            }

            _transform = Matrix.RotationX(RotateY);

            var axis2 = Vector3.TransformCoordinate(Vector3.UnitY, _transform);
            _transform = _transform * Matrix.RotationAxis(axis2, -RotateX);
            
            Scale -= ((float)_prevState.ScrollWheelValue - mouseState.ScrollWheelValue)/1000;
            _transform = _transform * Matrix.Scaling(Scale);
            
            _prevState = mouseState;

            base.Update(ref timeSpend);
        }

        public override void Draw(int index)
        {
            StatesRepository.ApplyRaster(_renderRasterId);

            //Set Effect variables
            _wrappedEffect.Begin();
            _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(_transform); //Matrix.Translation(new Vector3(-0.5f,-0.5f,-0.5f)) *
            _wrappedEffect.CBPerDraw.IsDirty = true;
            _wrappedEffect.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _wrappedEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_projection);
            _wrappedEffect.CBPerFrame.IsDirty = true;
            _wrappedEffect.Apply();

            //Set the vertex buffer to the Graphical Card.
            _vertexBuffer.SetToDevice(0);

            _d3dEngine.Context.Draw(6, 0); //2 Vertex by line ! ==> 6 vertex to draw
        }

        public override void UnloadContent()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _wrappedEffect.Dispose();

            base.UnloadContent();
        }
    }
}
