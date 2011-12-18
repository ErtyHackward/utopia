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
using Utopia.Entities.Voxel;
using Utopia.Settings;
using Utopia.Shared.Structs;
using S33M3Engines;
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
        VertexBuffer<VertexPositionColor> _boxVertexBuffer;
        IndexBuffer<ushort> _boxIndexBuffer;

        HLSLVoxelModel _voxelEffect;

        MouseState _prevState;

        int _renderRasterId;
        int _renderModelRasterId;

        private float RotateX;
        private float RotateY;
        private float Scale = 0.1f;
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

            
            var ptList = new List<VertexPositionColor>();

            var color = Color.White;

            var topLeftFront =      new VertexPositionColor(new Vector3(0, 1, 1), color);
            var topLeftBack =       new VertexPositionColor(new Vector3(0, 1, 0), color);
            var topRightFront =     new VertexPositionColor(new Vector3(1, 1, 1), color);
            var topRightBack =      new VertexPositionColor(new Vector3(1, 1, 0), color);
            var bottomLeftFront =   new VertexPositionColor(new Vector3(0, 0, 1), color);
            var bottomLeftBack =    new VertexPositionColor(new Vector3(0, 0, 0), color);
            var bottomRightFront =  new VertexPositionColor(new Vector3(1, 0, 1), color);
            var bottomRightBack =   new VertexPositionColor(new Vector3(1, 0, 0), color);

            ptList.Add(topLeftFront);       // 0
            ptList.Add(topLeftBack);        // 1
            ptList.Add(topRightFront);      // 2
            ptList.Add(topRightBack);       // 3
            ptList.Add(bottomLeftFront);    // 4
            ptList.Add(bottomLeftBack);     // 5
            ptList.Add(bottomRightFront);   // 6
            ptList.Add(bottomRightBack);    // 7

            var indices = new ushort[] { 0, 1, 1, 3, 3, 2, 2, 0, 4, 5, 5, 7, 7, 6, 6, 4, 0, 4, 2, 6, 1, 5, 3, 7 };
            
            _boxVertexBuffer = new VertexBuffer<VertexPositionColor>(_d3DEngine, 8, VertexPositionColor.VertexDeclaration, PrimitiveTopology.LineList, "EditorBox_vertexBuffer");
            _boxVertexBuffer.SetData(ptList.ToArray());

            _boxIndexBuffer = new IndexBuffer<ushort>(_d3DEngine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "EditorBox_indexBuffer");
            _boxIndexBuffer.SetData(indices);

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

            Scale -= ((float)_prevState.ScrollWheelValue - mouseState.ScrollWheelValue) / 10000;

            var horisontalRotationAxis = Vector3.UnitY;

            var bb = _visualVoxelModel.VoxelModel.States[_visualVoxelModel.ActiveState].BoundingBox;
            var translateVector = Vector3.Negate(Vector3.Subtract(bb.Maximum, bb.Minimum) / 2);
            var translation = Matrix.Translation(translateVector);
            var rotationX = Matrix.RotationX(RotateY);
            _transform = translation * rotationX;
            var axis2 = Vector3.TransformCoordinate(horisontalRotationAxis, rotationX);
            _transform = _transform * Matrix.RotationAxis(axis2, -RotateX);
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

        private void DrawBox(BoundingBox box)
        {
            DrawBox(box.Minimum, box.Maximum);
        }

        private void DrawBox(Vector3 min, Vector3 max)
        {

            var size = Vector3.Subtract(max, min);
            StatesRepository.ApplyRaster(_renderRasterId);

            //Set Effect variables
            _lines3DEffect.Begin();
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(size) * Matrix.Translation(min) * _transform); //Matrix.Translation(new Vector3(-0.5f,-0.5f,-0.5f)) *
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.CBPerFrame.Values.View = Matrix.Transpose(_view);
            _lines3DEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_projection);
            _lines3DEffect.CBPerFrame.IsDirty = true;
            _lines3DEffect.Apply();

            //Set the vertex buffer to the Graphical Card.
            _boxVertexBuffer.SetToDevice(0);
            _boxIndexBuffer.SetToDevice(0);

            _d3DEngine.Context.DrawIndexed(24, 0, 0); 
        }

        private void DrawModelView()
        {
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
            if (_visualVoxelModel != null)
            {
                // draw each part with bounding box

                var state = _visualVoxelModel.VoxelModel.States[_visualVoxelModel.ActiveState];

                //DrawBox(state.BoundingBox);

                StatesRepository.ApplyRaster(_renderModelRasterId);
                
                

                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelParts;

                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerFrame.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerFrame.IsDirty = true;
                }

                // draw each part of the model
                for (int i = 0; i < state.PartsStates.Length; i++)
                {
                    var voxelModelPartState = state.PartsStates[i];

                    DrawBox(voxelModelPartState.BoundingBox);
                    
                    var vb = visualParts[i].VertexBuffers[voxelModelPartState.ActiveFrame];
                    var ib = visualParts[i].IndexBuffers[voxelModelPartState.ActiveFrame];

                    _voxelEffect.Begin();
                    _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                    _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_view * _projection);
                    _voxelEffect.CBPerFrame.IsDirty = true;
                    _voxelEffect.Apply();

                    vb.SetToDevice(0);
                    ib.SetToDevice(0);

                    if (model.Parts[i].ColorMapping != null)
                    {
                        _voxelEffect.CBPerFrame.Values.ColorMapping = model.Parts[i].ColorMapping.BlockColors;
                        _voxelEffect.CBPerFrame.IsDirty = true;
                    }

                    _voxelEffect.CBPerPart.Values.Transform = Matrix.Transpose(voxelModelPartState.Transform);
                    _voxelEffect.CBPerPart.IsDirty = true;
                    _voxelEffect.Apply();

                    _d3DEngine.Context.DrawIndexed(ib.IndicesCount, 0, 0);
                }
            }
        }

        private void DrawFrameEdit()
        {

        }

        public override void UnloadContent()
        {
            if (_boxVertexBuffer != null) _boxVertexBuffer.Dispose();
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
