using System;
using System.Collections.Generic;
using System.Windows.Forms;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Shared.Math;
using S33M3Engines.Sprites;
using SharpDX;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Buffers;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using Utopia.InputManager;
using Utopia.Settings;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using S33M3Engines;
using UtopiaContent.Effects.Entities;
using ButtonState = S33M3Engines.InputHandler.MouseHelper.ButtonState;
using Control = Nuclex.UserInterface.Controls.Control;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.Components
{
    /// <summary>
    /// Allows user to edit a voxel model in a visual way
    /// </summary>
    public partial class ModelEditorComponent : DrawableGameComponent
    {
        #region Fields

        private readonly D3DEngine _d3DEngine;
        private HLSLColorLine _lines3DEffect;
        private SpriteFont _font;

        private VertexBuffer<VertexPosition> _boxVertexBuffer;
        private IndexBuffer<ushort> _boxIndexBuffer;

        private Vector3I _gridSize;
        private VertexBuffer<VertexPosition> _xGridVertextBuffer;
        private VertexBuffer<VertexPosition> _yGridVertextBuffer;
        private VertexBuffer<VertexPosition> _zGridVertextBuffer;

        private Vector3 _gridBackNormal = new Vector3(0, 0, 1);
        private Vector3 _gridFrontNormal = new Vector3(0, 0, -1);
        private Vector3 _gridTopNormal = new Vector3(0, -1, 0);
        private Vector3 _gridBottomNormal = new Vector3(0, 1, 0);
        private Vector3 _gridLeftNormal = new Vector3(1, 0, 0);
        private Vector3 _gridRightNormal = new Vector3(-1, 0, 0);

        private Plane _gridBackPlane;
        private Plane _gridFrontPlane;
        private Plane _gridTopPlane;
        private Plane _gridBottomPlane;
        private Plane _gridLeftPlane;
        private Plane _gridRightPlane;

        private Plane[] _gridPlanes;


        private VertexBuffer<VertexPosition> _crosshairVertexBuffer;

        private HLSLVoxelModel _voxelEffect;
        private MouseState _prevState;

        // view parameters
        private ViewParameters _mainViewData;
        private ViewParameters _frameViewData;

        private ViewParameters _currentViewData;
        private Vector2 _accumulatedPosition;

        private Matrix _transform;
        private VisualVoxelModel _visualVoxelModel;

        private Matrix _viewProjection;
        private Matrix _view;

        private EditorMode _mode;
        private VoxelFrame _voxelFrame;

        private readonly Screen _screen;
        private readonly VoxelModelManager _manager;
        private readonly VoxelMeshFactory _meshFactory;
        private readonly GuiManager _gui;
        private readonly ActionsManager _actions;
        private readonly List<Control> _controls = new List<Control>();

        private int _selectedFrameIndex;
        private int _selectedPartIndex;
        private int _selectedAnimationIndex;

        private bool _flipAxis;

        private Vector3I? _pickedCube;
        private Vector3I? _newCube;

        private int _selectedColorIndex;
        private int _selectedToolIndex;

        /// <summary>
        /// Provides a plane for a part translating
        /// </summary>
        private Plane _translatePlane;
        private bool _drawGround;
        private Vector3? _translatePoint;

        private bool _needSave;
        private VoxelModelInstance _instance;

        #endregion

        #region Properties

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
            set
            {
                _visualVoxelModel = value;
                if (_visualVoxelModel != null && _statesList != null)
                {
                    // fill the lists
                    _statesList.Items.Clear();
                    _statesList.SelectedItems.Clear();

                    foreach (var state in _visualVoxelModel.VoxelModel.States)
                    {
                        _statesList.Items.Add(state);
                    }
                    SelectedStateIndex = 0;
                    _statesList.SelectedItems.Add(SelectedStateIndex);


                    _partsList.Items.Clear();
                    _partsList.SelectedItems.Clear();

                    foreach (var voxelModelPart in _visualVoxelModel.VoxelModel.Parts)
                    {
                        _partsList.Items.Add(voxelModelPart);
                    }
                    if (_selectedPartIndex != -1)
                        _partsList.SelectedItems.Add(_selectedPartIndex);

                    _animationsList.Items.Clear();
                    _animationsList.SelectedItems.Clear();

                    foreach (var animation in _visualVoxelModel.VoxelModel.Animations)
                    {
                        _animationsList.Items.Add(animation);
                    }

                    UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, 0);

                    _instance = _visualVoxelModel.VoxelModel.CreateInstance();
                }
            }
        }

        public int SelectedStateIndex { get; private set; }

        public int SelectedPartIndex
        {
            get { return _selectedPartIndex; }
            private set
            {
                if (_selectedPartIndex != value)
                {
                    _selectedPartIndex = value;

                    // update frames list


                    _framesList.Items.Clear();

                    if (_selectedPartIndex != -1)
                    {

                        for (int i = 0; i < _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames.Count; i++)
                        {
                            var frame = _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames[i];
                            _framesList.Items.Add(frame);
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
            private set
            {
                _selectedFrameIndex = value;

                if (Mode == EditorMode.ModelLayout)
                {
                    _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex].ActiveFrame
                        = (byte) _selectedFrameIndex;
                }
            }
        }

        public int SelectedAnimationIndex
        {
            get { return _selectedAnimationIndex; }
            private set
            {
                if (_selectedAnimationIndex != value)
                {
                    _selectedAnimationIndex = value;

                    // update steps list

                    _animationStepsList.Items.Clear();

                    if (_selectedAnimationIndex != -1)
                    {

                        for (int i = 0;
                             i < _visualVoxelModel.VoxelModel.Animations[_selectedAnimationIndex].Steps.Count;
                             i++)
                        {
                            var step = _visualVoxelModel.VoxelModel.Animations[_selectedAnimationIndex].Steps[i];
                            _animationStepsList.Items.Add(step);
                        }

                        _animationStepsList.SelectedItems.Clear();
                        _animationStepsList.SelectedItems.Add(0);
                    }
                }

            }
        }

        public int SelectedAnimationStepIndex { get; private set; }


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
            set
            {
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
        
        #endregion

        #region Events

        /// <summary>
        /// Occurs when back button is pressed
        /// </summary>
        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            if (_needSave)
            {
                AskModelSave(OnBackModelSave);
                return;
            }

            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnBackModelSave(string button)
        {
            if (button == "Save")
            {
                _manager.SaveModel(VisualVoxelModel);
            }
            _needSave = false;
            OnBackPressed();
        }

        #endregion

        /// <summary>
        /// Creates new editor component
        /// </summary>
        /// <param name="d3DEngine"></param>
        /// <param name="screen"></param>
        /// <param name="manager"> </param>
        /// <param name="meshFactory"> </param>
        /// <param name="gui"> </param>
        public ModelEditorComponent(D3DEngine d3DEngine, Screen screen, VoxelModelManager manager, VoxelMeshFactory meshFactory, GuiManager gui)
        {
            _d3DEngine = d3DEngine;
            _screen = screen;
            _manager = manager;
            _meshFactory = meshFactory;
            _gui = gui;
            Transform = Matrix.Identity;
            
            var aspect = d3DEngine.ViewPort.Width / d3DEngine.ViewPort.Height;
            var projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 1f, 100);
            _view = Matrix.LookAtLH(new Vector3(0, 0, 5), new Vector3(0, 0, 0), Vector3.UnitY);

            _viewProjection = _view * projection;

            _mainViewData.Scale = 0.1f;
            _currentViewData.Scale = 0.1f;
            _frameViewData.Scale = 0.1f;
            _d3DEngine.ViewPort_Updated += ViewportUpdated;

            DrawOrders.UpdateIndex(0, 15);
        }

        private void InitPlanes(Vector3I chunkSize)
        {
            chunkSize = new Vector3I() - chunkSize;
            _gridBackPlane = new Plane(new Vector3(), _gridBackNormal);
            _gridFrontPlane = new Plane(chunkSize, _gridFrontNormal);
            _gridTopPlane = new Plane(chunkSize, _gridTopNormal);
            _gridBottomPlane = new Plane(new Vector3(), _gridBottomNormal);
            _gridLeftPlane = new Plane(new Vector3(), _gridLeftNormal);
            _gridRightPlane = new Plane(chunkSize, _gridRightNormal);

            _gridPlanes = new[] { _gridBackPlane, _gridFrontPlane, _gridTopPlane, _gridBottomPlane, _gridLeftPlane, _gridRightPlane };
        }

        private void ViewportUpdated(Viewport port)
        {
            UpdateLayout();
        }

        public override void Initialize()
        {
            InitializeGui();

            foreach (var model in _manager.Enumerate())
            {
                _modelsList.Items.Add(model);
            }
            

            base.Initialize();
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
            _font = new SpriteFont();
            _font.Initialize("Tahoma", 13f, System.Drawing.FontStyle.Regular, true, _d3DEngine.Device);

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

            ptList.Add(new VertexPosition(new Vector3(0,  0, -1)));
            ptList.Add(new VertexPosition(new Vector3(0,  0,  1)));
            ptList.Add(new VertexPosition(new Vector3(0, -1,  0)));
            ptList.Add(new VertexPosition(new Vector3(0,  1,  0)));

            _crosshairVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, 4, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorCrosshair_vertexBuffer");
            _crosshairVertexBuffer.SetData(ptList.ToArray());

            ptList.Clear();
            ptList.Add(new VertexPosition(new Vector3(0, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3(1, 0, 0)));

            _voxelEffect = new HLSLVoxelModel(_d3DEngine, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            

            base.LoadContent();
        }

        #region Buttons handlers

        private void OnModelsAddPressed()
        {
            _modelEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogModelEditStruct(), "Add a new model", OnModelAdded);
        }
        private void OnModelAdded(DialogModelEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "rename_me";
            var model = new VisualVoxelModel(new VoxelModel { Name = e.Name }, _meshFactory);
            var voxelModelState = new VoxelModelState(model.VoxelModel);
            voxelModelState.Name = "Default";
            model.VoxelModel.States.Add(voxelModelState);
            model.VoxelModel.ColorMapping = new ColorMapping { BlockColors = new Color4[64] };

            // set some initial colors
            ColorLookup.Colours.CopyTo(model.VoxelModel.ColorMapping.BlockColors,0);            

            // add default part

            var part = new VoxelModelPart { Name = "Main" };
            part.Frames.Add(new VoxelFrame(new Vector3I(16, 16, 16)));
            voxelModelState.PartsStates.Add(new VoxelModelPartState { Transform = Matrix.Identity });


            model.VoxelModel.Parts.Add(part);
            model.BuildMesh();
            _partsList.Items.Add(part);


            _modelsList.Items.Add(model);
            _modelsList.SelectedItems.Clear();
            _modelsList.SelectedItems.Add(_modelsList.Items.Count - 1);

            VisualVoxelModel = model;
            _needSave = true;
        }

        private void OnPartsAddPressed()
        {
            if (_visualVoxelModel == null)
            {
                _gui.MessageBox("Please select or create a model to edit");
                return;
            }

            _partEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogPartsEditStruct(), "Add a new part", OnPartAdded);
        }
        private void OnPartAdded(DialogPartsEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "noname part";

            var part = new VoxelModelPart { Name = e.Name };

            part.Name = e.Name;
            part.IsHead = e.IsHead;
            part.IsArm = e.IsArm;

            part.Frames.Add(new VoxelFrame(new Vector3I(16,16,16)));

            foreach (var voxelModelState in _visualVoxelModel.VoxelModel.States)
            {
                voxelModelState.PartsStates.Add(new VoxelModelPartState { Transform = Matrix.Identity });
            }

            _visualVoxelModel.VoxelModel.Parts.Add(part);
            _visualVoxelModel.BuildMesh();
            _partsList.Items.Add(part);

            _instance.UpdateStates();
        }

        private void OnPartsEditPressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part to edit");
                return;
            }

            var s = new DialogPartsEditStruct();

            var part = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex];

            s.Name = part.Name;
            s.IsHead = part.IsHead;
            s.IsArm = part.IsArm;

            _partEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, s, "Add a new part", OnPartEdited);

        }
        private void OnPartEdited(DialogPartsEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "noname part";

            var part = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex];

            part.Name = e.Name;
            part.IsHead = e.IsHead;
            part.IsArm = e.IsArm;

            //_partsList.Items[SelectedPartIndex] = part;
        }

        private void OnPartsDeletePressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part to delete");
                return;
            }

            _visualVoxelModel.VoxelModel.Parts.RemoveAt(SelectedPartIndex);

            foreach (var voxelModelState in _visualVoxelModel.VoxelModel.States)
            {
                voxelModelState.PartsStates.RemoveAt(SelectedPartIndex);
            }

            _visualVoxelModel.RemovePartAt(SelectedPartIndex);

        }

        private void OnFrameAddPressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part to add a frame to");
                return;
            }

            _frameEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogFrameEditStruct { SizeX = 16, SizeY = 16, SizeZ = 16 }, "Add a new frame", OnFrameAdded);
        }
        private void OnFrameAdded(DialogFrameEditStruct e)
        {
            if (e.SizeX < 1 || e.SizeY < 1 || e.SizeZ < 1 || e.SizeX > 128 || e.SizeY > 128 || e.SizeZ > 128)
            {
                _gui.MessageBox("Each part of frame size should be in range [1;128]");
                return;
            }
            var frame = new VoxelFrame(new Vector3I(e.SizeX, e.SizeY, e.SizeZ));
            _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames.Add(frame);
            _visualVoxelModel.BuildMesh();
            _framesList.Items.Add(frame);
        }
        
        private void OnFrameEditPressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part and frame to delete");
                return;
            }
            if (SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Please select a frame to edit");
                return;
            }

            var frame = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames[SelectedFrameIndex];
            var size = frame.BlockData.ChunkSize;
            var args = new DialogFrameEditStruct {
                SizeX = size.X, 
                SizeY = size.Y, 
                SizeZ = size.Z
            };
            
            _frameEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, args, "Edit frame size", OnFrameEdited);
        }
        private void OnFrameEdited(DialogFrameEditStruct e)
        {
            var frame = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames[SelectedFrameIndex];

            frame.BlockData.UpdateChunkSize(new Vector3I(e.SizeX, e.SizeY, e.SizeZ), true);
        }

        private void OnFrameDeletePressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part and frame to delete");
                return;
            }
            if (SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Please select a frame to delete");
                return;
            }
            if (_visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames.Count == 1)
            {
                _gui.MessageBox("Model part must have at least one frame");
                return;
            }

        }

        private void OnColorAddPressed()
        {
            if (_colorPalette.Count == 64)
            {
                _gui.MessageBox("Sorry the maximum number of the colors is 64, consider changing one of existing colors");
                return;
            }

            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    _visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_colorPalette.Count] = colorDialog.Color;
                    UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, _selectedColorIndex);
                }
            }
            GuiManager.DialogClosed = true;

        }

        private void OnColorChangePressed()
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = _visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_selectedColorIndex];
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    _visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_selectedColorIndex] = colorDialog.Color;
                    UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, _selectedColorIndex);
                    _visualVoxelModel.BuildMesh();
                }
                GuiManager.DialogClosed = true;
            }
        }

        private void OnColorRemPressed()
        {
            if (_colorPalette.Count == 1)
            {
                _gui.MessageBox("The model should have at least one color");
                return;
            }
            var array = _visualVoxelModel.VoxelModel.ColorMapping.BlockColors;

            if (_selectedColorIndex != 63)
            {
                Array.Copy(array, _selectedColorIndex + 1, array, _selectedColorIndex, 63 - _selectedColorIndex);
            }
            array[63] = new Color4();
            UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, _selectedColorIndex);

            // shift all blocks to save old colors
            foreach (var voxelModelPart in _visualVoxelModel.VoxelModel.Parts)
            {
                foreach (var voxelFrame in voxelModelPart.Frames)
                {
                    var buffer = voxelFrame.BlockData.BlockBytes;
                    if (buffer == null) continue;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] == _selectedColorIndex)
                            buffer[i] = 1;
                        else if (buffer[i] > _selectedColorIndex)
                        {
                            buffer[i]--;
                        }
                    }
                }
            }

            _visualVoxelModel.BuildMesh();
        }

        private void OnModelsSelected()
        {
            var newModel = (VisualVoxelModel)_modelsList.SelectedItem;

            if (newModel != VisualVoxelModel && _needSave)
            {
                AskModelSave(OnModelSaveConfirm);
                return;
            }

            VisualVoxelModel = newModel;
            if (VisualVoxelModel != null && !VisualVoxelModel.Initialized)
            {
                VisualVoxelModel.BuildMesh();
            }

        }

        private void AskModelSave(Action<string> callback)
        {
            _gui.MessageBox("Current model was modified. Would you like to save the changes?", "Confirm", new[] { "Save", "Drop changes" }, callback );
        }

        private void OnModelSaveConfirm(string button)
        {
            if (button == "Save")
            {
                _manager.SaveModel(VisualVoxelModel);
            }
            _needSave = false;
            var newModel = (VisualVoxelModel)_modelsList.SelectedItem;
            VisualVoxelModel = newModel;
        }

        private void OnStateAddButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before add a state");
                return;
            }

            _stateEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogStateEditStruct(), "Add a new state", OnStateAdded);
            
        }
        private void OnStateAdded(DialogStateEditStruct e)
        {
            var vms = new VoxelModelState(VisualVoxelModel.VoxelModel);
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "unnamed";
            vms.Name = e.Name;
            VisualVoxelModel.VoxelModel.States.Add(vms);
            _statesList.Items.Add(vms);
        }
        
        private void OnStateEditButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before add a state");
                return;
            }

            if (SelectedStateIndex == -1)
            {
                _gui.MessageBox("Select a state to edit");
                return;
            }

            var state = VisualVoxelModel.VoxelModel.States[SelectedStateIndex];

            _stateEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogStateEditStruct { Name = state.Name }, "State edit", OnStateEdited);
        }
        private void OnStateEdited(DialogStateEditStruct e)
        {
            var state = VisualVoxelModel.VoxelModel.States[SelectedStateIndex];
            state.Name = e.Name;
        }

        private void OnStateDeleteButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before delete");
                return;
            }
            if (VisualVoxelModel.VoxelModel.States.Count == 1)
            {
                _gui.MessageBox("Model should have at least one state");
                return;
            }

            _statesList.Items.RemoveAt(SelectedStateIndex);
            VisualVoxelModel.VoxelModel.RemoveStateAt(SelectedStateIndex);
        }

        private void OnAnimationsAddButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before add a state");
                return;
            }

            _animationsEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogAnimationEditStruct(), "Add a new animation", OnAnimationAdded);
        }
        private void OnAnimationAdded(DialogAnimationEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "unnamed";
            
            var animation = new VoxelModelAnimation();
            
            animation.Name = e.Name;

            VisualVoxelModel.VoxelModel.Animations.Add(animation);
            _animationsList.Items.Add(animation);
        }
        
        private void OnAnimationsEditButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before edit");
                return;
            }

            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to edit");
                return;
            }

            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];

            _animationsEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogAnimationEditStruct { Name = animation.Name }, "Animation edit", OnAnimationEdited);
        }
        private void OnAnimationEdited(DialogAnimationEditStruct e)
        {
            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];
            animation.Name = e.Name;
        }

        private void OnAnimationsDeleteButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before delete");
                return;
            }

            VisualVoxelModel.VoxelModel.Animations.RemoveAt(SelectedAnimationIndex);
            _animationsList.Items.RemoveAt(SelectedAnimationIndex);

        }

        private void OnAnimationStepAddButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before add a state");
                return;
            }
            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to add step to");
                return;
            }
            var e = new DialogAnimationStepStruct();
            e.State = new DialogSelection { SelectedIndex = -1, Elements = VisualVoxelModel.VoxelModel.States };

            _animationStepDialog.ShowDialog(_screen, _d3DEngine.ViewPort, e, "Add a new animation step", OnAnimationStepAdded);
        }
        private void OnAnimationStepAdded(DialogAnimationStepStruct e)
        {
            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];

            var step = new AnimationStep { Duration = e.Duration, StateIndex = (byte)e.State.SelectedIndex };
            animation.Steps.Add(step);
            _animationStepsList.Items.Add(step);
        }

        private void UpdateAnimationStepsList()
        {
            _animationStepsList.Items.Clear();
            foreach (var animationStep in VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex].Steps)
            {
                _animationStepsList.Items.Add(animationStep);
            }
        }

        private void OnAnimationStepEditButtonPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Select a model before edit");
                return;
            }

            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to edit");
                return;
            }

            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];
            var step = animation.Steps[SelectedAnimationStepIndex];

            _animationStepDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogAnimationStepStruct { Duration = step.Duration, State = new DialogSelection { SelectedIndex = step.StateIndex, Elements = VisualVoxelModel.VoxelModel.States } }, "Step edit", OnAnimationStepEdited);
        }
        private void OnAnimationStepEdited(DialogAnimationStepStruct e)
        {
            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];
            var step = animation.Steps[SelectedAnimationStepIndex];
            step.Duration = e.Duration;
            step.StateIndex = (byte)e.State.SelectedIndex;
            animation.Steps[SelectedAnimationStepIndex] = step;
            UpdateAnimationStepsList();
        }

        private void OnAnimationStepDeleteButtonPressed()
        {
            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to edit");
                return;
            }

            if (SelectedAnimationStepIndex == -1)
            {
                _gui.MessageBox("Select an animation step to edit");
                return;
            }

            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];
            animation.Steps.RemoveAt(SelectedAnimationStepIndex);
            _animationStepsList.Items.RemoveAt(SelectedAnimationStepIndex);

            if (_animationStepsList.Items.Count == 0)
            {
                SelectedAnimationStepIndex = -1;
            }

            if (SelectedAnimationStepIndex == _animationStepsList.Items.Count)
            {
                SelectedAnimationIndex--;
            }

        }

        private void OnAnimationPlayButtonPressed()
        {
            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to play");
                return;
            }
            _instance.Play(SelectedAnimationIndex);
        }

        private void OnAnimationStopButtonPressed()
        {
            _instance.Stop();
        }

        #region Presets
        private void OnCubePresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames[SelectedFrameIndex].BlockData;

                if (frame.BlockBytes == null)
                    frame.SetBlock(new Vector3I(), 0);

                for (int i = 0; i < frame.BlockBytes.Length; i++)
                {
                    frame.BlockBytes[i] = (byte)(_selectedColorIndex + 1);
                }

                RebuildFrameVertices();
            }
        }

        private void OnSpherePresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames[SelectedFrameIndex].BlockData;

                // clear everything
                frame.SetBlockBytes(new byte[frame.ChunkSize.X * frame.ChunkSize.Y * frame.ChunkSize.Z]);

                var center =  (Vector3)frame.ChunkSize / 2;
                var radius = Math.Min(Math.Min(center.X, center.Y), center.Z);

                for (int x = 0; x < frame.ChunkSize.X; x++)
                {
                    for (int y = 0; y < frame.ChunkSize.Y; y++)
                    {
                        for (int z = 0; z < frame.ChunkSize.Z; z++)
                        {
                            var point = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                            if (Vector3.Distance(point, center) <= radius)
                                frame.SetBlock((Vector3I)point, (byte)(_selectedColorIndex + 1));
                        }
                    }
                }

                RebuildFrameVertices();
            }
        }
        #endregion

        private void OnToolSelected(int toolIndex)
        {
            for (int i = 0; i < _toolsButtons.Count; i++)
            {
                if (i != toolIndex)
                {
                    _toolsButtons[i].Release();
                }
                else _toolsButtons[i].Sticked = true;
            }

            _selectedToolIndex = toolIndex;
        }

        #endregion

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd, ref long timePassed)
        {
            if (_instance != null)
                _instance.Update(ref timePassed);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="timeSpent">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime timeSpent)
        {
            if (_visualVoxelModel == null || !_d3DEngine.HasFocus || DialogHelper.DialogBg.Parent != null) return;

            if (GuiManager.DialogClosed)
                _prevState = Mouse.GetState();

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
                            var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex];
                            var partState = state.PartsStates[_selectedPartIndex];
                            bb = partState.BoundingBox;
                            var center = new Vector3((bb.Maximum.X - bb.Minimum.X)/2,(bb.Maximum.Y - bb.Minimum.Y)/2,(bb.Maximum.Z - bb.Minimum.Z)/2) + partState.Transform.TranslationVector;
                            _translatePlane = new Plane(center, _flipAxis ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0));

                            if (mouseState.LeftButton == ButtonState.Released)
                            {
                                _translatePoint = null;
                                // recalculate state bounding box
                                state.UpdateBoundingBox();
                            }

                            if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                Vector3D mPosition, mLookAt;
                                var worldViewProjection = _transform * _viewProjection;
                                InputsManager.UnprojectMouseCursor(_d3DEngine, ref worldViewProjection, out mPosition, out mLookAt);
                                var r = new Ray(mPosition.AsVector3(), mLookAt.AsVector3());

                                Vector3 intersectPoint;
                                var intersects = r.Intersects(ref _translatePlane, out intersectPoint);

                                if (_translatePoint == null)
                                {
                                    if (intersects)
                                    {
                                        _translatePoint = intersectPoint;
                                    }
                                    else return;
                                }

                                var translationVector = intersectPoint - _translatePoint.Value;

                                if (keyboardState.IsKeyDown(Keys.Alt))
                                {
                                    _translatePoint = intersectPoint;
                                }
                                else
                                {
                                    translationVector.X = (int)translationVector.X;
                                    translationVector.Y = (int)translationVector.Y;
                                    translationVector.Z = (int)translationVector.Z;

                                    if (Math.Abs(translationVector.X) >= 1 || Math.Abs(translationVector.Y) >= 1 ||
                                        Math.Abs(translationVector.Z) >= 1)
                                    {

                                        _translatePoint = intersectPoint;
                                    }
                                }

                                // send translation to current state
                                //var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex];
                                var translationMatrix = Matrix.Translation(translationVector);
                                partState.Transform.TranslationVector += translationVector;
                                partState.BoundingBox = new BoundingBox(Vector3.TransformCoordinate(partState.BoundingBox.Minimum, translationMatrix), Vector3.TransformCoordinate(partState.BoundingBox.Maximum, translationMatrix));


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

                        GetSelectedCube(out _pickedCube, out _newCube);

                        if (mouseState.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed)
                        {
                            if (_pickedCube.HasValue)
                            {
                                switch (_selectedToolIndex)
                                {
                                    case 0:
                                        frame.BlockData.SetBlock(_pickedCube.Value, 0);
                                        break;  
                                    case 1:
                                        frame.BlockData.SetBlock(_pickedCube.Value, (byte)(_selectedColorIndex + 1));
                                        break;
                                    case 2:
                                        // color fill 
                                        var fillIndex = frame.BlockData.GetBlock(_pickedCube.Value);
                                        var fillWith = (byte)(_selectedColorIndex + 1);
                                        // recursive change all adjacent cubes
                                        if (fillIndex != fillWith)
                                            ColorFill(frame, _pickedCube.Value, fillIndex, fillWith);
                                        break;
                                }

                                
                                RebuildFrameVertices();
                            }
                        }
                        else if (mouseState.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed)
                        {
                            if (_newCube.HasValue)
                            {
                                switch (_selectedToolIndex)
                                {
                                    case 0:
                                        frame.BlockData.SetBlock(_newCube.Value, (byte)(_selectedColorIndex + 1));
                                        break;
                                    case 1:
                                        break;
                                    case 2:
                                        break;
                                }
                                
                                RebuildFrameVertices();
                            }
                        }
                        
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _prevState = mouseState;

            base.Update(ref timeSpent);
        }

        private void ColorFill(VoxelFrame frame, Vector3I vector3I, byte fillIndex, byte newIndex)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        var checkVector = vector3I + new Vector3I(x, y, z);
                        if (InChunk(frame.BlockData.ChunkSize, checkVector) && frame.BlockData.GetBlock(checkVector) == fillIndex)
                        {
                            frame.BlockData.SetBlock(checkVector, newIndex);
                            ColorFill(frame, checkVector, fillIndex, newIndex);
                        }
                    }
                }
            }
        }
        
        private void GetSelectedCube(out Vector3I? cubePosition, out Vector3I? newCubePosition)
        {
            if (_selectedPartIndex == -1 || _selectedFrameIndex == -1)
            {
                cubePosition = null;
                newCubePosition = null;
                return;
            }

            Vector3D mPosition, mLookAt;

            var worldViewProjection = _transform * _viewProjection;

            InputsManager.UnprojectMouseCursor(_d3DEngine, ref worldViewProjection, out mPosition, out mLookAt);

            if (double.IsNaN(mPosition.X) || double.IsNaN(mLookAt.X))
            {
                cubePosition = null;
                newCubePosition = null;
                return;
            }

            var blocks = _visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames[_selectedFrameIndex].BlockData;

            var size = blocks.ChunkSize;

            for (float i = 0; i < 100; i += 0.1f)
            {
                var point = (mPosition + (mLookAt * i));
                var targetPoint = (Vector3I)point;

                if (point.X < 0 || point.Y < 0 || point.Z < 0 || point.X >= size.X || point.Y >= size.Y || point.Z >= size.Z)
                    continue;

                if (blocks.GetBlock(targetPoint) != 0)
                {
                    cubePosition = targetPoint;
                    newCubePosition = null;

                    // roll back to find a new CubePlace
                    for (; i > 0; i -= 0.1f)
                    {
                        point = (mPosition + (mLookAt * i));
                        targetPoint = (Vector3I)point;

                        if (point.X < 0 || point.Y < 0 || point.Z < 0 || point.X >= size.X || point.Y >= size.Y || point.Z >= size.Z)
                            return;

                        if (blocks.GetBlock(targetPoint) == 0)
                        {
                            newCubePosition = targetPoint;
                            return;
                        }
                    }


                    return;
                }
            }

            newCubePosition = null;

            if (_gridPlanes != null)
            {

                // try to find a plain cross
                var r = new Ray(mPosition.AsVector3(), mLookAt.AsVector3());
                float distance = 0;
                for (int i = 0; i < _gridPlanes.Length; i++)
                {
                    float d;
                    if (r.Intersects(ref _gridPlanes[i], out d) && d > distance)
                    {
                        Vector3 point;
                        r.Intersects(ref _gridPlanes[i], out point);

                        //var pos = new Vector3I(Math.Floor(point.X), Math.Floor(point.Y), Math.Floor(point.Z));
                        var pos = (Vector3I)point;
                        if (_gridPlanes[i] == _gridBottomPlane)
                        {
                            pos.Y = 0;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                        if (_gridPlanes[i] == _gridTopPlane)
                        {
                            pos.Y = size.Y - 1;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                        if (_gridPlanes[i] == _gridLeftPlane)
                        {
                            pos.X = 0;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                        if (_gridPlanes[i] == _gridRightPlane)
                        {
                            pos.X = size.X - 1;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                        if (_gridPlanes[i] == _gridFrontPlane)
                        {
                            pos.Z = size.Z - 1;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                        if (_gridPlanes[i] == _gridBackPlane)
                        {
                            pos.Z = 0;
                            if (InChunk(size, pos))
                            {
                                newCubePosition = pos;
                                distance = d;
                            }
                        }
                    }
                }
            }
            
            cubePosition = null;
            
        }

        private bool InChunk(Vector3I size, Vector3I pos)
        {
            if (pos.X >= 0 && pos.Y >= 0 && pos.Z >= 0 && pos.X < size.X && pos.Y < size.Y && pos.Z < size.Z)
                return true;
            return false;
        }

        private void RebuildFrameVertices()
        {
            if (_visualVoxelModel != null && _selectedPartIndex != -1 && _selectedFrameIndex != -1)
            {
                _visualVoxelModel.RebuildFrame(_selectedPartIndex, _selectedFrameIndex);
            }
        }

        private void UpdateTransformMatrix(ViewParameters parameters, BoundingBox modelBoundingBox)
        {
            var translateVector = Vector3.Negate(Vector3.Add(modelBoundingBox.Maximum, modelBoundingBox.Minimum) / 2);

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

        #region Draw
        public override void Draw(int index)
        {
            if (_visualVoxelModel == null)
            {
                _infoLabel.Text = "Choose a model to open or create new";
                return;
            }

            if (_visualVoxelModel.VoxelModel.Parts.Count == 0)
            {
                _infoLabel.Text = "Please go to Frames mode and add at least one part to the model";
            }

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

                _visualVoxelModel.Draw(_voxelEffect, _instance.State);
            }
        }

        private void DrawModelLayout()
        {
            if (_visualVoxelModel != null)
            {
                // draw each part with bounding box

                var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex];

                //DrawBox(state.BoundingBox);

                StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
                

                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelParts;

                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerFrame.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerFrame.IsDirty = true;
                }

                // draw each part of the model
                for (int i = 0; i < state.PartsStates.Count; i++)
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
                for (int i = 0; i < state.PartsStates.Count; i++)
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

                DrawBox(state.BoundingBox, new Color4(1, 1, 1, 0.1f));
            }
        }

        private void DrawGrid()
        {
            _lines3DEffect.Begin();
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 1, 1, 0.1f);

            // x-z

            var eye = new Vector3(0,0,5);
            var point = new Vector3(1, 0, 1);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationX((float)Math.PI/2) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            var bottomNormal = Vector3.TransformNormal(_gridBottomNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            var viewVector = eye - point;
            var dot = Vector3.Dot(viewVector, bottomNormal);
            if (dot > 0)
            {
                _xGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_xGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(0, _gridSize.Y, 0);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation(0,_gridSize.Y,0) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            var topNormal = Vector3.TransformNormal(_gridTopNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, topNormal);
            if (dot > 0)
            {
                _xGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_xGridVertextBuffer.VertexCount, 0);
            }
            // x-y

            point = new Vector3();

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(_transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();
            var backNormal = Vector3.TransformNormal(_gridBackNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, backNormal);
            if (dot > 0)
            {
                _yGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_yGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(0, 0, _gridSize.Z);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Translation(0, 0, _gridSize.Z) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();
            var frontNormal = Vector3.TransformNormal(_gridFrontNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, frontNormal);
            if (dot > 0)
            {
                _yGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_yGridVertextBuffer.VertexCount, 0);
            }
            // z-y

            point = new Vector3();

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationY(-(float)Math.PI / 2) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            var leftNormal = Vector3.TransformNormal(_gridLeftNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, leftNormal);
            if (dot > 0)
            {
                _zGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_zGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(_gridSize.X,0,0);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(_gridSize.X, 0 , 0) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply();

            var rightNormal = Vector3.TransformNormal(_gridRightNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, rightNormal);
            if (dot > 0)
            {
                _zGridVertextBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(_zGridVertextBuffer.VertexCount, 0);
            }
        }

        private void DrawFrameEdit()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

                #region Grid
                var frame = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex].Frames[SelectedFrameIndex];
                if (_gridSize != frame.BlockData.ChunkSize)
                {
                    InitPlanes(frame.BlockData.ChunkSize);
                    BuildGrid(frame.BlockData.ChunkSize);
                }

                DrawGrid();

                #endregion

                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelParts;

                StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);
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
                _voxelEffect.CBPerFrame.Values.LightDirection = new Vector3(1, 1, 1);
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

                
                if (_pickedCube != null)
                {
                    // draw selected cube
                    DrawBox(new BoundingBox(_pickedCube.Value, _pickedCube.Value + Vector3I.One), new Color4(1, 0, 0, 1));
                }
                else if (_newCube != null)
                {
                    // draw selected cube
                    DrawBox(new BoundingBox(_newCube.Value, _newCube.Value + Vector3I.One), new Color4(1, 0, 0, 0.5f));
                }
            }
        }
        #endregion

        private void BuildGrid(Vector3I size)
        {
            if (_gridSize.X != size.X || _gridSize.Z != size.Z)
            {
                if(_xGridVertextBuffer != null)
                    _xGridVertextBuffer.Dispose();
                var list = new List<VertexPosition>();
                FillGrid(list, size.X, size.Z);
                _xGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, list.Count, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorXGrid");
                _xGridVertextBuffer.SetData(list.ToArray());
            }

            if (_gridSize.X != size.X || _gridSize.Y != size.Y)
            {
                if (_yGridVertextBuffer != null)
                    _yGridVertextBuffer.Dispose();
                var list = new List<VertexPosition>();
                FillGrid(list, size.X, size.Y);
                _yGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, list.Count, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorYGrid");
                _yGridVertextBuffer.SetData(list.ToArray());
            }

            if (_gridSize.Z != size.Z || _gridSize.Y != size.Y)
            {
                if (_zGridVertextBuffer != null)
                    _zGridVertextBuffer.Dispose();
                var list = new List<VertexPosition>();
                FillGrid(list, size.Z, size.Y);
                _zGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine, list.Count, VertexPosition.VertexDeclaration, PrimitiveTopology.LineList, "EditorYGrid");
                _zGridVertextBuffer.SetData(list.ToArray());
            }

            _gridSize = size;

        }

        private void FillGrid(List<VertexPosition> positions, int x, int y)
        {
            for (int i = 0; i <= x; i++)
            {
                positions.Add(new VertexPosition(new Vector3I(i, 0, 0)));
                positions.Add(new VertexPosition(new Vector3I(i, y, 0)));
            }
            for (int i = 0; i <= y; i++)
            {
                positions.Add(new VertexPosition(new Vector3I(0, i, 0)));
                positions.Add(new VertexPosition(new Vector3I(x, i, 0)));
            }
        }

        public override void UnloadContent()
        {
            if (_boxVertexBuffer != null) _boxVertexBuffer.Dispose();
            if (_boxIndexBuffer != null) _boxIndexBuffer.Dispose();
            if (_crosshairVertexBuffer != null) _crosshairVertexBuffer.Dispose();
            _lines3DEffect.Dispose();
            _voxelEffect.Dispose();
            _font.Dispose();

            if (_xGridVertextBuffer != null)
            {
                _xGridVertextBuffer.Dispose();
                _yGridVertextBuffer.Dispose();
                _zGridVertextBuffer.Dispose();

                _xGridVertextBuffer = null;
                _yGridVertextBuffer = null;
                _zGridVertextBuffer = null;
            }

            base.UnloadContent();
        }


        
    }
}
