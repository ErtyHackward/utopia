using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ninject;
using S33M3Resources.Structs.Helpers;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using System.Windows.Forms;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Resources.Effects.Entities;
using Matrix = SharpDX.Matrix;
using System.Globalization;

namespace Utopia.Components
{
    /// <summary>
    /// Allows user to edit a voxel model in a visual way
    /// </summary>
    public partial class ModelEditorComponent : DrawableGameComponent
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly InputsManager _inputManager;
        private readonly IconFactory _iconFactory;
        private readonly D3DEngine _d3DEngine;
        private HLSLColorLine _lines3DEffect;
        private SpriteFont _font;

        private VertexBuffer<VertexPosition> _boxVertexBuffer;
        private IndexBuffer<ushort> _boxIndexBuffer;
        
        private Vector3I _gridSize;
        private VertexBuffer<VertexPosition> _xGridVertextBuffer;
        private VertexBuffer<VertexPosition> _yGridVertextBuffer;
        private VertexBuffer<VertexPosition> _zGridVertextBuffer;

        private readonly Vector3 _gridBackNormal = new Vector3(0, 0, 1);
        private readonly Vector3 _gridFrontNormal = new Vector3(0, 0, -1);
        private readonly Vector3 _gridTopNormal = new Vector3(0, -1, 0);
        private readonly Vector3 _gridBottomNormal = new Vector3(0, 1, 0);
        private readonly Vector3 _gridLeftNormal = new Vector3(1, 0, 0);
        private readonly Vector3 _gridRightNormal = new Vector3(-1, 0, 0);

        private Plane _gridBackPlane;
        private Plane _gridFrontPlane;
        private Plane _gridTopPlane;
        private Plane _gridBottomPlane;
        private Plane _gridLeftPlane;
        private Plane _gridRightPlane;

        private Plane[] _gridPlanes;


        private VertexBuffer<VertexPosition> _crosshairVertexBuffer;
        private VertexBuffer<VertexPosition> _directionVertexBuffer;
        private VertexBuffer<VertexPosition> _rotationVertexBuffer;
        private VertexBuffer<VertexPosition> _toolVertexBuffer;

        private HLSLVoxelModelInstanced _voxelInstancedEffect;
        private HLSLVoxelModel _voxelEffect;

        // view parameters
        private ViewParameters _mainViewData;
        private ViewParameters _frameViewData;

        private ViewParameters _currentViewData;
        //private Vector2 _accumulatedPosition;

        private Matrix _transform;
        private VisualVoxelModel _visualVoxelModel;

        private Matrix _viewProjection;
        private Matrix _view;

        private EditorMode _mode;
        private VoxelFrame _voxelFrame = null;

        private readonly MainScreen _screen;
        private readonly VoxelModelManager _manager;
        private readonly VoxelMeshFactory _meshFactory;
        private readonly GuiManager _gui;
        private readonly List<S33M3CoreComponents.GUI.Nuclex.Controls.Control> _controls = new List<S33M3CoreComponents.GUI.Nuclex.Controls.Control>();

        private int _selectedFrameIndex = -1;
        private int _selectedPartIndex = -1;
        private int _selectedAnimationIndex = -1;

        private bool _flipAxis;

        private Vector3I? _pickedCube;
        private Vector3I? _newCube;

        private int _selectedColorIndex;
        private FrameEditorTools _frameEditorTool;
        private EditorAxis _sliceAxis = EditorAxis.Y;
        private EditorAxis _mirror = EditorAxis.None;

        private bool _displayLayoutRotationPosition = true;

        // common selection point, used as a center for rotation and tool mount point
        private Vector3 _cursorPoint;

        // indicates if user moves cursor (or model part itself)
        private bool _cursorMode;

        /// <summary>
        /// Provides a plane for a part translating
        /// </summary>
        private Plane _translatePlane;
        private Vector3? _translatePoint;

        private bool _needSave;
        private VoxelModelInstance _instance;
        private VoxelModelState _clipboardState;
        private InsideDataProvider _clipboardBlock;
        private InsideDataProvider _backupBlock;

        private Vector3I? _selectionStart;
        private Vector3I? _selectionEnd;
        private bool _toolMountPointSelection;

        private List<BoundingBox> _frameShapeBBoxes = null;
        private int _selectedStateIndex;

        #endregion

        #region Properties

        public string Author { get; set; }

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
                    _selectedPartIndex = 0;
                    _selectedFrameIndex = 0;
                    _selectedStateIndex = 0;

                    // fill the lists
                    _statesList.Items.Clear();
                    _statesList.SelectedItems.Clear();

                    foreach (var state in _visualVoxelModel.VoxelModel.States)
                    {
                        _statesList.Items.Add(state);
                    }
                    SelectedStateIndex = 0;
                    _statesList.SelectedItems.Add(SelectedStateIndex);

                    // ----------------------------------------------------------------

                    _partsList.Items.Clear();
                    _partsList.SelectedItems.Clear();

                    foreach (var voxelModelPart in _visualVoxelModel.VoxelModel.Parts)
                    {
                        _partsList.Items.Add(voxelModelPart);
                    }
                    if (SelectedPartIndex == -1)
                        SelectedPartIndex = 0;

                    _partsList.SelectedItems.Add(_selectedPartIndex);

                    // ----------------------------------------------------------------

                    _animationsList.Items.Clear();
                    _animationsList.SelectedItems.Clear();

                    foreach (var animation in _visualVoxelModel.VoxelModel.Animations)
                    {
                        _animationsList.Items.Add(animation);
                    }

                    // ----------------------------------------------------------------

                    _framesList.Items.Clear();
                    foreach (var frame in _visualVoxelModel.VoxelModel.Frames)
                    {
                        _framesList.Items.Add(frame);
                    }
                    _framesList.SelectedItems.Clear();

                    // ----------------------------------------------------------------

