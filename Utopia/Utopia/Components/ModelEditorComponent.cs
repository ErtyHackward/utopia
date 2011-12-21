using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Shared.Math;
using SharpDX;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Buffers;
using S33M3Engines.D3D.Effects.Basics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.InputManager;
using Utopia.Settings;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using S33M3Engines;
using UtopiaContent.Effects.Entities;
using ButtonState = S33M3Engines.InputHandler.MouseHelper.ButtonState;
using Control = Nuclex.UserInterface.Controls.Control;
using ListControl = Nuclex.UserInterface.Controls.Desktop.ListControl;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.Components
{
    /// <summary>
    /// Allows user to edit a voxel model in a visual way
    /// </summary>
    public partial class ModelEditorComponent : DrawableGameComponent
    {
        private readonly D3DEngine _d3DEngine;
        private HLSLColorLine _lines3DEffect;
        
        private VertexBuffer<VertexPosition> _boxVertexBuffer;
        private IndexBuffer<ushort> _boxIndexBuffer;

        private VertexBuffer<VertexPosition> _crosshairVertexBuffer;
        

        private HLSLVoxelModel _voxelEffect;
        private MouseState _prevState;

        // view parameters
        private ViewParameters _mainViewData;
        private ViewParameters _frameViewData;

        private ViewParameters _currentViewData;

        private Matrix _transform;
        private VisualVoxelModel _visualVoxelModel;

        private Matrix _viewProjection;

        private EditorMode _mode;
        private VoxelFrame _voxelFrame;
        
        private readonly Screen _screen;

        private ButtonControl _backButton;
        private WindowControl _toolsWindow;
        private WindowControl _modelNavigationWindow;

        private readonly List<Control> _controls = new List<Control>();
        private ListControl _statesList;
        private ListControl _partsList;
        private ListControl _framesList;

        private int _selectedFrameIndex;
        private int _selectedPartIndex;

        private bool _flipAxis;

        /// <summary>
        /// Gets current editor camera transformation
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
        
        /// <summary>
        /// Gets or sets model to edit
        /// </summary>
        public VisualVoxelModel VisualVoxelModel
        {
            get { return _visualVoxelModel; }
            set { 
                _visualVoxelModel = value;
                if (_visualVoxelModel != null && _statesList != null)
                {
                    // fill the lists
                    _statesList.Items.Clear();
                    _statesList.SelectedItems.Clear();

                    for (int i = 0; i < _visualVoxelModel.VoxelModel.States.Count; i++)
                    {
                        _statesList.Items.Add(i.ToString());
                    }
                    SelectedStateIndex = 0;
                    _statesList.SelectedItems.Add(SelectedStateIndex);


                    _partsList.Items.Clear();
                    _partsList.SelectedItems.Clear();

                    foreach (var voxelModelPart in _visualVoxelModel.VoxelModel.Parts)
                    {
                        _partsList.Items.Add(voxelModelPart.Name);
                    }
                    if (_selectedPartIndex != -1)
                        _partsList.SelectedItems.Add(_selectedPartIndex);
                }
            }
        }

        public int SelectedStateIndex { get; private set; }
        
        public int SelectedPartIndex
        {
            get { return _selectedPartIndex; }
            private set { 
                if(_selectedPartIndex != value)
                {
                    _selectedPartIndex = value;

                    // update frames list


                    _framesList.Items.Clear();

                    if (_selectedPartIndex != -1)
                    {

                        for (int i = 0; i < _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames.Count; i++)
                        {
                            _framesList.Items.Add(i.ToString());
                        }

                        _framesList.SelectedItems.Clear();
                        _framesList.SelectedItems.Add(0);
                    }
                }
            }
        }
        
        public int SelectedFrameIndex
        {
            get { return _selectedFrameIndex; }
            private set {
                _selectedFrameIndex = value;

                if (Mode == EditorMode.ModelLayout)
                {
                    _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex].ActiveFrame = (byte)_selectedFrameIndex;
                }
            }
        }

        /// <summary>
        /// Gets current editing frame in frame mode
        /// </summary>
        public VoxelFrame VoxelFrame
        {
            get { return _voxelFrame; }
        }

        /// <summary>
        /// Gets or sets current editor mode
        /// </summary>
        public EditorMode Mode
        {
            get { return _mode; }
            set {
                if (_mode != value)
                {
                    // frame edit have separate view transformation
                    if (value == EditorMode.FrameEdit)
                    {
                        _mainViewData = _currentViewData;
                        _currentViewData = _frameViewData;
                    }
                    else if (_mode == EditorMode.FrameEdit)
                    {
                        _frameViewData = _currentViewData;
                        _currentViewData = _mainViewData;
                    }
                    _mode = value;
                }
            }
        }

        

        #region Events

        /// <summary>
        /// Occurs when back button is pressed
        /// </summary>
        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// Creates new editor component
        /// </summary>
        /// <param name="d3DEngine"></param>
        /// <param name="screen"></param>
        public ModelEditorComponent(D3DEngine d3DEngine, Screen screen)
        {
            _d3DEngine = d3DEngine;
            _screen = screen;
            Transform = Matrix.Identity;

            var aspect = d3DEngine.ViewPort.Width / d3DEngine.ViewPort.Height;
            var projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 0.5f, 100);
            var view = Matrix.LookAtLH(new Vector3(0,0,5), new Vector3(0,0,0), Vector3.UnitY);

            _viewProjection = view * projection;

            _mainViewData.Scale = 0.1f;
            _currentViewData.Scale = 0.1f;
            _frameViewData.Scale = 0.1f;
            _d3DEngine.ViewPort_Updated += ViewportUpdated;
        }

        private void ViewportUpdated(Viewport port)
        {
            UpdateLayout();
        }

        public override void Initialize()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };
            
            _toolsWindow = CreateToolsWindow();
            _modelNavigationWindow = CreateNavigationWindow();

            _controls.Add(_modelNavigationWindow);
            //_controls.Add(_colorPaletteWindow);
            _controls.Add(_toolsWindow);

            base.Initialize();
        }

        public void UpdateLayout()
        {
            _backButton.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, _d3DEngine.ViewPort.Height - 30, 120, 24);
            _modelNavigationWindow.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, 0, 200, _d3DEngine.ViewPort.Height - 40);
            _modelNavigationWindow.UpdateLayout();
        }

        protected override void OnEnabledChanged()
        {
            if (!IsInitialized) return;

            if (Enabled)
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Add(control);
                }

                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout();

                // call property setter to fill the lists
                if(_visualVoxelModel != null)
                    VisualVoxelModel = _visualVoxelModel;
            }
            else
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Remove(control);
                }

                _screen.Desktop.Children.Remove(_backButton);
            }

            base.OnEnabledChanged();
        }
        
        public override void LoadContent()
        {
            _lines3DEffect = new HLSLColorLine(_d3DEngine, ClientSettings.EffectPack + @"Entities\ColorLine.hlsl", VertexPosition.VertexDeclaration);
            
            var ptList = new List<VertexPosition>();
            

            var topLeftFront        = new VertexPosition(new Vector3(0, 1, 1));
            var topLeftBack         = new VertexPosition(new Vector3(0, 1, 0));
            var topRightFront       = new VertexPosition(new Vector3(1, 1, 1));
            var topRightBack        = new VertexPosition(new Vector3(1, 1, 0));
            var bottomLeftFront     = new VertexPosition(new Vector3(0, 0, 1));
            var bottomLeftBack      = new VertexPosition(new Vector3(0, 0, 0));
            var bottomRightFront    = new VertexPosition(new Vector3(1, 0, 1));
            var bottomRightBack     = new VertexPosition(new Vector3(1, 0, 0));

            ptList.Add(topLeftFront);       // 0
            ptList.Add(topLeftBack);        // 1
            ptList.Add(topRightFront);      // 2
            ptList.Add(topRightBack);       // 3
            ptList.Add(bottomLeftFront);    // 4
            ptList.Add(bottomLeftBack);     // 5
            ptList.Add(bottomRightFront);   // 6
            ptList.Add(bottomRightBack);    // 7

            var indices = new ushort[] { 0, 1, 1, 3, 3, 2, 2, 0, 4, 5, 5, 7, 7, 6, 6, 4, 0, 4, 2, 6, 1, 5, 3, 7 };

            _boxVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, 8, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorBox_vertexBuffer");
            _boxVertexBuffer.SetData(ptList.ToArray());

            _boxIndexBuffer = new IndexBuffer<ushort>(_d3DEngine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "EditorBox_indexBuffer");
            _boxIndexBuffer.SetData(indices);

            ptList.Clear();

            ptList.Add(new VertexPosition(new Vector3(0,0,-1)));
            ptList.Add(new VertexPosition(new Vector3(0, 0, 1)));
            ptList.Add(new VertexPosition(new Vector3(0, -1, 0)));
            ptList.Add(new VertexPosition(new Vector3(0, 1, 0)));

            _crosshairVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, 4, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorCrosshair_vertexBuffer");
            _crosshairVertexBuffer.SetData(ptList.ToArray());
            
            _voxelEffect = new HLSLVoxelModel(_d3DEngine, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="timeSpent">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime timeSpent)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            _flipAxis = keyboardState.IsKeyDown(Keys.ShiftKey);

            var dx = ((float)mouseState.X - _prevState.X) / 100;
            var dy = ((float)mouseState.Y - _prevState.Y) / 100;
            

            if (mouseState.MiddleButton == ButtonState.Pressed && _prevState.X != 0 && _prevState.Y != 0)
            {
                

                if (keyboardState.IsKeyDown(Keys.ShiftKey))
                {
                    // translate
                    _currentViewData.Translate.X -= dx;
                    _currentViewData.Translate.Y -= dy;
                }
                else
                {
                    // rotate
                    _currentViewData.RotateX += dx;
                    _currentViewData.RotateY += dy;
                }
            }

            _currentViewData.Scale -= ((float)_prevState.ScrollWheelValue - mouseState.ScrollWheelValue) / 10000;

            switch (Mode)
            {
                case EditorMode.ModelView:
                    if (SelectedStateIndex != -1)
                    {
                        var bb = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].BoundingBox;
                        UpdateTransformMatrix(_currentViewData, bb);
                    }
                    break;
                case EditorMode.ModelLayout:
                    if (SelectedStateIndex != -1)
                    {
                        var bb = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].BoundingBox;
                        UpdateTransformMatrix(_currentViewData, bb);

                        if (_selectedPartIndex != -1)
                        {
                            if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                var translationVector = _flipAxis ? new Vector3(-dx, -dy, 0) : new Vector3(0, -dy, -dx);
                                // send translation to current state
                                var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex];
                                var translationMatrix = Matrix.Translation(translationVector);
                                state.Transform *= translationMatrix;
                                state.BoundingBox = new BoundingBox(Vector3.TransformCoordinate(state.BoundingBox.Minimum, translationMatrix), Vector3.TransformCoordinate(state.BoundingBox.Maximum, translationMatrix));
                            }
                        }
                    }
                    break;
                case EditorMode.FrameEdit:
                    if (_selectedPartIndex != -1 && _selectedFrameIndex != -1)
                    {
                        var frame = _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames[_selectedFrameIndex];
                        var box = new BoundingBox(new Vector3(), frame.BlockData.ChunkSize);
                        UpdateTransformMatrix(_currentViewData, box);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _prevState = mouseState;

            base.Update(ref timeSpent);
        }

        private void UpdateTransformMatrix(ViewParameters parameters, BoundingBox modelBoundingBox)
        {
            var translateVector = Vector3.Negate(Vector3.Subtract(modelBoundingBox.Maximum, modelBoundingBox.Minimum) / 2);

            var translation = Matrix.Translation(translateVector);
            var rotationX = Matrix.RotationX(parameters.RotateY);
            _transform = translation * rotationX;
            var axis2 = Vector3.TransformCoordinate(Vector3.UnitY, rotationX);
            _transform = _transform * Matrix.RotationAxis(axis2, -parameters.RotateX);
            _transform = _transform * Matrix.Scaling(parameters.Scale);
            _transform *= Matrix.Translation(parameters.Translate);
        }

        private float GetBestScale(BoundingBox bb)
        {
            var size = Vector3.Negate(Vector3.Subtract(bb.Maximum, bb.Minimum));

            var maxValue = Math.Max(Math.Max(size.X, size.Y), size.Z);

            return 2f / maxValue;
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
            DrawBox(box, new Color4(1, 1, 1, 1));
        }

        private void DrawBox(BoundingBox box, Color4 color)
        {
            DrawBox(box.Minimum, box.Maximum, color);
        }

        private void DrawBox(Vector3 min, Vector3 max, Color4 color)
        {
            var size = Vector3.Subtract(max, min);
            //StatesRepository.ApplyStates(_renderRasterId, _blendStateId, _depthStateWithDepthId);
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            //Set Effect variables
            _lines3DEffect.Begin();
            _lines3DEffect.CBPerDraw.Values.Color = color;
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(size) * Matrix.Translation(min) * _transform); //Matrix.Translation(new Vector3(-0.5f,-0.5f,-0.5f)) *
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            //Set the vertex buffer to the Graphical Card.
            _boxVertexBuffer.SetToDevice(0);
            _boxIndexBuffer.SetToDevice(0);

            _d3DEngine.Context.DrawIndexed(24, 0, 0); 
        }

        private void DrawCrosshair(Vector3 position, float size, bool turnAxis)
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            var transform = Matrix.Scaling(size) * Matrix.Translation(position) * _transform;

            if (turnAxis)
            {
                transform = Matrix.RotationY((float)Math.PI / 2) * transform;
            }

            //Set Effect variables
            _lines3DEffect.Begin();
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(0,1,0,1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transform);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            //Set the vertex buffer to the Graphical Card.
            _crosshairVertexBuffer.SetToDevice(0);
            
            _d3DEngine.Context.Draw(4, 0); 
        }

        private void DrawModelView()
        {
            // draw the model
            if (_visualVoxelModel != null)
            {
                StatesRepository.ApplyRaster(GameDXStates.DXStates.Rasters.Default);

                _voxelEffect.Begin();
                _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
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

                var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex];

                //DrawBox(state.BoundingBox);

                StatesRepository.ApplyRaster(GameDXStates.DXStates.Rasters.Default);
                
                

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
                    
                    var vb = visualParts[i].VertexBuffers[voxelModelPartState.ActiveFrame];
                    var ib = visualParts[i].IndexBuffers[voxelModelPartState.ActiveFrame];

                    _voxelEffect.Begin();
                    _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                    _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
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

                // draw bounding boxes
                for (int i = 0; i < state.PartsStates.Length; i++)
                {
                    var voxelModelPartState = state.PartsStates[i];
                    DrawBox(voxelModelPartState.BoundingBox, i == _selectedPartIndex ? new Color4(0, 1, 0, 0.1f) : new Color4(1, 1, 1, 0.1f));

                    if (i == _selectedPartIndex)
                    {
                        var bb = voxelModelPartState.BoundingBox;
                        var size = Vector3.Subtract(bb.Maximum, bb.Minimum);
                        var sizef = Math.Max(Math.Max(size.X, size.Y), size.Z);
                        
                        var center = new Vector3((bb.Maximum.X - bb.Minimum.X)/2,(bb.Maximum.Y - bb.Minimum.Y)/2,(bb.Maximum.Z - bb.Minimum.Z)/2);

                        var translate = voxelModelPartState.Transform.TranslationVector + center;

                        DrawCrosshair(translate, sizef, _flipAxis); 
                    }
                }

                
            }
        }

        private void DrawFrameEdit()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelParts;

                StatesRepository.ApplyRaster(GameDXStates.DXStates.Rasters.Default);
                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerFrame.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerFrame.IsDirty = true;
                }


                var vb = visualParts[SelectedPartIndex].VertexBuffers[SelectedFrameIndex];
                var ib = visualParts[SelectedPartIndex].IndexBuffers[SelectedFrameIndex];

                _voxelEffect.Begin();
                _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply();

                vb.SetToDevice(0);
                ib.SetToDevice(0);

                if (model.Parts[SelectedPartIndex].ColorMapping != null)
                {
                    _voxelEffect.CBPerFrame.Values.ColorMapping = model.Parts[SelectedPartIndex].ColorMapping.BlockColors;
                    _voxelEffect.CBPerFrame.IsDirty = true;
                }

                _voxelEffect.CBPerPart.Values.Transform = Matrix.Transpose(Matrix.Identity);
                _voxelEffect.CBPerPart.IsDirty = true;
                _voxelEffect.Apply();

                _d3DEngine.Context.DrawIndexed(ib.IndicesCount, 0, 0);

                Vector3I selectedCube;
                if (GetSelectedCube(out selectedCube))
                {
                    // draw selected cube
                    DrawBox(new BoundingBox(selectedCube, selectedCube + Vector3I.One), new Color4(1, 0, 0, 1));
                }
            }
        }

        private bool GetSelectedCube(out Vector3I cubePosition)
        {
            if (_selectedPartIndex == -1 || _selectedFrameIndex == -1)
            {
                cubePosition = new Vector3I();
                return false;
            }

            Vector3D mPosition, mLookAt;
            InputsManager.UnprojectMouseCursor(_d3DEngine, ref _viewProjection, out mPosition, out mLookAt);

            var blocks = _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames[_selectedFrameIndex].BlockData;

            for (float i = 0; i < 100; i += 0.1f)
            {
                var targetPoint = (Vector3I)(mPosition + (mLookAt * i));
                if (blocks.GetBlock(targetPoint) != 0)
                {
                    cubePosition = targetPoint;
                    return true;
                }
            }

            cubePosition = new Vector3I();
            return false;
        }

        public override void UnloadContent()
        {
            if (_boxVertexBuffer != null) _boxVertexBuffer.Dispose();
            if (_boxIndexBuffer != null) _boxIndexBuffer.Dispose();
            if (_crosshairVertexBuffer != null) _crosshairVertexBuffer.Dispose();
            _lines3DEffect.Dispose();
            _voxelEffect.Dispose();

            base.UnloadContent();
        }
    }

    public struct ViewParameters
    {
        public float RotateX;
        public float RotateY;
        public float Scale;
        public Vector3 Translate;
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
