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
using Utopia.Entities.Voxel;
using Utopia.InputManager;
using Utopia.Resources.ModelComp;
using Utopia.Settings;
using Utopia.Shared.Structs;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;
using UtopiaContent.Effects.Entities;

namespace Utopia.Components
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ModelEditorComponent : DrawableGameComponent
    {
        float _axisSize = 16f;
        readonly D3DEngine _d3DEngine;
        HLSLVertexPositionColor _lines3DEffect;
        VertexBuffer<VertexPositionColor> _linesVertexBuffer;

        HLSLVoxelModel _voxelEffect;

        MouseState _prevState;

        int _renderRasterId;
        int _renderModelRasterId;

        private float RotateX;
        private float RotateY;
        private float Scale = 1f;
        private Vector3 Translate;

        private Matrix _transform;
        private VisualVoxelModel _visualVoxelModel;

        private Matrix _projection;
        private Matrix _view;

        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
        
        public VisualVoxelModel VisualVoxelModel
        {
            get { return _visualVoxelModel; }
            set { _visualVoxelModel = value; }
        }

        /// <summary>
        /// Gets or sets current editor mode
        /// </summary>
        public EditorMode Mode { get; set; }
        
        public ModelEditorComponent(D3DEngine d3dEngine)
        {
            _d3DEngine = d3dEngine;
            Transform = Matrix.Identity;

            var aspect = d3dEngine.ViewPort.Width / d3dEngine.ViewPort.Height;
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 0.5f, 100);
            _view = Matrix.LookAtLH(new Vector3(0,0,5), new Vector3(0,0,0), Vector3.UnitY);
        }

        public override void LoadContent()
        {
            _renderRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.None, FillMode = FillMode.Solid });
            _renderModelRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Solid });

            _lines3DEffect = new HLSLVertexPositionColor(_d3DEngine, @"D3D\Effects\Basics\VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);

            // Create the 3 Lines

            var ptList = new List<VertexPositionColor>();
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(1f, 0, 0) * _axisSize, Color.Red).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 1f, 0) * _axisSize, Color.Green).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 0, 1f) * _axisSize, Color.Blue).PointsList);

            _linesVertexBuffer = new VertexBuffer<VertexPositionColor>(_d3DEngine, 6, VertexPositionColor.VertexDeclaration, PrimitiveTopology.LineList, "Axis_vertexBuffer");
            _linesVertexBuffer.SetData(ptList.ToArray());


            _voxelEffect = new HLSLVoxelModel(_d3DEngine, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            

            base.LoadContent();
        }
        
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime timeSpend)
        {
            
            var mouseState = Mouse.GetState();
            
            var dx = ((float)mouseState.X - _prevState.X) / 100;
            var dy = ((float)mouseState.Y - _prevState.Y) / 100;
            

            if (mouseState.MiddleButton == ButtonState.Pressed && _prevState.X != 0 && _prevState.Y != 0)
            {
                var keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                {
                    // translate
                    Translate.X -= dx;
                    Translate.Y -= dy;
                }
                else
                {
                    // rotate
                    RotateX += dx;
                    RotateY += dy;
                }
            }

            var horisontalRotationAxis = Vector3.UnitY;

            var bb = _visualVoxelModel.VoxelModel.States[_visualVoxelModel.ActiveState].BoundingBox;
            var translateVector = Vector3.Negate(Vector3.Subtract(bb.Maximum, bb.Minimum) / 2);
            var translation = Matrix.Translation(translateVector);

            //horisontalRotationAxis = Vector3.TransformCoordinate(horisontalRotationAxis, translation);

            var rotationX = Matrix.RotationX(RotateY);

            _transform = translation * rotationX;

            var axis2 = Vector3.TransformCoordinate(horisontalRotationAxis, rotationX);

            //_transform *= translation;

            _transform = _transform * Matrix.RotationAxis(axis2, -RotateX);
            
            Scale -= ((float)_prevState.ScrollWheelValue - mouseState.ScrollWheelValue)/10000;
            _transform = _transform * Matrix.Scaling(Scale);

            _transform *= Matrix.Translation(Translate);
            

            _prevState = mouseState;

            base.Update(ref timeSpend);
        }

        public override void Draw(int index)
        {
            switch (Mode)
            {
                case EditorMode.ModelView: DrawModelView(); break;
                case EditorMode.ModelLayout: DrawModelLayout(); break;
                case EditorMode.FrameEdit: DrawFrameEdit(); break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawModelView()
        {
            StatesRepository.ApplyRaster(_renderRasterId);

            //Set Effect variables
            _lines3DEffect.Begin();
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(_transform); //Matrix.Translation(new Vector3(-0.5f,-0.5f,-0.5f)) *
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _lines3DEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_projection);
            _lines3DEffect.CBPerFrame.IsDirty = true;
            _lines3DEffect.Apply();

            //Set the vertex buffer to the Graphical Card.
            _linesVertexBuffer.SetToDevice(0);

            _d3DEngine.Context.Draw(6, 0); //2 Vertex by line ! ==> 6 vertex to draw

            // draw the model

            if (_visualVoxelModel != null)
            {
                StatesRepository.ApplyRaster(_renderModelRasterId);

                _voxelEffect.Begin();
                _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_view * _projection);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply();

                _visualVoxelModel.Draw(_voxelEffect);
            }
        }

        private void DrawModelLayout()
        {

        }

        private void DrawFrameEdit()
        {

        }

        public override void UnloadContent()
        {
            if (_linesVertexBuffer != null) _linesVertexBuffer.Dispose();
            _lines3DEffect.Dispose();

            base.UnloadContent();
        }
    }

    public enum EditorMode
    {
        /// <summary>
        /// Represents a simple model view
        /// </summary>
        ModelView,
        /// <summary>
        /// Shows whole model and allows to change layout of the parts
        /// </summary>
        ModelLayout,
        /// <summary>
        /// Shows only one frame of the part and allows voxel edition
        /// </summary>
        FrameEdit
    }
}
