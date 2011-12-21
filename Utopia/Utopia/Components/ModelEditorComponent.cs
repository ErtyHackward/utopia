﻿using System;
using System.Collections.Generic;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
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
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using S33M3Engines;
using UtopiaContent.Effects.Entities;

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
        private HLSLVoxelModel _voxelEffect;
        private MouseState _prevState;
        private int _renderRasterId;
        private int _renderModelRasterId;
        private int _blendStateId;
        private int _depthStateWithDepthId;

        // view parameters
        private ViewParameters _mainViewData;
        private ViewParameters _frameViewData;

        private ViewParameters _currentViewData;

        private Matrix _transform;
        private VisualVoxelModel _visualVoxelModel;

        private Matrix _projection;
        private Matrix _view;

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
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 0.5f, 100);
            _view = Matrix.LookAtLH(new Vector3(0,0,5), new Vector3(0,0,0), Vector3.UnitY);

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
            _renderRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription { CullMode = CullMode.None, FillMode = FillMode.Solid });
            _renderModelRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription { CullMode = CullMode.Back, FillMode = FillMode.Solid });

            var blendDescr = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (var i = 0; i < 8; i++)
            {
                blendDescr.RenderTarget[i].IsBlendEnabled = true;
                blendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = StatesRepository.AddBlendStates(blendDescr);

            _depthStateWithDepthId = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });

            _lines3DEffect = new HLSLColorLine(_d3DEngine, ClientSettings.EffectPack + @"Entities\ColorLine.hlsl", VertexPosition.VertexDeclaration);

            
            var ptList = new List<VertexPosition>();

            var color = Color.White;

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
            
            var dx = ((float)mouseState.X - _prevState.X) / 100;
            var dy = ((float)mouseState.Y - _prevState.Y) / 100;
            

            if (mouseState.MiddleButton == ButtonState.Pressed && _prevState.X != 0 && _prevState.Y != 0)
            {
                var keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
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
                case EditorMode.ModelLayout:
                    if (SelectedStateIndex != -1)
                    {
                        var bb = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].BoundingBox;
                        UpdateTransformMatrix(_currentViewData, bb);
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
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_view * _projection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
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

                var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex];

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

                // draw bounding boxes
                for (int i = 0; i < state.PartsStates.Length; i++)
                {
                    var voxelModelPartState = state.PartsStates[i];
                    DrawBox(voxelModelPartState.BoundingBox, i == _selectedPartIndex ? new Color4(0, 1, 0, 0.1f) : new Color4(1, 1, 1, 0.1f));
                }

                
            }
        }

        private void DrawFrameEdit()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelParts;

                StatesRepository.ApplyRaster(_renderModelRasterId);
                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerFrame.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerFrame.IsDirty = true;
                }


                var vb = visualParts[SelectedPartIndex].VertexBuffers[SelectedFrameIndex];
                var ib = visualParts[SelectedPartIndex].IndexBuffers[SelectedFrameIndex];

                _voxelEffect.Begin();
                _voxelEffect.CBPerFrame.Values.World = Matrix.Transpose(_transform);
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_view * _projection);
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

            }
        }

        public override void UnloadContent()
        {
            if (_boxVertexBuffer != null) _boxVertexBuffer.Dispose();
            _lines3DEffect.Dispose();

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