                    UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, 0);

                    _instance = _visualVoxelModel.VoxelModel.CreateInstance();
                }

                if (_visualVoxelModel == null)
                {
                    _selectedPartIndex = -1;
                    _selectedFrameIndex = -1;
                    _selectedAnimationIndex = -1;

                    _partsList.Items.Clear();
                    _framesList.Items.Clear();
                    _animationsList.Items.Clear();

                    if (_statesList != null)
                        _statesList.Items.Clear();
                }
            }
        }

        public int SelectedStateIndex
        {
            get { return _selectedStateIndex; }
            private set { 
                if (_selectedStateIndex == value)
                    return;

                _selectedStateIndex = value;

                if (SelectedStateIndex != -1 && _selectedPartIndex != -1)
                {
                    SelectedFrameIndex =
                        _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex]
                            .ActiveFrame;
                }
            }
        }

        public int SelectedPartIndex
        {
            get { return _selectedPartIndex; }
            private set
            {
                if (_selectedPartIndex != value)
                {
                    _selectedPartIndex = value;

                    if (SelectedStateIndex != -1 && _selectedPartIndex != -1)
                    {
                        SelectedFrameIndex =
                            _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex]
                                .ActiveFrame;
                    }

                }
            }
        }

        public int SelectedFrameIndex
        {
            get { return _selectedFrameIndex; }
            private set
            {
                if (_selectedFrameIndex == value)
                    return;
                
                _selectedFrameIndex = value;

                ClearSelection();

                if (Mode == EditorMode.ModelLayout)
                {
                    var ps = _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex];

                    if (ps.ActiveFrame != _selectedFrameIndex)
                    {
                        ps.ActiveFrame = (byte)_selectedFrameIndex;
                        NeedSave();
                        _instance.UpdateStates();
                    }

                    if (_framesList.SelectedItems.Count == 1)
                        _framesList.SelectedItems[0] = _selectedFrameIndex;
                    else 
                        _framesList.SelectedItems.Add(_selectedFrameIndex);
                }
                if (Mode == EditorMode.FrameEdit)
                    UpdateCamera();
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

                    if (_mode == EditorMode.MainView)
                        SelectedStateIndex = 0;

                    UpdateCamera();
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

        #region DI

        [Inject]
        public ClientWebApi WebApi { get; set; }

        #endregion

        /// <summary>
        /// Creates new editor component
        /// </summary>
        public ModelEditorComponent(
            D3DEngine d3DEngine, 
            MainScreen screen, 
            VoxelModelManager manager, 
            VoxelMeshFactory meshFactory, 
            GuiManager gui, 
            InputsManager inputManager, 
            IconFactory iconFactory)
        {
            _inputManager = inputManager;
            _iconFactory = iconFactory;
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
            _d3DEngine.ScreenSize_Updated += ViewportUpdated;

            _manager.VoxelModelAvailable += ManagerVoxelModelAvailable;

            DrawOrders.UpdateIndex(0, 15);
        }

        void ManagerVoxelModelAvailable(object sender, VoxelModelReceivedEventArgs e)
        {
            UpdateModelsList(_visualVoxelModel);
        }

        private void InitPlanes(Vector3I chunkSize)
        {
            _gridBackPlane = new Plane(new Vector3(), _gridBackNormal);
            _gridFrontPlane = new Plane(chunkSize, _gridFrontNormal);
            _gridTopPlane = new Plane(chunkSize, _gridTopNormal);
            _gridBottomPlane = new Plane(new Vector3(), _gridBottomNormal);
            _gridLeftPlane = new Plane(new Vector3(), _gridLeftNormal);
            _gridRightPlane = new Plane(chunkSize, _gridRightNormal);

            _gridPlanes = new[] { _gridBackPlane, _gridFrontPlane, _gridTopPlane, _gridBottomPlane, _gridLeftPlane, _gridRightPlane };
        }

        private void ViewportUpdated(ViewportF port, Texture2DDescription newBackBufferDescr)
        {
            //Update The projection matrix
            var aspect = port.Width / port.Height;
            var projection = Matrix.PerspectiveFovLH((float)Math.PI / 3, aspect, 1f, 100);
            _viewProjection = _view * projection;

            UpdateLayout();
        }

        public override void Initialize()
        {
            InitializeGui();

            foreach (var model in _manager.Enumerate().OrderBy(x => x.VoxelModel.Name))
            {
                _modelsList.Items.Add(model);
            }
            

            base.Initialize();
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            if (Updatable)
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Add(control);
                }

                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout();

                // call property setter to fill the lists
                if (_visualVoxelModel != null)
                    VisualVoxelModel = _visualVoxelModel;

                OnMainViewMode();
            }
            else
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Remove(control);
                }

                _screen.Desktop.Children.Remove(_backButton);
            }

            base.OnUpdatableChanged(sender, args);
        }
        
        public override void LoadContent(DeviceContext context)
        {
            _font = new SpriteFont();
            _font.Initialize("Tahoma", 13f, System.Drawing.FontStyle.Regular, true, _d3DEngine.Device);

            _lines3DEffect = new HLSLColorLine(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\ColorLine.hlsl", VertexPosition.VertexDeclaration);
            
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

            _boxVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, 8, PrimitiveTopology.LineList, "EditorBox_vertexBuffer");
            _boxVertexBuffer.SetData(_d3DEngine.ImmediateContext, ptList.ToArray());

            _boxIndexBuffer = new IndexBuffer<ushort>(_d3DEngine.Device, indices.Length, "EditorBox_indexBuffer");
            _boxIndexBuffer.SetData(_d3DEngine.ImmediateContext, indices);
            
            ptList.Clear();

            ptList.Add(new VertexPosition(new Vector3(0,  0, -1)));
            ptList.Add(new VertexPosition(new Vector3(0,  0,  1)));
            ptList.Add(new VertexPosition(new Vector3(0, -1,  0)));
            ptList.Add(new VertexPosition(new Vector3(0,  1,  0)));

            _crosshairVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, ptList.Count, PrimitiveTopology.LineList, "EditorCrosshair_vertexBuffer");
            _crosshairVertexBuffer.SetData(_d3DEngine.ImmediateContext, ptList.ToArray());

            ptList.Clear();
            ptList.Add(new VertexPosition(new Vector3(-1, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3( 1, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3( 0, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3( 0, 1, 0)));
            ptList.Add(new VertexPosition(new Vector3( 0, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3( 0, 0, 1)));

            _directionVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, ptList.Count, PrimitiveTopology.LineList, "EditorDirection_vertexBuffer");
            _directionVertexBuffer.SetData(_d3DEngine.ImmediateContext, ptList.ToArray());

            ptList.Clear();
            ptList.Add(new VertexPosition(new Vector3(0, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3(1, 0, 0)));
            
            _rotationVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, ptList.Count, PrimitiveTopology.LineList, "EditorRotation_vertexBuffer");
            _rotationVertexBuffer.SetData(_d3DEngine.ImmediateContext, ptList.ToArray());

            ptList.Clear();

            ptList.Add(new VertexPosition(new Vector3(0, 0, 0)));
            ptList.Add(new VertexPosition(new Vector3(0, 1, 0)));
            ptList.Add(new VertexPosition(new Vector3(-0.5f, 0.8f, 0)));
            ptList.Add(new VertexPosition(new Vector3(0.5f, 0.8f, 0)));

            _toolVertexBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, ptList.Count, PrimitiveTopology.LineList, "EditorTool_vertexBuffer");
            _toolVertexBuffer.SetData(_d3DEngine.ImmediateContext, ptList.ToArray());

            _voxelEffect = new HLSLVoxelModel(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration);
            _voxelInstancedEffect = new HLSLVoxelModelInstanced(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VoxelInstanceData.VertexDeclaration);


            base.LoadContent(context);
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
            voxelModelState.PartsStates.Add(new VoxelModelPartState());
            model.VoxelModel.Parts.Add(part);

            // add default frame
            model.VoxelModel.Frames.Add(new VoxelFrame(new Vector3I(16, 16, 16)));

            model.BuildMesh();
            _partsList.Items.Add(part);


            _modelsList.Items.Add(model);
            _modelsList.SelectedItems.Clear();
            _modelsList.SelectedItems.Add(_modelsList.Items.Count - 1);

            VisualVoxelModel = model;
            NeedSave();
        }

        private void OnModelsEditPressed()
        {
            if (VisualVoxelModel == null)
            {
                _gui.MessageBox("Please select or create a model to edit");
                return;
            }

            _modelEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogModelEditStruct { Name = VisualVoxelModel.VoxelModel.Name }, "Model edit", OnModelEdited);
        }

        private void OnModelEdited(DialogModelEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "rename_me";

            if (_manager.Contains(VisualVoxelModel.VoxelModel.Name))
            {
                _manager.Rename(VisualVoxelModel.VoxelModel.Name, e.Name);
            }

            VisualVoxelModel.VoxelModel.Name = e.Name;
            
        }

        private void OnModelsDeletePressed()
        {
            _gui.MessageBox(string.Format("Are you sure want to delete '{0}'?", VisualVoxelModel.VoxelModel.Name), "Confirmation", new[] { "Yes", "No" }, OnModelDeleted);
        }

        private void OnModelDeleted(string btn)
        {
            if (btn == "Yes")
            {
                _manager.DeleteModel(VisualVoxelModel.VoxelModel.Name);
                VisualVoxelModel = null;
                _modelsList.Items.RemoveAt(_modelsList.SelectedItems[0]);
            }
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
            
            _visualVoxelModel.VoxelModel.AddPart(part);
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

            _partEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, s, "Edit the part", OnPartEdited);

        }
        private void OnPartEdited(DialogPartsEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "noname part";

            var part = _visualVoxelModel.VoxelModel.Parts[SelectedPartIndex];

            part.Name = e.Name;
            part.IsHead = e.IsHead;
            part.IsArm = e.IsArm;

            NeedSave();
        }

        private void OnPartsDeletePressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part to delete");
                return;
            }

            if (_visualVoxelModel.VoxelModel.Parts.Count == 1)
            {
                _gui.MessageBox("Model must have at least one part");
                return;
            }
            
            _visualVoxelModel.VoxelModel.RemovePartAt(SelectedPartIndex);
            _partsList.Items.RemoveAt(SelectedPartIndex);

            if (_partsList.Items.Count > 0 && SelectedPartIndex == 0)
            {
                _selectedPartIndex = -1;
                SelectedPartIndex = 0;
            }
            else
                SelectedPartIndex = SelectedPartIndex - 1;

            _partsList.SelectItem(SelectedPartIndex);
            _visualVoxelModel.BuildMesh();
            _instance.UpdateStates();
        }

        private void OnFrameAddPressed()
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a part to add a frame to");
                return;
            }

            var ds = new DialogFrameEditStruct { 
                SizeX = 16, 
                SizeY = 16, 
                SizeZ = 16, 
                Name = "noname" 
            };
            
            _frameEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, ds, "Add a new frame", OnFrameAdded);
        }
        private void OnFrameAdded(DialogFrameEditStruct e)
        {
            if (e.SizeX < 1 || e.SizeY < 1 || e.SizeZ < 1 || e.SizeX > 128 || e.SizeY > 128 || e.SizeZ > 128)
            {
                _gui.MessageBox("Each part of frame size should be in range [1;128]");
                return;
            }
            var frame = new VoxelFrame(new Vector3I(e.SizeX, e.SizeY, e.SizeZ)) { Name = e.Name };
            _visualVoxelModel.VoxelModel.Frames.Add(frame);
            _visualVoxelModel.BuildMesh();
            _framesList.Items.Add(frame);
        }
        
        private void OnFrameEditPressed()
        {
            if (SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Please select a frame to edit");
                return;
            }

            var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex];
            var size = frame.BlockData.ChunkSize;
            var args = new DialogFrameEditStruct {
                SizeX = size.X, 
                SizeY = size.Y, 
                SizeZ = size.Z,
                Name = frame.Name
            };

            args.MirrorBack   = (frame.FrameMirror & FrameMirror.MirrorBack) == FrameMirror.MirrorBack;
            args.MirrorFront  = (frame.FrameMirror & FrameMirror.MirrorFront) == FrameMirror.MirrorFront;
            args.MirrorTop    = (frame.FrameMirror & FrameMirror.MirrorTop) == FrameMirror.MirrorTop;
            args.MirrorBottom = (frame.FrameMirror & FrameMirror.MirrorBottom) == FrameMirror.MirrorBottom;
            args.MirrorLeft   = (frame.FrameMirror & FrameMirror.MirrorLeft) == FrameMirror.MirrorLeft;
            args.MirrorRight  = (frame.FrameMirror & FrameMirror.MirrorRight) == FrameMirror.MirrorRight;
            args.TileBack     = (frame.FrameMirror & FrameMirror.TileBack) == FrameMirror.TileBack;
            args.TileFront    = (frame.FrameMirror & FrameMirror.TileFront) == FrameMirror.TileFront;
            args.TileTop      = (frame.FrameMirror & FrameMirror.TileTop) == FrameMirror.TileTop;
            args.TileBottom   = (frame.FrameMirror & FrameMirror.TileBottom) == FrameMirror.TileBottom;
            args.TileLeft     = (frame.FrameMirror & FrameMirror.TileLeft) == FrameMirror.TileLeft;
            args.TileRight    = (frame.FrameMirror & FrameMirror.TileRight) == FrameMirror.TileRight;

            
            _frameEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, args, "Edit frame size", OnFrameEdited);
        }
        private void OnFrameEdited(DialogFrameEditStruct e)
        {
            var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex];
            frame.Name = e.Name;
            frame.BlockData.UpdateChunkSize(new Vector3I(e.SizeX, e.SizeY, e.SizeZ), true);

            frame.FrameMirror = 0;
            if (e.MirrorBack) frame.FrameMirror |= FrameMirror.MirrorBack;
            if (e.MirrorFront) frame.FrameMirror |= FrameMirror.MirrorFront;
            if (e.MirrorTop) frame.FrameMirror |= FrameMirror.MirrorTop;
            if (e.MirrorBottom) frame.FrameMirror |= FrameMirror.MirrorBottom;
            if (e.MirrorLeft) frame.FrameMirror |= FrameMirror.MirrorLeft;
            if (e.MirrorRight) frame.FrameMirror |= FrameMirror.MirrorRight;

            if (e.TileBack) frame.FrameMirror |= FrameMirror.TileBack;
            if (e.TileFront) frame.FrameMirror |= FrameMirror.TileFront;
            if (e.TileTop) frame.FrameMirror |= FrameMirror.TileTop;
            if (e.TileBottom) frame.FrameMirror |= FrameMirror.TileBottom;
            if (e.TileLeft) frame.FrameMirror |= FrameMirror.TileLeft;
            if (e.TileRight) frame.FrameMirror |= FrameMirror.TileRight;

            RebuildFrameVertices();
            ClearSelection();
            NeedSave();
        }

        private void OnFrameDeletePressed()
        {
            if (SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Please select a frame to delete");
                return;
            }
            if (_visualVoxelModel.VoxelModel.Frames.Count == 1)
            {
                _gui.MessageBox("Model must have at least one frame");
                return;
            }

            _visualVoxelModel.RemoveFrameAt(SelectedFrameIndex);
            _visualVoxelModel.BuildMesh();
            _framesList.Items.RemoveAt(SelectedFrameIndex);

            _framesList.SelectItem(0);
            _selectedFrameIndex = 0;
        }

        private void OnFrameHidePressed()
        {
            if (SelectedStateIndex == -1 || SelectedPartIndex == -1)
            {
                _gui.MessageBox("Please select a state and part to hide");
                return;
            }

            _visualVoxelModel.VoxelModel.States[SelectedStateIndex].PartsStates[_selectedPartIndex].ActiveFrame = byte.MaxValue;
            NeedSave();
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
                    _visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_colorPalette.Count] = ColorHelper.ToColor4(colorDialog.Color);
                    UpdateColorPalette(_visualVoxelModel.VoxelModel.ColorMapping, _selectedColorIndex);
                }
            }
            GuiManager.DialogClosed = true;

        }

        private void OnColorChangePressed()
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = ColorHelper.ToSystemColor(_visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_selectedColorIndex]);
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    _visualVoxelModel.VoxelModel.ColorMapping.BlockColors[_selectedColorIndex] = ColorHelper.ToColor4(colorDialog.Color);
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
            foreach (var voxelFrame in _visualVoxelModel.VoxelModel.Frames)
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
            UpdateCamera();
        }

        private bool _askedSave;

        private void AskModelSave(Action<string> callback)
        {
            if (_askedSave)
                return;
            _gui.MessageBox("Current model was modified. Would you like to save the changes?", "Confirm", new[] { "Save", "Drop changes" },
                s =>
                {
                    _askedSave = false;
                    callback(s);
                });
            _askedSave = true;
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
            _stateEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogStateEditStruct(), "Add a new state", OnStateAdded);
            
        }
        private void OnStateAdded(DialogStateEditStruct e)
        {
            var vms = new VoxelModelState(VisualVoxelModel.VoxelModel.States[VisualVoxelModel.VoxelModel.States.Count - 1]);
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "unnamed";
            vms.Name = e.Name;
            VisualVoxelModel.VoxelModel.States.Add(vms);
            _statesList.Items.Add(vms);
        }
        
        private void OnStateEditButtonPressed()
        {
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
            if (VisualVoxelModel.VoxelModel.States.Count == 1)
            {
                _gui.MessageBox("Model should have at least one state");
                return;
            }

            _statesList.Items.RemoveAt(SelectedStateIndex);
            VisualVoxelModel.VoxelModel.RemoveStateAt(SelectedStateIndex);

            _statesList.SelectItem(0);
            SelectedStateIndex = 0;
            UpdateCamera();
        }

        private void OnAnimationsAddButtonPressed()
        {
            _animationsEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogAnimationEditStruct(), "Add a new animation", OnAnimationAdded);
        }

        private void OnAnimationAdded(DialogAnimationEditStruct e)
        {
            if (string.IsNullOrEmpty(e.Name))
                e.Name = "unnamed";
            
            var animation = new VoxelModelAnimation();
            
            animation.Name = e.Name;
            animation.StartFrame = e.StartFrame;

            VisualVoxelModel.VoxelModel.Animations.Add(animation);
            _animationsList.Items.Add(animation);
        }
        
        private void OnAnimationsEditButtonPressed()
        {
            if (SelectedAnimationIndex == -1)
            {
                _gui.MessageBox("Select an animation to edit");
                return;
            }

            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];

            _animationsEditDialog.ShowDialog(_screen, _d3DEngine.ViewPort, new DialogAnimationEditStruct { Name = animation.Name, StartFrame = animation.StartFrame }, "Animation edit", OnAnimationEdited);
        }
        private void OnAnimationEdited(DialogAnimationEditStruct e)
        {
            var animation = VisualVoxelModel.VoxelModel.Animations[SelectedAnimationIndex];
            animation.Name = e.Name;
            animation.StartFrame = e.StartFrame;
        }

        private void OnAnimationsDeleteButtonPressed()
        {
            VisualVoxelModel.VoxelModel.Animations.RemoveAt(SelectedAnimationIndex);
            _animationsList.Items.RemoveAt(SelectedAnimationIndex);

        }

        private void OnAnimationStepAddButtonPressed()
        {
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
            _instance.Play(SelectedAnimationIndex, true);
        }

        private void OnAnimationStopButtonPressed()
        {
            _instance.Stop();
        }

        #region Presets

        private void OnFillPresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                if (frame.BlockBytes == null)
                    frame.SetBlock(new Vector3I(), 0);

                if (_selectionStart.HasValue && _selectionEnd.HasValue)
                {
                    var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                    var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                    var selectionSize = max - min + Vector3I.One;
                    var range = new Range3I(min, selectionSize);

                    foreach (var position in range)
                    {
                        frame.SetBlock(position, (byte)(_selectedColorIndex + 1));
                    }
                }
                else
                {
                    for (int i = 0; i < frame.BlockBytes.Length; i++)
                    {
                        frame.BlockBytes[i] = (byte) (_selectedColorIndex + 1);
                    }
                }

                RebuildFrameVertices();
            }
        }

        private void OnCylinderPresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                if (frame.BlockBytes == null)
                    frame.SetBlock(new Vector3I(), 0);

                Vector3 center;

                Range3I range;
                if (_selectionStart.HasValue && _selectionEnd.HasValue)
                {
                    var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                    var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                    var selectionSize = max - min + Vector3I.One;
                    range = new Range3I(min, selectionSize);
                    center = (Vector3)selectionSize / 2;
                }
                else
                {
                    range = new Range3I { Size = frame.ChunkSize };
                    center = (Vector3)frame.ChunkSize / 2;
                }

                var radius = Math.Min(center.X, center.Z);

                foreach (var position in range)
                {
                    var point = position + new Vector3(0.5f, 0.5f, 0.5f);
                    var rangeOffset = center + range.Position;
                    //center.Y = point.Y;
                    rangeOffset.Y = point.Y;
                    if (Vector3.Distance(point, rangeOffset) <= radius)
                        frame.SetBlock((Vector3I)point, (byte)(_selectedColorIndex + 1));
                }

                RebuildFrameVertices();
            }
        }

        private void OnEllipsoidPresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                if (frame.BlockBytes == null)
                    frame.SetBlock(new Vector3I(), 0);

                Vector3 center;

                Range3I range;
                if (_selectionStart.HasValue && _selectionEnd.HasValue)
                {
                    var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                    var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                    var selectionSize = max - min + Vector3I.One;
                    range = new Range3I(min, selectionSize);
                    center = (Vector3)selectionSize / 2;
                }
                else
                {
                    range = new Range3I { Size = frame.ChunkSize };
                    center = (Vector3)frame.ChunkSize / 2;
                }

                var radius = frame.ChunkSize / 2;

                foreach (var position in range)
                {
                    var point = position + new Vector3(0.5f, 0.5f, 0.5f);
                    
                    if (IsInsideEllipsoid(center, radius, point ))
                        frame.SetBlock((Vector3I)point, (byte)(_selectedColorIndex + 1));
                }

                RebuildFrameVertices();
            }
        }

        private bool IsInsideEllipsoid(Vector3 center, Vector3 radius, Vector3 point)
        {
            var dx = point.X - center.X;
            var dy = point.Y - center.Y;
            var dz = point.Z - center.Z;

            return dx * dx / ( radius.X * radius.X ) + 
                   dy * dy / ( radius.Y * radius.Y ) +
                   dz * dz / ( radius.Z * radius.Z ) <= 1;
        }

        private void OnSpherePresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                if (frame.BlockBytes == null)
                    frame.SetBlock(new Vector3I(), 0);

                Vector3 center;

                Range3I range;
                if (_selectionStart.HasValue && _selectionEnd.HasValue)
                {
                    var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                    var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                    var selectionSize = max - min + Vector3I.One;
                    range = new Range3I(min, selectionSize);
                    center = (Vector3)selectionSize / 2;
                }
                else
                {
                    range = new Range3I { Size = frame.ChunkSize };
                    center = (Vector3)frame.ChunkSize / 2;
                }

                var radius = Math.Min(Math.Min(center.X, center.Y), center.Z);

                foreach (var position in range)
                {
                    var point = position + new Vector3(0.5f, 0.5f, 0.5f);
                    if (Vector3.Distance(point, center + range.Position) <= radius)
                        frame.SetBlock((Vector3I)point, (byte)(_selectedColorIndex + 1));
                }

                RebuildFrameVertices();
            }
        }
        
        private void OnOutlinePresetPressed()
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                // fill the frame 
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                Range3I range;
                if (_selectionStart.HasValue && _selectionEnd.HasValue)
                {
                    var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                    var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                    var selectionSize = max - min + Vector3I.One;
                    range = new Range3I(min, selectionSize);
                }
                else
                {
                    range = new Range3I { Size = frame.ChunkSize };
                }

                foreach (var position in range)
                {
                    var match = 0;
                    if (position.X == range.Position.X) match++;
                    if (position.Y == range.Position.Y) match++;
                    if (position.Z == range.Position.Z) match++;
                    if (position.X == range.Max.X - 1) match++;
                    if (position.Y == range.Max.Y - 1) match++;
                    if (position.Z == range.Max.Z - 1) match++;
                    if (match >= 2)
                    {
                        frame.SetBlock(position, (byte)(_selectedColorIndex + 1));
                    }
                }


                RebuildFrameVertices();
            }
        }

        #endregion

        private void OnSetToolPosition()
        {
            if (_toolMountPointSelection)
            {
                _toolMountPointSelection = false;
                _cursorMode = false;
                _toolSetPositionButton.Release();
                return;
            }

            var armIndex = _visualVoxelModel.VoxelModel.Parts.FindIndex(p => p.IsArm);

            if (armIndex == -1)
            {
                _gui.MessageBox("Model has not an arm. Choose one first.");
                return;
            }
            
            _toolMountPointSelection = true;
            _cursorMode = true;
            
            var curState = _instance.State.PartsStates[armIndex];

            if (curState.PalmTransform.HasValue)
                _cursorPoint = Vector3.TransformCoordinate(Vector3.Zero, curState.PalmTransform.Value); 
        }

        private void OnRotateTool()
        {
            var arm = _visualVoxelModel.VoxelModel.GetArm();

            if (arm != null && arm.PalmTransform.HasValue)
            {
                var translate = arm.PalmTransform.Value.TranslationVector;
                var rotation = Quaternion.RotationMatrix(arm.PalmTransform.Value);

                rotation *= Quaternion.RotationMatrix(Matrix.RotationX(0.1f));

                arm.PalmTransform = Matrix.RotationQuaternion(rotation) * Matrix.Translation(translate);
            }
        }

        #endregion

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (elapsedTime == 0)
                elapsedTime = 0.001f;
            if (_instance != null)
                _instance.Interpolation(elapsedTime);
        }

        private void UpdateCamera()
        {
            if (_visualVoxelModel == null)
                return;

            switch (Mode)
            {
                case EditorMode.MainView:
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
                    }
                    break;
                case EditorMode.FrameEdit:
                    if (_selectedPartIndex != -1 && _selectedFrameIndex != -1 && _selectedFrameIndex != byte.MaxValue)
                    {
                        var frame = _visualVoxelModel.VoxelModel.Frames[_selectedFrameIndex];
                        var box = new BoundingBox(new Vector3(), frame.BlockData.ChunkSize);
                        UpdateTransformMatrix(_currentViewData, box);
                    }
                    break;
            }
        }

        private IEnumerable<Vector3I> GetSelectedCubes(bool newCubes = false)
        {
            if (_newCube.HasValue || _pickedCube.HasValue)
            {
                var pos = _pickedCube.HasValue ? _pickedCube.Value : _newCube.Value;

                if (newCubes) pos = _newCube.Value;

                yield return pos;

                if (_mirror != EditorAxis.None)
                {
                    if (SelectedPartIndex != -1 && SelectedFrameIndex != -1)
                    {
                        var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex];

                        if (_mirror.HasFlag(EditorAxis.X))
                        {
                            yield return new Vector3I(frame.BlockData.ChunkSize.X - pos.X - 1, pos.Y, pos.Z);
                        }
                        if (_mirror.HasFlag(EditorAxis.Y))
                        {
                            yield return new Vector3I(pos.X, frame.BlockData.ChunkSize.Y - pos.Y - 1, pos.Z);
                        }
                        if (_mirror.HasFlag(EditorAxis.Z))
                        {
                            yield return new Vector3I(pos.X, pos.Y, frame.BlockData.ChunkSize.Z - pos.Z - 1);
                        }

                        if (_mirror.HasFlag(EditorAxis.X | EditorAxis.Y))
                        {
                            yield return
                                new Vector3I(frame.BlockData.ChunkSize.X - pos.X - 1,
                                             frame.BlockData.ChunkSize.Y - pos.Y - 1, pos.Z);
                        }
                        if (_mirror.HasFlag(EditorAxis.X | EditorAxis.Z))
                        {
                            yield return
                                new Vector3I(frame.BlockData.ChunkSize.X - pos.X - 1, pos.Y,
                                             frame.BlockData.ChunkSize.Z - pos.Z - 1);
                        }
                        if (_mirror.HasFlag(EditorAxis.Z | EditorAxis.Y))
                        {
                            yield return
                                new Vector3I(pos.X, frame.BlockData.ChunkSize.Y - pos.Y - 1,
                                             frame.BlockData.ChunkSize.Z - pos.Z - 1);
                        }

                        if (_mirror.HasFlag(EditorAxis.X | EditorAxis.Y | EditorAxis.Z))
                        {
                            yield return
                                new Vector3I(frame.BlockData.ChunkSize.X - pos.X - 1,
                                             frame.BlockData.ChunkSize.Y - pos.Y - 1,
                                             frame.BlockData.ChunkSize.Z - pos.Z - 1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="timeSpent">Provides a snapshot of timing values.</param>
        public override void FTSUpdate( GameTime timeSpent)
        {
            if (_visualVoxelModel == null || !_d3DEngine.HasFocus || DialogHelper.DialogBg.Parent != null || GuiManager.DialogClosed) 
                return;

            if ( _gui.Screen.IsMouseOverGui)
            {
                return;
            }

            _flipAxis = _inputManager.KeyboardManager.CurKeyboardState.IsKeyDown(Keys.ShiftKey);

            var dx = (_inputManager.MouseManager.MouseMoveDelta.X) / 100.0f;
            var dy = (_inputManager.MouseManager.MouseMoveDelta.Y) / 100.0f;
            
            if (_inputManager.MouseManager.CurMouseState.MiddleButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed &&
                _inputManager.MouseManager.PrevMouseState.X != 0 &&
                _inputManager.MouseManager.PrevMouseState.Y != 0)
            {
                if (_inputManager.KeyboardManager.CurKeyboardState.IsKeyDown(Keys.ShiftKey))
                {
                    // translate
                    _currentViewData.Translate.X += dx;
                    _currentViewData.Translate.Y += dy;
                }
                else
                {
                    // rotate
                    _currentViewData.RotateX -= dx;
                    _currentViewData.RotateY -= dy;
                }
            }

            _currentViewData.Scale -= (_inputManager.MouseManager.ScroolWheelDeltaValue / 10000.0f);

            switch (Mode)
            {
                case EditorMode.MainView:
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

                            // get the translation plane
                            if (_cursorMode)
                            {
                                _translatePlane = new Plane(_cursorPoint,
                                                            _flipAxis ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0));
                            }
                            else
                            {
                                var center = bb.GetCenter() + partState.Translation;
                                _translatePlane = new Plane(center,
                                                            _flipAxis ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0));
                            }

                            if (_inputManager.MouseManager.CurMouseState.LeftButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Released)
                            {
                                _translatePoint = null;
                                // recalculate state bounding box
                                state.UpdateBoundingBox();
                            }

                            if (_inputManager.MouseManager.CurMouseState.LeftButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
                            {
                                // get the intersection between plane and mouse screen ray
                                Vector3D mPosition, mLookAt;
                                var worldViewProjection = _transform * _viewProjection;
                                _inputManager.MouseManager.UnprojectMouseCursor(ref worldViewProjection, out mPosition, out mLookAt);
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

                                if (_inputManager.KeyboardManager.CurKeyboardState.IsKeyDown(Keys.ControlKey))
                                {
                                    translationVector.X = (float)Math.Round(translationVector.X, 1);
                                    translationVector.Y = (float)Math.Round(translationVector.Y, 1);
                                    translationVector.Z = (float)Math.Round(translationVector.Z, 1);
                                    if (Math.Abs(translationVector.X) >= 0.1f ||
                                        Math.Abs(translationVector.Y) >= 0.1f ||
                                        Math.Abs(translationVector.Z) >= 0.1f)
                                    {
                                        _translatePoint = intersectPoint;
                                    }
                                }
                                else
                                {
                                    translationVector.X = (int)translationVector.X;
                                    translationVector.Y = (int)translationVector.Y;
                                    translationVector.Z = (int)translationVector.Z;

                                    if (Math.Abs(translationVector.X) >= 1 || 
                                        Math.Abs(translationVector.Y) >= 1 ||
                                        Math.Abs(translationVector.Z) >= 1)
                                    {

                                        _translatePoint = intersectPoint;
                                    }
                                }

                                if (translationVector.Length() > 3f)
                                {
                                    _translatePoint = intersectPoint;
                                    translationVector = Vector3.Zero;
                                }

                                if (_cursorMode)
                                {
                                    _cursorPoint += translationVector;

                                    if (_toolMountPointSelection)
                                    {
                                        var armIndex = _visualVoxelModel.VoxelModel.Parts.FindIndex(p => p.IsArm);
                                        if (armIndex != -1)
                                        {
                                            var armState = _visualVoxelModel.VoxelModel.States[0].PartsStates[armIndex];
                                            var partOffset = armState.Translation;
                                            var prevRotation = Quaternion.Identity;
                                            if (armState.PalmTransform.HasValue)
                                                prevRotation = Quaternion.RotationMatrix(armState.PalmTransform.Value);

                                            armState.PalmTransform = Matrix.RotationQuaternion(prevRotation) * Matrix.Translation(_cursorPoint - partOffset);
                                        }
                                    }
                                }
                                else
                                {
                                    var translationMatrix = Matrix.Translation(translationVector);
                                    partState.Translation += translationVector;
                                    partState.BoundingBox = new BoundingBox(
                                        Vector3.TransformCoordinate(partState.BoundingBox.Minimum, translationMatrix),
                                        Vector3.TransformCoordinate(partState.BoundingBox.Maximum, translationMatrix));
                                    NeedSave();
                                    _instance.UpdateStates();
                                }
                            }
                        }
                    }
                    break;
                case EditorMode.FrameEdit:
                    if (_selectedPartIndex != -1 && _selectedFrameIndex != -1)
                    {
                        var frame = _visualVoxelModel.VoxelModel.Frames[_selectedFrameIndex];
                        var box = new BoundingBox(new Vector3(), frame.BlockData.ChunkSize);
                        UpdateTransformMatrix(_currentViewData, box);

                        GetSelectedCube(out _pickedCube, out _newCube);

                        if (_inputManager.MouseManager.CurMouseState.LeftButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Released && 
                            _inputManager.MouseManager.PrevMouseState.LeftButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
                        {
                            if (_pickedCube.HasValue)
                            {
                                foreach (var cubePos in GetSelectedCubes())
                                {
                                    switch (_frameEditorTool)
                                    {
                                        case FrameEditorTools.Edit:
                                            frame.BlockData.SetBlock(cubePos, 0);
                                            break;
                                        case FrameEditorTools.ColorBrush:
                                            frame.BlockData.SetBlock(cubePos, (byte) (_selectedColorIndex + 1));
                                            break;
                                        case FrameEditorTools.FillBrush:
                                            {
                                                // color fill 
                                                var fillIndex = frame.BlockData.GetBlock(cubePos);
                                                var fillWith = (byte) (_selectedColorIndex + 1);
                                                // recursive change all adjacent cubes
                                                if (fillIndex != fillWith)
                                                    ColorFill(frame, cubePos, fillIndex, fillWith);
                                            }
                                            break;
                                        case FrameEditorTools.SliceBrush:
                                            {
                                                // slice fill
                                                var fillWith = (byte) (_selectedColorIndex + 1);

                                                switch (_sliceAxis)
                                                {
                                                    case EditorAxis.X:
                                                        for (int y = 0; y < frame.BlockData.ChunkSize.Y; y++)
                                                        {
                                                            for (int z = 0; z < frame.BlockData.ChunkSize.Z; z++)
                                                            {
                                                                if (
                                                                    frame.BlockData.GetBlock(cubePos.X, y, z) !=
                                                                    0)
                                                                    frame.BlockData.SetBlock(
                                                                        new Vector3I(cubePos.X, y, z),
                                                                        fillWith);
                                                            }
                                                        }
                                                        break;
                                                    case EditorAxis.Y:
                                                        for (int x = 0; x < frame.BlockData.ChunkSize.X; x++)
                                                        {
                                                            for (int z = 0; z < frame.BlockData.ChunkSize.Z; z++)
                                                            {
                                                                if (
                                                                    frame.BlockData.GetBlock(x, cubePos.Y, z) !=
                                                                    0)
                                                                    frame.BlockData.SetBlock(
                                                                        new Vector3I(x, cubePos.Y, z),
                                                                        fillWith);
                                                            }
                                                        }
                                                        break;
                                                    case EditorAxis.Z:
                                                        for (int y = 0; y < frame.BlockData.ChunkSize.Y; y++)
                                                        {
                                                            for (int x = 0; x < frame.BlockData.ChunkSize.X; x++)
                                                            {
                                                                if (
                                                                    frame.BlockData.GetBlock(x, y, cubePos.Z) !=
                                                                    0)
                                                                    frame.BlockData.SetBlock(
                                                                        new Vector3I(x, y, cubePos.Z),
                                                                        fillWith);
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        throw new ArgumentOutOfRangeException();
                                                }

                                            }
                                            break;
                                        case FrameEditorTools.Selection : // selection tool

                                            if (_selectionEnd.HasValue || !_selectionStart.HasValue)
                                            {
                                                _selectionStart = _pickedCube.Value;
                                                _selectionEnd = null;
                                            }
                                            else
                                            {
                                                _selectionEnd = _pickedCube.Value;
                                            }

                                            break;
                                    }
                                }

                                RebuildFrameVertices();
                                NeedSave();
                            }
                        }
                        else if (_inputManager.MouseManager.CurMouseState.RightButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Released && 
                                 _inputManager.MouseManager.PrevMouseState.RightButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
                        {
                            if (_newCube.HasValue)
                            {
                                foreach (var cubePos in GetSelectedCubes(true))
                                {
                                    switch (_frameEditorTool)
                                    {
                                        case FrameEditorTools.Edit:
                                            frame.BlockData.SetBlock(cubePos, (byte)(_selectedColorIndex + 1));
                                            break;
                                        case FrameEditorTools.ColorBrush:
                                            break;
                                        case FrameEditorTools.FillBrush:
                                            break;
                                        case FrameEditorTools.Selection: // selection tool

                                            if (_selectionEnd.HasValue || !_selectionStart.HasValue)
                                            {
                                                _selectionStart = _newCube.Value;
                                                _selectionEnd = null;
                                            }
                                            else
                                            {
                                                _selectionEnd = _newCube.Value;
                                            }

                                            break;
                                    }
                                }
                                RebuildFrameVertices();
                                NeedSave();
                            }
                        }
                        
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            base.FTSUpdate(timeSpent);
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

                        if (HaveSelection())
                        {
                            var range = GetSelectionRange();

                            if (!range.Contains(checkVector))
                                continue;
                        }

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

            _inputManager.MouseManager.UnprojectMouseCursor(ref worldViewProjection, out mPosition, out mLookAt);

            if (double.IsNaN(mPosition.X) || double.IsNaN(mLookAt.X))
            {
                cubePosition = null;
                newCubePosition = null;
                return;
            }

            var blocks = _visualVoxelModel.VoxelModel.Frames[_selectedFrameIndex].BlockData;

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
            if (_visualVoxelModel != null && _selectedFrameIndex != -1)
            {
                _visualVoxelModel.RebuildFrame(_selectedFrameIndex);

                //var sw = Stopwatch.StartNew();
                //_frameShapeBBoxes = VoxelMeshFactory.GenerateShapeBBoxes(_visualVoxelModel.VoxelModel.Parts[_selectedPartIndex].Frames[_selectedFrameIndex].BlockData);
                //sw.Stop();

                //logger.Debug("Shape bb generation time {0} ms", sw.ElapsedMilliseconds);
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
        public override void Draw(DeviceContext context, int index)
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
                case EditorMode.MainView: DrawModelView(context); break;
                case EditorMode.ModelLayout: DrawModelLayout(context); break;
                case EditorMode.FrameEdit: DrawFrameEdit(context); break;
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
            //RenderStatesRepo.ApplyStates(_renderRasterId, _blendStateId, _depthStateWithDepthId);
            RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext,DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = color;
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(size) * Matrix.Translation(min) * _transform); //Matrix.Translation(new Vector3(-0.5f,-0.5f,-0.5f)) *
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            //Set the vertex buffer to the Graphical Card.
            _boxVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);
            _boxIndexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.DrawIndexed(24, 0, 0); 
        }

        private void DrawRotationAxis(Vector3 position)
        {
            RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext ,DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            var transformX =  Matrix.Scaling(16) * Matrix.Translation(position) * _transform;
            var transformY = Matrix.RotationAxis(new Vector3(0, 0, 1), (float)Math.PI / 2) * Matrix.Scaling(16) * Matrix.Translation(position) * _transform;
            var transformZ = Matrix.RotationAxis(new Vector3(0, 1, 0), (float)Math.PI / 2) * Matrix.Scaling(16) * Matrix.Translation(position) * _transform;

            //Set the vertex buffer to the Graphical Card.
            _rotationVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 0, 0, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transformX);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);
            
            _d3DEngine.ImmediateContext.Draw(2, 0);

            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(0, 1, 0, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transformY);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            _d3DEngine.ImmediateContext.Draw(2, 0);

            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(0, 0, 1, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transformZ);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            _d3DEngine.ImmediateContext.Draw(2, 0); 



        }

        private void DrawCrosshair(Vector3 position, float size, bool turnAxis)
        {
            RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            var transform = Matrix.Scaling(size) * Matrix.Translation(position) * _transform;

            if (turnAxis)
            {
                transform = Matrix.RotationY((float)Math.PI / 2) * transform;
            }

            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(0, 1, 0, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transform);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            //Set the vertex buffer to the Graphical Card.
            _crosshairVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.Draw(4, 0); 
        }

        private void DrawTool(VoxelModelPartState armState)
        {
            var palmMatrix = Matrix.Identity;

            if (armState.PalmTransform.HasValue)
                palmMatrix = armState.PalmTransform.Value;

            RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            var transform = Matrix.Scaling(16) * palmMatrix * armState.GetTransformation()  * _transform;
            
            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 1, 0, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transform);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            //Set the vertex buffer to the Graphical Card.
            _toolVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.Draw(4, 0); 
        }



        private void DrawDirection()
        {
            RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            var transform = Matrix.Scaling(16) * _transform;
            
            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 1, 1, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transform);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            //Set the vertex buffer to the Graphical Card.
            _directionVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.Draw(6, 0);

            transform = Matrix.Scaling(16) * Matrix.Translation(new Vector3(0,16,0)) * _transform;
            //Set Effect variables
            _lines3DEffect.Begin(_d3DEngine.ImmediateContext);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 1, 1, 1);
            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(transform);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(_d3DEngine.ImmediateContext);

            //Set the vertex buffer to the Graphical Card.
            _directionVertexBuffer.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.Draw(6, 0); 

            var box = new BoundingBox(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 1, 0.5f));
            box = box.Transform(Matrix.Scaling(16));
            DrawBox(box, new Color4(1, 1, 1, 1));
        }

        private void DrawModelView(DeviceContext context)
        {
            // draw the model
            if (_visualVoxelModel != null)
            {
                RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);

                //var direction = new Vector3(0.3f, 0.2f, 0.8f);
                var direction = new Vector3(0f, 0f, 1f);
                direction.Normalize();
                _voxelInstancedEffect.Begin(context);
                _voxelInstancedEffect.CBPerFrame.Values.LightDirection = direction;
                _voxelInstancedEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
                _voxelInstancedEffect.CBPerFrame.IsDirty = true;

                _instance.LightColor = new Color3(1, 1, 1);
                _instance.World = _transform;

                _visualVoxelModel.DrawInstanced(context, _voxelInstancedEffect, new[] { _instance });
            }
        }

        private void DrawModelLayout(DeviceContext context)
        {
            if (_visualVoxelModel != null)
            {
                // draw 0,0
                DrawDirection();

                // draw each part with bounding box

                var state = _visualVoxelModel.VoxelModel.States[SelectedStateIndex];

                //DrawBox(state.BoundingBox);

                RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);

                var model = _visualVoxelModel.VoxelModel;
                var visualFrames = _visualVoxelModel.VisualVoxelFrames;

                _voxelEffect.CBPerModel.Values.LightColor = new Color3(1, 1, 1);
                
                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerModel.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerModel.IsDirty = true;
                }

                // draw each part of the model
                for (int i = 0; i < state.PartsStates.Count; i++)
                {
                    var voxelModelPartState = state.PartsStates[i];

                    if (voxelModelPartState.ActiveFrame == byte.MaxValue)
                        continue;

                    var vb = visualFrames[voxelModelPartState.ActiveFrame].VertexBuffer;
                    var ib = visualFrames[voxelModelPartState.ActiveFrame].IndexBuffer;

                    _voxelEffect.Begin(context);
                    _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
                    _voxelEffect.CBPerFrame.IsDirty = true;

                    vb.SetToDevice(context, 0);
                    ib.SetToDevice(context, 0);

                    var colorMapping = model.Frames[voxelModelPartState.ActiveFrame].ColorMapping;

                    if (colorMapping != null)
                    {
                        _voxelEffect.CBPerModel.Values.ColorMapping = colorMapping.BlockColors;
                    }

                    _voxelEffect.CBPerModel.Values.World = Matrix.Transpose(_transform);
                    _voxelEffect.CBPerModel.IsDirty = true;

                    _voxelEffect.CBPerPart.Values.Transform = Matrix.Transpose(voxelModelPartState.GetTransformation());
                    _voxelEffect.CBPerPart.IsDirty = true;
                    _voxelEffect.Apply(context);

                    context.DrawIndexed(ib.IndicesCount, 0, 0);
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
                        var translate = bb.GetCenter();

                        if (_toolMountPointSelection)
                        {
                            var arm = _visualVoxelModel.VoxelModel.GetArm();

                            if (arm != null)
                                DrawTool(arm);
                        }
                        else if (_layoutTool == LayoutTool.Rotate)
                        {
                            if (_displayLayoutRotationPosition)
                            {
                                DrawCrosshair(_cursorPoint, sizef, _flipAxis);
                            }
                            else
                            {
                                DrawRotationAxis(_cursorPoint);
                            }
                        }
                        else
                        {
                            DrawCrosshair(translate, sizef, _flipAxis);
                        }
                    }
                }

                DrawBox(state.BoundingBox, new Color4(1, 1, 1, 0.1f));
            }
        }

        private void DrawGrid(DeviceContext context)
        {
            _lines3DEffect.Begin(context);
            _lines3DEffect.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_viewProjection);
            _lines3DEffect.CBPerDraw.Values.Color = new Color4(1, 1, 1, 0.1f);

            // x-z

            var eye = new Vector3(0,0,5);
            var point = new Vector3(1, 0, 1);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationX((float)Math.PI/2) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);

            var bottomNormal = Vector3.TransformNormal(_gridBottomNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            var viewVector = eye - point;
            var dot = Vector3.Dot(viewVector, bottomNormal);
            if (dot > 0)
            {
                _xGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_xGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(0, _gridSize.Y, 0);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation(0,_gridSize.Y,0) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);

            var topNormal = Vector3.TransformNormal(_gridTopNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, topNormal);
            if (dot > 0)
            {
                _xGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_xGridVertextBuffer.VertexCount, 0);
            }
            // x-y

            point = new Vector3();

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(_transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);
            var backNormal = Vector3.TransformNormal(_gridBackNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, backNormal);
            if (dot > 0)
            {
                _yGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_yGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(0, 0, _gridSize.Z);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Translation(0, 0, _gridSize.Z) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);
            var frontNormal = Vector3.TransformNormal(_gridFrontNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, frontNormal);
            if (dot > 0)
            {
                _yGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_yGridVertextBuffer.VertexCount, 0);
            }
            // z-y

            point = new Vector3();

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationY(-(float)Math.PI / 2) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);

            var leftNormal = Vector3.TransformNormal(_gridLeftNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, leftNormal);
            if (dot > 0)
            {
                _zGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_zGridVertextBuffer.VertexCount, 0);
            }

            point = new Vector3(_gridSize.X,0,0);

            _lines3DEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(_gridSize.X, 0 , 0) * _transform);
            _lines3DEffect.CBPerDraw.IsDirty = true;
            _lines3DEffect.Apply(context);

            var rightNormal = Vector3.TransformNormal(_gridRightNormal, _transform);
            point = Vector3.TransformCoordinate(point, _transform);
            viewVector = eye - point;
            dot = Vector3.Dot(viewVector, rightNormal);
            if (dot > 0)
            {
                _zGridVertextBuffer.SetToDevice(context, 0);
                context.Draw(_zGridVertextBuffer.VertexCount, 0);
            }
        }

        private void DrawFrameEdit(DeviceContext context)
        {
            if (_visualVoxelModel != null && SelectedPartIndex != -1 && SelectedFrameIndex != -1)
            {
                if (IsSelectionApplicable())
                {
                    if (GetSelectedCubes().Any())
                    {
                        var selection = GetSelectedCubes().First();
                        DrawBox(new BoundingBox(selection, new Vector3(1f) + selection), new Color4(1, 0, 0, 1f));
                    }

                    if (HaveSelection())
                    {
                        var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                        var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);

                        DrawBox(new BoundingBox(min, max + Vector3.One), new Color4(0, 1, 0, 1f));
                    }
                    else if (_selectionStart.HasValue)
                    {
                        DrawBox(new BoundingBox(_selectionStart.Value, _selectionStart.Value + Vector3.One), new Color4(0, 1, 0, 1f));
                    }
                }
                else
                {
                    foreach (var mirrorCube in GetSelectedCubes())
                    {
                        DrawBox(new BoundingBox(new Vector3(-0.01f) + mirrorCube, new Vector3(1.01f) + mirrorCube),
                                new Color4(1, 0, 0, 1f));
                        DrawBox(new BoundingBox(new Vector3(0.01f) + mirrorCube, new Vector3(0.99f) + mirrorCube),
                                new Color4(1, 0, 0, 1f));
                    }
                }

                // draw shape bboxes
                if (_frameShapeBBoxes != null)
                {
                    foreach (var frameShapeBBox in _frameShapeBBoxes)
                    {
                        DrawBox(frameShapeBBox, new Color4(1, 1, 1, 0.5f));
                    }
                }

                RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

                #region Grid
                var frame = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex];
                if (_gridSize != frame.BlockData.ChunkSize)
                {
                    InitPlanes(frame.BlockData.ChunkSize);
                    BuildGrid(frame.BlockData.ChunkSize);
                }

                DrawGrid(context);

                #endregion

                var model = _visualVoxelModel.VoxelModel;
                var visualParts = _visualVoxelModel.VisualVoxelFrames;

                _voxelEffect.CBPerModel.Values.LightColor = new Color3(1, 1, 1);

                RenderStatesRepo.ApplyStates(_d3DEngine.ImmediateContext, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
                if (model.ColorMapping != null)
                {
                    _voxelEffect.CBPerModel.Values.ColorMapping = model.ColorMapping.BlockColors;
                    _voxelEffect.CBPerModel.IsDirty = true;
                }

                var vb = visualParts[SelectedFrameIndex].VertexBuffer;
                var ib = visualParts[SelectedFrameIndex].IndexBuffer;

                _voxelEffect.Begin(context);
                _voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_viewProjection);
                _voxelEffect.CBPerFrame.Values.LightDirection = new Vector3(1, 1, 1);
                _voxelEffect.CBPerFrame.IsDirty = true;
                _voxelEffect.Apply(context);

                vb.SetToDevice(context, 0);
                ib.SetToDevice(context, 0);

                var colorMapping = model.Frames[SelectedFrameIndex].ColorMapping;

                if (colorMapping != null)
                {
                    _voxelEffect.CBPerModel.Values.ColorMapping = colorMapping.BlockColors;
                }

                _voxelEffect.CBPerModel.Values.World = Matrix.Transpose(_transform);
                _voxelEffect.CBPerModel.IsDirty = true;

                _voxelEffect.CBPerPart.Values.Transform = Matrix.Transpose(Matrix.Identity);
                _voxelEffect.CBPerPart.IsDirty = true;
                _voxelEffect.Apply(context);

                context.DrawIndexed(ib.IndicesCount, 0, 0);
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
                _xGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, list.Count, PrimitiveTopology.LineList, "EditorXGrid");
                _xGridVertextBuffer.SetData(_d3DEngine.ImmediateContext, list.ToArray());
            }

            if (_gridSize.X != size.X || _gridSize.Y != size.Y)
            {
                if (_yGridVertextBuffer != null)
                    _yGridVertextBuffer.Dispose();
                var list = new List<VertexPosition>();
                FillGrid(list, size.X, size.Y);
                _yGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, list.Count, PrimitiveTopology.LineList, "EditorYGrid");
                _yGridVertextBuffer.SetData(_d3DEngine.ImmediateContext ,list.ToArray());
            }

            if (_gridSize.Z != size.Z || _gridSize.Y != size.Y)
            {
                if (_zGridVertextBuffer != null)
                    _zGridVertextBuffer.Dispose();
                var list = new List<VertexPosition>();
                FillGrid(list, size.Z, size.Y);
                _zGridVertextBuffer = new VertexBuffer<VertexPosition>(_d3DEngine.Device, list.Count, PrimitiveTopology.LineList, "EditorYGrid");
                _zGridVertextBuffer.SetData(_d3DEngine.ImmediateContext, list.ToArray());
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

        public override void BeforeDispose()
        {
            if (_boxVertexBuffer != null) _boxVertexBuffer.Dispose();
            if (_boxIndexBuffer != null) _boxIndexBuffer.Dispose();
            if (_crosshairVertexBuffer != null) _crosshairVertexBuffer.Dispose();
            _lines3DEffect.Dispose();
            _voxelInstancedEffect.Dispose();
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
        }

        private void OnSaveClicked()
        {
            if (_visualVoxelModel == null)
            {
                _gui.MessageBox("No model is selected to be saved.", "Error");
                return;
            }

            _manager.SaveModel(_visualVoxelModel);
            _needSave = false;
            _saveButton.Enabled = false;
        }

        private void NeedSave()
        {
            _needSave = true;
            _saveButton.Enabled = true;
        }

        private void OnExport()
        {
            if (_visualVoxelModel == null)
            {
                _gui.MessageBox("No model is selected to be exported.", "Error");
                return;
            }
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia",
                                        _visualVoxelModel.VoxelModel.Name + ".uvm");
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(path))
                    File.Delete(path);

                _visualVoxelModel.VoxelModel.SaveToFile(path);

                //var previewPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia", _visualVoxelModel.VoxelModel.Name + ".png");
                //using (var tex2d = _iconFactory.CreateVoxelIcon(_visualVoxelModel, new DrawingSize { Width = 64, Height = 64 }))
                //    Resource.ToFile(_d3DEngine.ImmediateContext, tex2d, ImageFileFormat.Png, previewPath);

                _gui.MessageBox("Model saved at " + path, "Success");
            }
            catch (Exception x)
            {
                _gui.MessageBox(x.Message,"Error");
            }
        }

        private void OnRenderPng()
        {
            if (_visualVoxelModel == null)
            {
                _gui.MessageBox("No model is selected to be rendered.", "Error");
                return;
            }
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia",
                        _visualVoxelModel.VoxelModel.Name + ".png");
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(path))
                File.Delete(path);

            RenderPng(path, true);
        }


        private void OnExportAll()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");
            var dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            int count = 0;
            foreach (var visualVoxelModel in _manager.Enumerate())
            {
                var modelPath = Path.Combine(path, visualVoxelModel.VoxelModel.Name + ".uvm");
                var previewPath = Path.Combine(path, visualVoxelModel.VoxelModel.Name + ".png");
                visualVoxelModel.VoxelModel.SaveToFile(modelPath);

                // create icon

                using (var tex2d = _iconFactory.CreateVoxelIcon(visualVoxelModel, new Size2 { Width = 128, Height = 128 }))
                    Resource.ToFile(_d3DEngine.ImmediateContext, tex2d, ImageFileFormat.Png, previewPath);

                count++;
            }
            _gui.MessageBox(count + " models saved", "Success");
        }
        
        private void OnImportAll()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");
            foreach (var file in Directory.GetFiles(path, "*.uvm"))
            {
                try
                {
                    var voxelModel = VoxelModel.LoadFromFile(file);
                    var visualModel = new VisualVoxelModel(voxelModel, _meshFactory);
                    _manager.SaveModel(visualModel);
                }
                catch (InvalidDataException x)
                {
                    logger.Error("Unable to load model: " + x.Message);
                }
            }

            _modelsList.Items.Clear();
            foreach (var model in _manager.Enumerate().OrderBy(x => x.VoxelModel.Name))
            {
                _modelsList.Items.Add(model);
            }

            _modelsList.SelectedItems.Clear();
            _modelsList.SelectedItems.Add(0);
        }

        private void OnImport()
        {
            var e = new DialogImportModelStruct();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");

            e.Files = new DialogSelection {SelectedIndex = -1, Elements = Directory.GetFiles(path, "*.uvm").Select(Path.GetFileName)};
            _importDialog.ShowDialog(_screen, _d3DEngine.ViewPort, e, "Select file to import", OnModelImported);
        }
        
        private void OnModelImported(DialogImportModelStruct e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");
            var files = Directory.GetFiles(path, "*.uvm");

            try
            {
                var file = files[e.Files.SelectedIndex];
                var voxelModel = VoxelModel.LoadFromFile(file);
                var visualModel = new VisualVoxelModel(voxelModel, _meshFactory);
                _manager.SaveModel(visualModel);

                UpdateModelsList(visualModel);
                
                _gui.MessageBox("Model imported", "Success");
            }
            catch (Exception x)
            {
                _gui.MessageBox(x.Message, "Error");
            }
        }

        private void UpdateModelsList(VisualVoxelModel selected = null)
        {
            _modelsList.Items.Clear();
            var index = 0;
            int i = 0;
            foreach (var model in _manager.Enumerate().OrderBy(m => m.VoxelModel.Name))
            {
                _modelsList.Items.Add(model);
                if (model == selected)
                    index = i;
                i++;
            }

            _modelsList.SelectedItems.Clear();
            _modelsList.SelectedItems.Add(index);
        }

        private void OnLoadServerModels()
        {
            WebApi.GetModelsListAsync(m => 
            {
                foreach (var model in m.Models)
                {
                    _manager.GetModel(model.Name);     
                }
            });
        }

        private void OnPublish()
        {
            if (_visualVoxelModel == null)
            {
                _gui.MessageBox("No model is selected to be published.", "Error");
                return;
            }
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia",
                                        _visualVoxelModel.VoxelModel.Name + ".uvm");
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(path))
                    File.Delete(path);

                if (string.IsNullOrEmpty(_visualVoxelModel.VoxelModel.Author))
                {
                    _visualVoxelModel.VoxelModel.Author = Author;
                }

                _visualVoxelModel.VoxelModel.SaveToFile(path);

                var imgPath = Path.ChangeExtension(path, ".png");

                RenderPng(imgPath);

                WebApi.UploadModel(path);

                _gui.MessageBox("Model published successfully.");
            }
            catch (Exception x)
            {
                _gui.MessageBox(x.Message, "Error");
            }
        }

        private void RenderPng(string filePath, bool customRotation = false)
        {
            using (var tex2d = _iconFactory.CreateVoxelIcon(_visualVoxelModel, new Size2 { Width = 2048, Height = 2048 }, null, null, !customRotation ? default(Matrix) : _transform * Matrix.RotationAxis(new Vector3(0,1,0),(float) Math.PI) ))
                Resource.ToFile(_d3DEngine.ImmediateContext, tex2d, ImageFileFormat.Png, filePath);

            Image img;

            using (var fs = File.OpenRead(filePath))
                img = Image.FromStream(fs);

            var newImage = new Bitmap(512, 512);
            using (var gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(img, new System.Drawing.Rectangle(0, 0, 512, 512));
            }

            newImage.Save(filePath, ImageFormat.Png);
            _gui.MessageBox("Image was saved to 'MyDocuments\\Utopia'");
        }

        private void OnPublishAll()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");
            var dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            int count = 0;
            foreach (var visualVoxelModel in _manager.Enumerate())
            {
                var modelPath = Path.Combine(path, visualVoxelModel.VoxelModel.Name + ".uvm");
                var previewPath = Path.Combine(path, visualVoxelModel.VoxelModel.Name + ".png");

                if (string.IsNullOrEmpty(_visualVoxelModel.VoxelModel.Author))
                {
                    _visualVoxelModel.VoxelModel.Author = Author;
                }

                visualVoxelModel.VoxelModel.SaveToFile(modelPath);

                // create icon

                using (var tex2d = _iconFactory.CreateVoxelIcon(visualVoxelModel, new Size2 { Width = 512, Height = 512 }))
                    Resource.ToFile(_d3DEngine.ImmediateContext, tex2d, ImageFileFormat.Png, previewPath);

                WebApi.UploadModel(modelPath);

                count++;
            }
            _gui.MessageBox(count + " models uploaded", "Success");
        }

        private void OnPartRotation(EditorAxis editorAxis)
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Select part to rotate");
                return;
            }

            float rotationAngle;
            if (!float.TryParse(_rotateAngleInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out rotationAngle))
            {
                _gui.MessageBox("Invalid angle value");
                return;
            }

            var state = VisualVoxelModel.VoxelModel.States[SelectedStateIndex];
            var ps = state.PartsStates[SelectedPartIndex];

            // convert to radians 
            rotationAngle = (float)((Math.PI / 180) * rotationAngle);

            var translation = _cursorPoint - ps.Translation;
            
            switch (editorAxis)
            {
                case EditorAxis.None:
                    ps.Rotation = Quaternion.Identity;
                    break;
                case EditorAxis.X:
                    ps.RotationOffset = translation;
                    ps.Rotation = ps.Rotation * Quaternion.RotationMatrix(Matrix.Translation(-translation) * Matrix.RotationX(rotationAngle) * Matrix.Translation(translation));
                    break;
                case EditorAxis.Y:
                    ps.RotationOffset = translation;
                    ps.Rotation = ps.Rotation * Quaternion.RotationMatrix(Matrix.Translation(-translation) * Matrix.RotationY(rotationAngle) * Matrix.Translation(translation));
                    break;
                case EditorAxis.Z:
                    ps.RotationOffset = translation;
                    ps.Rotation = ps.Rotation * Quaternion.RotationMatrix(Matrix.Translation(-translation) * Matrix.RotationZ(rotationAngle) * Matrix.Translation(translation));
                    break;
            }
            
            state.UpdateBoundingBox();
            NeedSave();
        }

        private void OnLayoutCopy()
        {
            if (SelectedStateIndex == -1)
            {
                _gui.MessageBox("Select state to copy");
                return;
            }

            _clipboardState = new VoxelModelState(VisualVoxelModel.VoxelModel.States[SelectedStateIndex]);
        }

        private void OnLayoutPaste()
        {
            if (SelectedStateIndex == -1)
            {
                _gui.MessageBox("Select state to copy");
                return;
            }

            _clipboardState.Name = VisualVoxelModel.VoxelModel.States[SelectedStateIndex].Name;
            
            VisualVoxelModel.VoxelModel.States[SelectedStateIndex] = _clipboardState;
            _clipboardState = new VoxelModelState(_clipboardState);
            NeedSave();
        }

        private void OnMoveRotationToCenter()
        {
            if (SelectedStateIndex == -1 || SelectedPartIndex == -1)
            {
                _gui.MessageBox("Select state and part");
                return;
            }

            var state = VisualVoxelModel.VoxelModel.States[SelectedStateIndex];

            _cursorPoint = state.PartsStates[SelectedPartIndex].BoundingBox.GetCenter();
        }

        private void OnPartScale(EditorAxis editorAxis)
        {
            if (SelectedPartIndex == -1)
            {
                _gui.MessageBox("Select part to scale");
                return;
            }

            float scaleFactor;
            if (!float.TryParse(_scaleAngleInput.Text, NumberStyles.Float ,CultureInfo.InvariantCulture, out scaleFactor))
            {
                _gui.MessageBox("Invalid factor value");
                return;
            }

            var state = VisualVoxelModel.VoxelModel.States[SelectedStateIndex];
            var ps = state.PartsStates[SelectedPartIndex];
            
            switch (editorAxis)
            {
                case EditorAxis.None:
                    ps.Scale = Vector3.One;
                    break;
                case EditorAxis.X:
                    {
                        var scale = ps.Scale;
                        scale.X *= scaleFactor;
                        ps.Scale = scale;
                    }
                    break;
                case EditorAxis.Y:
                    {
                        var scale = ps.Scale;
                        scale.Y *= scaleFactor;
                        ps.Scale = scale;
                    }
                    break;
                case EditorAxis.Z:
                    {
                        var scale = ps.Scale;
                        scale.Z *= scaleFactor;
                        ps.Scale = scale;
                    }
                    break;
                case EditorAxis.X | EditorAxis.Y | EditorAxis.Z:
                    ps.Scale *= scaleFactor;
                    break;
            }

            state.UpdateBoundingBox();
            NeedSave();
        }

        private bool HaveSelection()
        {
            return _selectionStart.HasValue && _selectionEnd.HasValue;
        }

        private Range3I GetSelectionRange()
        {
            if (_selectionStart == null || _selectionEnd == null)
                return new Range3I(Vector3I.Zero, _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData.ChunkSize);

            var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
            var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);

            var size = max - min + Vector3I.One;

            return new Range3I(min, size);
        }

        private void ClearSelection()
        {
            _selectionStart = null;
            _selectionEnd = null;
        }

        private bool IsSelectionApplicable()
        {
            return _frameEditorTool == FrameEditorTools.Selection || _frameEditorTool == FrameEditorTools.Preset || _frameEditorTool == FrameEditorTools.FillBrush;
        }

        private void OnFrameCopyPressed()
        {
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nothing to copy. Select the part and frame");
                return;
            }
            
            // if we have selection then copy only selected blocks
            if (HaveSelection() && IsSelectionApplicable())
            {
                var range = GetSelectionRange();

                _clipboardBlock = new InsideDataProvider();
                _clipboardBlock.UpdateChunkSize(range.Size);

                var copyFrom = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

                foreach (var vector in range)
                {
                    var clipPosition = vector - range.Position;
                    _clipboardBlock.SetBlock(clipPosition, copyFrom.GetBlock(vector));
                }
            }
            else 
                // copy whole block
                _clipboardBlock = new InsideDataProvider(_visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData);
        }

        private void OnFramePastePressed()
        {
            if (_clipboardBlock == null)
            {
                _gui.MessageBox("Nothing to paste. Copy a buffer first");
                return;
            }
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nowhere to paste. Select the part and frame");
                return;
            }

            var pasteTo = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;
            var size = Vector3I.Min(pasteTo.ChunkSize, _clipboardBlock.ChunkSize);

            _backupBlock = new InsideDataProvider(pasteTo);

            Range3I range;

            if (HaveSelection() && IsSelectionApplicable())
            {
                var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                var selectionSize = Vector3I.Min(size, max - min + Vector3I.One);
                range = new Range3I(min, selectionSize);
            }
            else
            {
                range = new Range3I { Size = size };
            }

            foreach (var position in range)
            {
                var offset = position - range.Position;
                pasteTo.SetBlock(position, _clipboardBlock.GetBlock(offset));
            }

            RebuildFrameVertices();
        }

        private void OnFrameMergePressed()
        {
            if (_clipboardBlock == null)
            {
                _gui.MessageBox("Nothing to paste. Copy a buffer first");
                return;
            }
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nowhere to paste. Select the part and frame");
                return;
            }
            var pasteTo = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;
            var size = Vector3I.Min(pasteTo.ChunkSize, _clipboardBlock.ChunkSize);

            _backupBlock = new InsideDataProvider(pasteTo);

            Range3I range;

            if (HaveSelection() && IsSelectionApplicable())
            {
                var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                var selectionSize = Vector3I.Min(size, max - min + Vector3I.One);
                range = new Range3I(min, selectionSize);
            }
            else
            {
                range = new Range3I { Size = size };
            }

            foreach (var position in range)
            {
                var offset = position - range.Position;
                if (pasteTo.GetBlock(position) != 0)
                {
                    pasteTo.SetBlock(position, _clipboardBlock.GetBlock(offset));
                }
            }

            RebuildFrameVertices();
        }

        private void OnFrameUndoPressed()
        {
            if (_backupBlock == null)
            {
                _gui.MessageBox("Nothing to undo. Do something crappy first ;)");
                return;
            }
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nowhere to return backup. Select a part and a frame");
                return;
            }

            var pasteTo = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;
            var size = Vector3I.Min(pasteTo.ChunkSize, _backupBlock.ChunkSize);

            // paste at 0,0,0
            var range = new Range3I {Size = size};

            foreach (var position in range)
            {
                pasteTo.SetBlock(position, _backupBlock[position]);
            }

            RebuildFrameVertices();

        }

        private void OnFrameBlockDeletePressed()
        {
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nowhere to delete. Select a part and a frame");
                return;
            }
            var pasteTo = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;
            _backupBlock = new InsideDataProvider(pasteTo);

            Range3I range;

            if (HaveSelection() && IsSelectionApplicable())
            {
                var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                var selectionSize = Vector3I.Min(pasteTo.ChunkSize, max - min + Vector3I.One);
                range = new Range3I(min, selectionSize);
            }
            else
            {
                range = new Range3I { Size = pasteTo.ChunkSize };
            }

            foreach (var position in range)
            {
                pasteTo.SetBlock(position, 0);
            }

            RebuildFrameVertices();
        }

        private void OnFrameShift(EditorAxis editorAxis, bool plus)
        {
            if (SelectedPartIndex == -1 || SelectedFrameIndex == -1)
            {
                _gui.MessageBox("Nowhere to shift. Select a part and a frame");
                return;
            }

            var shiftVector = new Vector3I();

            switch (editorAxis)
            {
                case EditorAxis.X:
                    shiftVector = new Vector3I(1, 0, 0);
                    break;
                case EditorAxis.Y:
                    shiftVector = new Vector3I(0, 1, 0);
                    break;
                case EditorAxis.Z:
                    shiftVector = new Vector3I(0, 0, 1);
                    break;
            }

            if (plus) 
                shiftVector = Vector3I.Zero - shiftVector;

            var pasteTo = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;
            _backupBlock = new InsideDataProvider(pasteTo);

            Range3I range;

            if (HaveSelection() && IsSelectionApplicable())
            {
                var min = Vector3I.Min(_selectionStart.Value, _selectionEnd.Value);
                var max = Vector3I.Max(_selectionStart.Value, _selectionEnd.Value);
                var selectionSize = Vector3I.Min(pasteTo.ChunkSize, max - min + Vector3I.One);
                range = new Range3I(min, selectionSize);
            }
            else
            {
                range = new Range3I { Size = pasteTo.ChunkSize };
            }

            foreach (var position in range)
            {
                var shifted = position + shiftVector;
                if (range.Contains(shifted))
                    pasteTo.SetBlock(position, _backupBlock.GetBlock(shifted));
                else
                    pasteTo.SetBlock(position, 0);
            }

            RebuildFrameVertices();
        }

        private void OnFlip(EditorAxis editorAxis)
        {
            Range3I range;

            var blockBuffer = _visualVoxelModel.VoxelModel.Frames[SelectedFrameIndex].BlockData;

            _backupBlock = new InsideDataProvider(blockBuffer);

            if (IsSelectionApplicable())
                range = GetSelectionRange();
            else
                range = new Range3I { Size = blockBuffer.ChunkSize };

            var newBuffer = new InsideDataProvider();
            newBuffer.UpdateChunkSize(range.Size);

            foreach (var pos in range)
            {
                var readFrom = pos - range.Position;

                switch (editorAxis)
                {
                    case EditorAxis.X:
                        readFrom.X = range.Position.X + range.Size.X - readFrom.X - 1;
                        break;
                    case EditorAxis.Y:
                        readFrom.Y = range.Position.Y + range.Size.Y - readFrom.Y - 1;
                        break;
                    case EditorAxis.Z:
                        readFrom.Z = range.Position.Z + range.Size.Z - readFrom.Z - 1;
                        break;
                }

                var value = blockBuffer.GetBlock(readFrom);
                newBuffer.SetBlock(pos - range.Position, value);
            }

            foreach (var pos in range)
            {
                var val = newBuffer.GetBlock(pos - range.Position);
                blockBuffer.SetBlock(pos, val);
            }

            RebuildFrameVertices();
        }
    }

    [Flags]
    internal enum EditorAxis
    {
        None = 0x0,
        X    = 0x1,
        Y    = 0x2,
        Z    = 0x4
    }
}
