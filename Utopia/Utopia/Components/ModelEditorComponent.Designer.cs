using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using Utopia.Shared.Entities.Models;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;

namespace Utopia.Components
{
    public partial class ModelEditorComponent
    {
        private struct DialogImportModelStruct
        {
            public DialogSelection Files;
        }

        private struct DialogAnimationEditStruct
        {
            public string Name;
            public int StartFrame;
        }

        private struct DialogAnimationStepStruct
        {
            public DialogSelection State;
            public int Duration;
        }

        private struct DialogModelEditStruct
        {
            public string Name;
        }

        private struct DialogStateEditStruct
        {
            public string Name;
        }

        private struct DialogPartsEditStruct
        {
            public string Name;
            public bool IsHead;
            public bool IsArm;
        }

        private struct DialogFrameEditStruct
        {
            public string Name;
            public int SizeX;
            public int SizeY;
            public int SizeZ;
            public bool MirrorLeft;
            public bool MirrorRight;
            public bool MirrorTop;
            public bool MirrorBottom;
            public bool MirrorFront;
            public bool MirrorBack;
            public bool TileLeft;
            public bool TileRight;
            public bool TileTop;
            public bool TileBottom;
            public bool TileFront;
            public bool TileBack;
        }

        private ButtonControl _backButton;
        private WindowControl _toolsWindow;
        private WindowControl _modelNavigationWindow;

        private DialogControl<DialogAnimationEditStruct> _animationsEditDialog;
        private DialogControl<DialogAnimationStepStruct> _animationStepDialog;
        private DialogControl<DialogStateEditStruct> _stateEditDialog;
        private DialogControl<DialogModelEditStruct> _modelEditDialog;
        private DialogControl<DialogPartsEditStruct> _partEditDialog;
        private DialogControl<DialogFrameEditStruct> _frameEditDialog;
        private DialogControl<DialogImportModelStruct> _importDialog; 
        private LabelControl _infoLabel;
        private InputControl _rotateAngleInput;
        private InputControl _scaleAngleInput;

        // navigation groups
        private Control _modelsGroup;
        private Control _animationsGroup;
        private Control _statesGroup;
        private Control _partsGroup;
        private Control _framesGroup;

        // tools groups
        private Control _mainToolsGroup;
        private Control _colorsGroup;
        private Control _frameToolsGroup;
        private Control _layoutToolsGroup;
        private Control _modesButtonsGroup;
        private Control _colorPaletteGroup;

        // tool properties groups
        private Control _tpPreset;
        private Control _tpSliceBrush;
        private Control _tpEdit;
        private Control _tpMirror;

        // view properties groups
        LayoutTool _layoutTool;
        private Control _vpLayout;
        private Control _vpRotate;
        private Control _vpScale;


        private List<ColorButtonControl> _colorPalette = new List<ColorButtonControl>();
        
        private ListControl _modelsList;
        private ListControl _animationsList;
        private ListControl _animationStepsList;
        private ListControl _statesList;
        private ListControl _partsList;
        private ListControl _framesList;
        private ButtonControl _saveButton;
        private StickyButtonControl _toolSetPositionButton;


        private void InitializeGui()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };

            _toolsWindow = CreateToolsWindow();
            _modelNavigationWindow = CreateNavigationWindow();
            _animationsEditDialog = new DialogControl<DialogAnimationEditStruct>();
            _animationStepDialog = new DialogControl<DialogAnimationStepStruct>();
            _stateEditDialog = new DialogControl<DialogStateEditStruct>();
            _modelEditDialog = new DialogControl<DialogModelEditStruct>();
            _partEditDialog = new DialogControl<DialogPartsEditStruct>();
            _frameEditDialog = new DialogControl<DialogFrameEditStruct>();
            _importDialog = new DialogControl<DialogImportModelStruct>();
            _infoLabel = new LabelControl { Bounds = new UniRectangle(300, 20, 600, 20) };

            _controls.Add(_modelNavigationWindow);
            //_controls.Add(_colorPaletteWindow);
            _controls.Add(_toolsWindow);
            _controls.Add(_infoLabel);
        }



        private WindowControl CreateNavigationWindow()
        {
            var height = _d3DEngine.ViewPort.Height;

            var listWindow = new WindowControl { Title = "Navigation" };
            listWindow.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, 0, 200, height - 40);
            listWindow.ControlsSpacing = new Vector2();
            listWindow.LeftTopMargin.X = 5;

            var modelsLabel = new LabelControl { Text = "Models" };
            modelsLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var modelsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var modelsEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var modelsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };

            modelsAddButton.Pressed += delegate { OnModelsAddPressed(); };
            modelsEditButton.Pressed += delegate { OnModelsEditPressed(); };
            modelsDeleteButton.Pressed += delegate { OnModelsDeletePressed(); };


            _modelsList = new ListControl { Name = "modelsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _modelsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _modelsList.SelectionMode = ListSelectionMode.Single;
            _modelsList.SelectionChanged += delegate { OnModelsSelected(); };
            _modelsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _modelsGroup.Children.Add(modelsLabel);
            _modelsGroup.Children.Add(modelsAddButton);
            _modelsGroup.Children.Add(modelsEditButton);
            _modelsGroup.Children.Add(modelsDeleteButton);
            _modelsGroup.Children.Add(_modelsList);




            var animationsLabel = new LabelControl { Text = "Animations", Bounds = new UniRectangle(0, 0, 60, 20) };
            var animationsStepsLabel = new LabelControl { Text = "Steps", Bounds = new UniRectangle(0, 0, 60, 20) };
            var animationsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationsEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };

            animationsAddButton.Pressed += delegate { OnAnimationsAddButtonPressed(); };
            animationsEditButton.Pressed += delegate { OnAnimationsEditButtonPressed(); };
            animationsDeleteButton.Pressed += delegate { OnAnimationsDeleteButtonPressed(); };

            var animationStepAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationStepEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationStepDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };

            animationStepAddButton.Pressed += delegate { OnAnimationStepAddButtonPressed(); };
            animationStepEditButton.Pressed += delegate { OnAnimationStepEditButtonPressed(); };
            animationStepDeleteButton.Pressed += delegate { OnAnimationStepDeleteButtonPressed(); };

            var animationPlayButton = new ButtonControl { Text = "Play", Bounds = new UniRectangle(0, 0, 85, 20) };
            var animationStopButton = new ButtonControl { Text = "Stop", Bounds = new UniRectangle(0, 0, 85, 20) };

            animationPlayButton.Pressed += delegate { OnAnimationPlayButtonPressed(); };
            animationStopButton.Pressed += delegate { OnAnimationStopButtonPressed(); };

            _animationsList = new ListControl { Name = "animationsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _animationsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _animationsList.SelectionMode = ListSelectionMode.Single;
            _animationsList.SelectionChanged += delegate { SelectedAnimationIndex = _animationsList.SelectedItems.Count > 0 ? _animationsList.SelectedItems[0] : -1; };
            _animationsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _animationStepsList = new ListControl { Name = "animationSteps", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _animationStepsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _animationStepsList.SelectionMode = ListSelectionMode.Single;
            _animationStepsList.SelectionChanged += delegate { SelectedAnimationStepIndex = _animationStepsList.SelectedItems.Count > 0 ? _animationStepsList.SelectedItems[0] : -1; };

            _animationsGroup.Children.Add(animationsLabel);
            _animationsGroup.Children.Add(animationsAddButton);
            _animationsGroup.Children.Add(animationsEditButton);
            _animationsGroup.Children.Add(animationsDeleteButton);
            _animationsGroup.Children.Add(_animationsList);
            _animationsGroup.Children.Add(animationsStepsLabel);
            _animationsGroup.Children.Add(animationStepAddButton);
            _animationsGroup.Children.Add(animationStepEditButton);
            _animationsGroup.Children.Add(animationStepDeleteButton);
            _animationsGroup.Children.Add(_animationStepsList);
            _animationsGroup.Children.Add(animationPlayButton);
            _animationsGroup.Children.Add(animationStopButton);


            var statesLabel = new LabelControl { Text = "States" };
            statesLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var statesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var statesEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var statesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };

            statesAddButton.Pressed += delegate { OnStateAddButtonPressed(); };
            statesEditButton.Pressed += delegate { OnStateEditButtonPressed(); };
            statesDeleteButton.Pressed += delegate { OnStateDeleteButtonPressed(); };

            _statesList = new ListControl { Name = "statesList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _statesList.Bounds = new UniRectangle(0, 0, 180, 20);
            _statesList.SelectionMode = ListSelectionMode.Single;
            _statesList.SelectionChanged += delegate { SelectedStateIndex = _statesList.SelectedItems.Count > 0 ? _statesList.SelectedItems[0] : -1; UpdateCamera(); };
            _statesGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _statesGroup.Children.Add(statesLabel);
            _statesGroup.Children.Add(statesAddButton);
            _statesGroup.Children.Add(statesEditButton);
            _statesGroup.Children.Add(statesDeleteButton);
            _statesGroup.Children.Add(_statesList);



            var partsLabel = new LabelControl { Text = "Parts" };
            partsLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var partsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var partsEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var partsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };
            _partsList = new ListControl { Name = "partsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _partsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _partsList.SelectionMode = ListSelectionMode.Single;
            _partsList.SelectionChanged += delegate { SelectedPartIndex = _partsList.SelectedItems.Count > 0 ? _partsList.SelectedItems[0] : -1; };

            partsAddButton.Pressed += delegate { OnPartsAddPressed(); };
            partsEditButton.Pressed += delegate { OnPartsEditPressed(); };
            partsDeleteButton.Pressed += delegate { OnPartsDeletePressed(); };

            _partsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };
            _partsGroup.Children.Add(partsLabel);
            _partsGroup.Children.Add(partsAddButton);
            _partsGroup.Children.Add(partsEditButton);
            _partsGroup.Children.Add(partsDeleteButton);
            _partsGroup.Children.Add(_partsList);



            var framesLabel = new LabelControl { Text = "Frames", Bounds = new UniRectangle(0, 0, 25, 20) };
            var framesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var framesEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var framesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };
            var framesHideButton = new ButtonControl { Text = "Hide", Bounds = new UniRectangle(0, 0, 35, 20) };
            _framesList = new ListControl { Name = "framesList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _framesList.Bounds = new UniRectangle(0, 0, 180, 20);
            _framesList.SelectionMode = ListSelectionMode.Single;
            _framesList.SelectionChanged += delegate { SelectedFrameIndex = _framesList.SelectedItems.Count > 0 ? _framesList.SelectedItems[0] : -1; };

            framesAddButton.Pressed += delegate { OnFrameAddPressed(); };
            framesEditButton.Pressed += delegate { OnFrameEditPressed(); };
            framesDeleteButton.Pressed += delegate { OnFrameDeletePressed(); };
            framesHideButton.Pressed += delegate { OnFrameHidePressed(); };
            _framesGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 90), LayoutFlags =  ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _framesGroup.Children.Add(framesLabel);
            _framesGroup.Children.Add(framesAddButton);
            _framesGroup.Children.Add(framesEditButton);
            _framesGroup.Children.Add(framesDeleteButton);
            _framesGroup.Children.Add(framesHideButton);
            _framesGroup.Children.Add(_framesList);


            //listWindow.Children.Add(_animationsGroup);
            listWindow.Children.Add(_modelsGroup);
            //listWindow.Children.Add(_partsGroup);
            //listWindow.Children.Add(_framesGroup);
            

            listWindow.UpdateLayout();

            _statesGroup.UpdateLayout();
            _partsGroup.UpdateLayout();
            _framesGroup.UpdateLayout();

            return listWindow;
        }
        
        private void OnMainViewMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_modelsGroup);

            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            _toolsWindow.Children.Add(_mainToolsGroup);
            
            
            UpdateLayout();
        }

        private void OnLayoutMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_statesGroup);
            _modelNavigationWindow.Children.Add(_partsGroup);
            _modelNavigationWindow.Children.Add(_framesGroup);

            OnLayoutGroupSelected(_layoutTool);

            UpdateLayout();
        }

        private void OnFrameMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_framesGroup);

            if (_framesList.SelectedItems.Count == 0 && _framesList.Items.Count > 0)
                _framesList.SelectItem(0);

            if (_frameEditorTool == FrameEditorTools.None)
                _frameEditorTool = FrameEditorTools.Edit;

            OnFrameToolSelected(_frameEditorTool);
            
            UpdateLayout();
        }

        private void OnAnimationMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_animationsGroup);

            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);

            UpdateLayout();
        }


        private WindowControl CreateModelEditWindow()
        {
            var window = new WindowControl { Title = "Model", Bounds = new UniRectangle(0,0, 200, 60) };

            var label = new LabelControl { Text = "Name", Bounds = new UniRectangle(0, 0, 70, 20) };
            var input = new InputControl { Bounds = new UniRectangle(0, 0, 100, 20) };
            
            window.Children.Add(label);
            window.Children.Add(input);
            
            return window;
        }

        private void UpdateColorPalette(ColorMapping mapping, int selectedColorIndex)
        {
            foreach (var colorButtonControl in _colorPalette)
            {
                colorButtonControl.Pressed -= OnColorSelected;
            }

            _colorPalette.Clear();

            for (int i = 0; i < mapping.BlockColors.Length; i++)
            {
                if (mapping.BlockColors[i].Alpha == 0)
                    break;

                var colorControl = new ColorButtonControl { Bounds = new UniRectangle(0, 0, 20, 20), Name = "color" + i, Color = mapping.BlockColors[i], Sticked = i == selectedColorIndex };
                colorControl.Pressed += OnColorSelected;
                _colorPalette.Add(colorControl);

            }

            _colorPaletteGroup.Children.Clear();

            foreach (var colorButtonControl in _colorPalette)
            {
                _colorPaletteGroup.Children.Add(colorButtonControl);
            }

            var rowsCount = _colorPaletteGroup.Children.Count%8 > 0 ? _colorPaletteGroup.Children.Count/8 + 1 : _colorPaletteGroup.Children.Count/8;
            
            _colorPaletteGroup.UpdateLayout();
            _colorPaletteGroup.Bounds.Size.Y.Offset = 20 * rowsCount + 5;
            _colorsGroup.UpdateLayout();
        }
        
        private void OnColorSelected(object sender, EventArgs e)
        {
            var control = (ColorButtonControl)sender;
            int.TryParse(control.Name.Substring(5), out _selectedColorIndex);
        }

        private WindowControl CreateToolsWindow()
        {
            var toolsWindow = new WindowControl { Title = "Tools" };
            toolsWindow.Bounds = new UniRectangle(0, 0, 200, 600);

            _modesButtonsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 45), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            var mainModeButton = new StickyButtonControl { Text = "Main", Sticked = true };
            mainModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            mainModeButton.Pressed += delegate { Mode = EditorMode.MainView; OnMainViewMode(); };

            var layoutModeButton = new StickyButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            layoutModeButton.Pressed += delegate {
                if (VisualVoxelModel == null)
                {
                    _gui.MessageBox("Select or create a model before entering");
                    layoutModeButton.Release();
                    mainModeButton.Sticked = true;
                    return;
                }
                Mode = EditorMode.ModelLayout; OnLayoutMode(); 
            };

            var frameModeButton = new StickyButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            frameModeButton.Pressed += delegate {
                if (VisualVoxelModel == null)
                {
                    _gui.MessageBox("Select or create a model before entering");
                    frameModeButton.Release();
                    mainModeButton.Sticked = true;
                    return;
                }
                Mode = EditorMode.FrameEdit; OnFrameMode(); 
            };

            var animationModeButton = new StickyButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            animationModeButton.Pressed += delegate {
                if (VisualVoxelModel == null)
                {
                    _gui.MessageBox("Select or create a model before entering");
                    animationModeButton.Release();
                    mainModeButton.Sticked = true;
                    return;
                }
                Mode = EditorMode.MainView; OnAnimationMode(); 
            };

            _modesButtonsGroup.Children.Add(mainModeButton);
            _modesButtonsGroup.Children.Add(layoutModeButton);
            _modesButtonsGroup.Children.Add(frameModeButton);
            _modesButtonsGroup.Children.Add(animationModeButton);
            
            _modesButtonsGroup.UpdateLayout();

            #region Main tools

            _mainToolsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 120), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _mainToolsGroup.Children.Add(new LabelControl { Text = "Storage", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });
            
            _saveButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Save" };
            _saveButton.Pressed += delegate { OnSaveClicked(); };

            var exportButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Export" };
            exportButton.Pressed += delegate { OnExport(); };

            var renderButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Render png" };
            renderButton.Pressed += delegate { OnRenderPng(); };

#if DEBUG

            var importButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Import" };
            importButton.Pressed += delegate { OnImport(); };

            var exportAllButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Export all" };
            exportAllButton.Pressed += delegate { OnExportAll(); };
            var importAllButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Import all" };
            importAllButton.Pressed += delegate { OnImportAll(); };

            var publishAllButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Publish All" };
            publishAllButton.Pressed += delegate { OnPublishAll(); };
#endif
            var publishButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Publish" };
            publishButton.Pressed += delegate { OnPublish(); };

            var downloadButton = new ButtonControl { Bounds = new UniRectangle(0, 0, 140, 20), Text = "Load from server" };
            downloadButton.Pressed += delegate { OnLoadServerModels(); };
            

            _mainToolsGroup.Children.Add(_saveButton);
            _mainToolsGroup.Children.Add(exportButton);
#if DEBUG
            _mainToolsGroup.Children.Add(importButton);
            _mainToolsGroup.Children.Add(importAllButton);
            _mainToolsGroup.Children.Add(exportAllButton);
            _mainToolsGroup.Children.Add(publishAllButton);
#endif
            _mainToolsGroup.Children.Add(renderButton);
            _mainToolsGroup.Children.Add(publishButton);
            _mainToolsGroup.Children.Add(downloadButton);

            #endregion

            #region Frame tools
            _frameToolsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 80), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _frameToolsGroup.Children.Add(new LabelControl { Text = "Tools", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var toolEditButton = new StickyButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 70, 20), Sticked = true };
            toolEditButton.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.Edit); };

            var toolColorBrushButton = new StickyButtonControl { Text = "Color brush", Bounds = new UniRectangle(0, 0, 70, 20) };
            toolColorBrushButton.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.ColorBrush); };

            var toolColorFillButton = new StickyButtonControl { Text = "Color fill", Bounds = new UniRectangle(0, 0, 70, 20) };
            toolColorFillButton.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.ColorFillBrush); };

            var toolBlockFillBrush = new StickyButtonControl { Text = "Block fill", Bounds = new UniRectangle(0, 0, 70, 20) };
            toolBlockFillBrush.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.BlockFillBrush); };

            var presetTool = new StickyButtonControl { Text = "Preset", Bounds = new UniRectangle(0, 0, 70, 20) };
            presetTool.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.Preset); };

            var selectionTool = new StickyButtonControl { Text = "Selection", Bounds = new UniRectangle(0, 0, 70, 20) };
            selectionTool.Pressed += delegate { OnFrameToolSelected(FrameEditorTools.Selection); };

            _frameToolsGroup.Children.Add(toolEditButton);
            _frameToolsGroup.Children.Add(toolColorBrushButton);
            _frameToolsGroup.Children.Add(toolColorFillButton);
            _frameToolsGroup.Children.Add(toolBlockFillBrush);
            _frameToolsGroup.Children.Add(presetTool);
            _frameToolsGroup.Children.Add(selectionTool);
            
            #region Tool properties

            #region Preset Tool properties

            _tpPreset = new Control { Bounds = new UniRectangle(0, 0, 180, 60), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _tpPreset.Children.Add(new LabelControl { Text = "Presets:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var fillPresetButton = new ButtonControl { Text = "Fill", Bounds = new UniRectangle(0, 0, 50, 20) };
            fillPresetButton.Pressed += delegate { OnFillPresetPressed(); };
            
            var spherePresetButton = new ButtonControl { Text = "Sphere", Bounds = new UniRectangle(0, 0, 50, 20) };
            spherePresetButton.Pressed += delegate { OnSpherePresetPressed(); };

            var cylinderPresetButton = new ButtonControl { Text = "Cylinder", Bounds = new UniRectangle(0, 0, 50, 20) };
            cylinderPresetButton.Pressed += delegate { OnCylinderPresetPressed(); };
            
            var outlinePresetButton = new ButtonControl { Text = "Outline", Bounds = new UniRectangle(0, 0, 50, 20) };
            outlinePresetButton.Pressed += delegate { OnOutlinePresetPressed(); };

            var ellipsoidPresetButton = new ButtonControl { Text = "Ellipsoid", Bounds = new UniRectangle(0, 0, 50, 20) };
            ellipsoidPresetButton.Pressed += delegate { OnEllipsoidPresetPressed(); };

            _tpPreset.Children.Add(fillPresetButton);
            _tpPreset.Children.Add(spherePresetButton);
            _tpPreset.Children.Add(cylinderPresetButton);
            _tpPreset.Children.Add(outlinePresetButton);
            _tpPreset.Children.Add(ellipsoidPresetButton);
            _tpPreset.UpdateLayout();

            #endregion

            #region Slice tool properties

            _tpSliceBrush = new Control { Bounds = new UniRectangle(0, 0, 180, 40), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _tpSliceBrush.Children.Add(new LabelControl { Text = "Slice axis:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var xSlice = new StickyButtonControl { Text = "X", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            xSlice.Pressed += delegate { _sliceAxis ^= EditorAxis.X; if (!_sliceAxis.HasFlag(EditorAxis.X)) xSlice.Release(); };

            var ySlice = new StickyButtonControl { Text = "Y", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            ySlice.Pressed += delegate { _sliceAxis ^= EditorAxis.Y; if (!_sliceAxis.HasFlag(EditorAxis.Y)) ySlice.Release(); };

            var zSlice = new StickyButtonControl { Text = "Z", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            zSlice.Pressed += delegate { _sliceAxis ^= EditorAxis.Z; if (!_sliceAxis.HasFlag(EditorAxis.Z)) zSlice.Release(); };

            var wholeSlice = new StickyButtonControl { Text = "Go through", Bounds = new UniRectangle(0, 0, 60, 20), Separate = true };
            wholeSlice.Pressed += delegate { _wholeSlice = !_wholeSlice; if (!_wholeSlice) wholeSlice.Release(); };

            var diagonalTouch = new StickyButtonControl { Text = "Diag", Bounds = new UniRectangle(0, 0, 60, 20), Separate = true };
            diagonalTouch.Pressed += delegate { _diagonalTouch = !_diagonalTouch; if (!_diagonalTouch) diagonalTouch.Release(); };

            _tpSliceBrush.Children.Add(xSlice);
            _tpSliceBrush.Children.Add(ySlice);
            _tpSliceBrush.Children.Add(zSlice);
            _tpSliceBrush.Children.Add(wholeSlice);
            _tpSliceBrush.Children.Add(diagonalTouch);
            _tpSliceBrush.UpdateLayout();

            #endregion

            #region Edit properties

            _tpMirror = new Control { Bounds = new UniRectangle(0, 0, 180, 40), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _tpMirror.Children.Add(new LabelControl { Text = "Mirror:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var xMirror = new StickyButtonControl { Text = "X", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            xMirror.Pressed += delegate { _mirror ^= EditorAxis.X; if (!_mirror.HasFlag(EditorAxis.X)) xMirror.Release(); };

            var yMirror = new StickyButtonControl { Text = "Y", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            yMirror.Pressed += delegate { _mirror ^= EditorAxis.Y; if (!_mirror.HasFlag(EditorAxis.Y)) yMirror.Release(); };

            var zMirror = new StickyButtonControl { Text = "Z", Bounds = new UniRectangle(0, 0, 20, 20), Separate = true };
            zMirror.Pressed += delegate { _mirror ^= EditorAxis.Z; if (!_mirror.HasFlag(EditorAxis.Z)) zMirror.Release(); };

            _tpMirror.Children.Add(xMirror);
            _tpMirror.Children.Add(yMirror);
            _tpMirror.Children.Add(zMirror);

            _tpMirror.UpdateLayout();

            _tpEdit = new Control { Bounds = new UniRectangle(0, 0, 180, 120), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };
            
            var frameLabel = new LabelControl { Text = "Frame:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow };
            
            var copyFrame = new ButtonControl { Text = "Copy", Bounds = new UniRectangle(0, 0, 40, 20) };
            copyFrame.Pressed += delegate { OnFrameCopyPressed(); };
            var delFrame = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 40, 20) };
            delFrame.Pressed += delegate { OnFrameBlockDeletePressed(); };

            var replaceFrame = new ButtonControl { Text = "Paste (replace)", Bounds = new UniRectangle(0, 0, 90, 20) };
            replaceFrame.Pressed += delegate { OnFramePastePressed(); };
            var mergeFrame = new ButtonControl { Text = "Paste (merge)", Bounds = new UniRectangle(0, 0, 90, 20) };
            mergeFrame.Pressed += delegate { OnFrameMergePressed(); };
            var undoFrame = new ButtonControl { Text = "Undo", Bounds = new UniRectangle(0, 0, 40, 20) };
            undoFrame.Pressed += delegate { OnFrameUndoPressed(); };
            var flipX = new ButtonControl { Text = "Flip by X", Bounds = new UniRectangle(0, 0, 60, 20) };
            flipX.Pressed += delegate { OnFlip(EditorAxis.X); };
            var flipY = new ButtonControl { Text = "Flip by Y", Bounds = new UniRectangle(0, 0, 60, 20) };
            flipY.Pressed += delegate { OnFlip(EditorAxis.Z); };
            var flipZ = new ButtonControl { Text = "Flip by Z", Bounds = new UniRectangle(0, 0, 60, 20) };
            flipZ.Pressed += delegate { OnFlip(EditorAxis.Y); };

            var shiftLabel = new LabelControl { Text = "Shift:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow };

            var shiftXPlus = new ButtonControl { Text = "X+", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftXPlus.Pressed += delegate { OnFrameShift(EditorAxis.X, true); };
            var shiftXMinus = new ButtonControl { Text = "X-", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftXMinus.Pressed += delegate { OnFrameShift(EditorAxis.X, false); };

            var shiftYPlus = new ButtonControl { Text = "Y+", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftYPlus.Pressed += delegate { OnFrameShift(EditorAxis.Y, true); };
            var shiftYMinus = new ButtonControl { Text = "Y-", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftYMinus.Pressed += delegate { OnFrameShift(EditorAxis.Y, false); };

            var shiftZPlus = new ButtonControl { Text = "Z+", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftZPlus.Pressed += delegate { OnFrameShift(EditorAxis.Z, true); };
            var shiftZMinus = new ButtonControl { Text = "Z-", Bounds = new UniRectangle(0, 0, 25, 20) };
            shiftZMinus.Pressed += delegate { OnFrameShift(EditorAxis.Z, false); };

            _tpEdit.Children.Add(frameLabel);
            _tpEdit.Children.Add(copyFrame);
            _tpEdit.Children.Add(delFrame);
            _tpEdit.Children.Add(undoFrame);
            _tpEdit.Children.Add(replaceFrame);
            _tpEdit.Children.Add(mergeFrame);
            _tpEdit.Children.Add(flipX);
            _tpEdit.Children.Add(flipY);
            _tpEdit.Children.Add(flipZ);

            _tpEdit.Children.Add(shiftLabel);
            _tpEdit.Children.Add(shiftXPlus);
            _tpEdit.Children.Add(shiftXMinus);
            _tpEdit.Children.Add(shiftYPlus);
            _tpEdit.Children.Add(shiftYMinus);
            _tpEdit.Children.Add(shiftZPlus);
            _tpEdit.Children.Add(shiftZMinus);

            _tpEdit.UpdateLayout();

            #endregion

            #endregion

            _colorsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 210), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _colorsGroup.Children.Add(new LabelControl { Text = "Colors", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            _colorPaletteGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 200), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _colorsGroup.Children.Add(_colorPaletteGroup);

            var colorsButtons = new Control { LayoutFlags = ControlLayoutFlags.WholeRow, Bounds = new UniRectangle(0,0,180,25) };

            var colorAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            colorAddButton.Pressed += delegate { OnColorAddPressed(); };
            var colorRemButton = new ButtonControl { Text = "Rem", Bounds = new UniRectangle(0, 0, 50, 20) };
            colorRemButton.Pressed += delegate { OnColorRemPressed(); };
            var colorChangeButton = new ButtonControl { Text = "Change", Bounds = new UniRectangle(0, 0, 50, 20) };
            colorChangeButton.Pressed += delegate { OnColorChangePressed(); };

            colorsButtons.Children.Add(colorAddButton);
            colorsButtons.Children.Add(colorRemButton);
            colorsButtons.Children.Add(colorChangeButton);

            colorsButtons.UpdateLayout();

            _colorsGroup.Children.Add(colorsButtons);

            _colorsGroup.UpdateLayout();

            _frameToolsGroup.UpdateLayout();
            #endregion
            
            #region Layout tools
            _layoutToolsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 45), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _layoutToolsGroup.Children.Add(new LabelControl { Text = "Tools:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var layoutTools = new StickyButtonControl { Text = "Layout", Bounds = new UniRectangle(0, 0, 60, 20), Sticked = true };
            layoutTools.Pressed += delegate { OnLayoutGroupSelected(LayoutTool.Move); };

            var rotateTools = new StickyButtonControl { Text = "Rotate", Bounds = new UniRectangle(0, 0, 60, 20) };
            rotateTools.Pressed += delegate { OnLayoutGroupSelected(LayoutTool.Rotate); };

            var scaleTools = new StickyButtonControl { Text = "Scale", Bounds = new UniRectangle(0, 0, 60, 20) };
            scaleTools.Pressed += delegate { OnLayoutGroupSelected(LayoutTool.Scale); };

            #region Layout tools
            _vpLayout = new Control { Bounds = new UniRectangle(0, 0, 180, 100), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            var copyLayoutButton = new ButtonControl { Text = "Copy layout", Bounds = new UniRectangle(0, 0, 100, 20) };
            copyLayoutButton.Pressed += delegate { OnLayoutCopy(); };
            var pasteLayoutButton = new ButtonControl { Text = "Paste layout", Bounds = new UniRectangle(0, 0, 100, 20) };
            pasteLayoutButton.Pressed += delegate { OnLayoutPaste(); };
            var toolLabel = new LabelControl { Text = "Tool position:", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow };
            _toolSetPositionButton = new StickyButtonControl { Text = "Set tool mount point", Bounds = new UniRectangle(0, 0, 120, 20) };
            _toolSetPositionButton.Pressed += delegate { OnSetToolPosition(); };
            var rotateToolButton = new ButtonControl { Text = "Rotate tool", Bounds = new UniRectangle(0, 0, 100, 20) };
            rotateToolButton.Pressed += delegate { OnRotateTool(); };

            _vpLayout.Children.Add(copyLayoutButton);
            _vpLayout.Children.Add(pasteLayoutButton);
            _vpLayout.Children.Add(toolLabel);
            _vpLayout.Children.Add(_toolSetPositionButton);
            _vpLayout.Children.Add(rotateToolButton);
            _vpLayout.UpdateLayout();

            #endregion

            #region Rotate tools
            _vpRotate = new Control { Bounds = new UniRectangle(0, 0, 180, 180), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };


            var rotateDisplay = new LabelControl { Bounds = new UniRectangle(0, 0, 50, 20), Text = "Display:", LayoutFlags = ControlLayoutFlags.WholeRow };

            var displayPosition = new StickyButtonControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = "Position" , Sticked = true };
            displayPosition.Pressed += delegate { _displayLayoutRotationPosition = true; };
            var displayAxis = new StickyButtonControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = "Rotation" };
            displayAxis.Pressed += delegate { _displayLayoutRotationPosition = false; };

            var angleLabel = new LabelControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Rotate Angle:" };
            _rotateAngleInput = new InputControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = "10", LayoutFlags = ControlLayoutFlags.WholeRow };
            
            var rotateX       = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Rotate X (RED)" };
            rotateX.Pressed += delegate { OnPartRotation(EditorAxis.X); };
            var rotateY       = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Rotate Y (GREEN)" };
            rotateY.Pressed += delegate { OnPartRotation(EditorAxis.Y); };
            var rotateZ       = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Rotate Z (BLUE)" };
            rotateZ.Pressed += delegate { OnPartRotation(EditorAxis.Z); };
            var resetRotation = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Reset rotation" };
            resetRotation.Pressed += delegate { OnPartRotation(EditorAxis.None); };
            var moveRotationPointToCenter = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Move to center" };
            moveRotationPointToCenter.Pressed += delegate { OnMoveRotationToCenter(); };

            _vpRotate.Children.Add(rotateDisplay);
            _vpRotate.Children.Add(displayPosition);
            _vpRotate.Children.Add(displayAxis);

            _vpRotate.Children.Add(angleLabel);
            _vpRotate.Children.Add(_rotateAngleInput);

            _vpRotate.Children.Add(rotateX);
            _vpRotate.Children.Add(rotateY);
            _vpRotate.Children.Add(rotateZ);
            _vpRotate.Children.Add(resetRotation);
            _vpRotate.Children.Add(moveRotationPointToCenter);
            
            _vpRotate.UpdateLayout();

            #endregion
            
            #region Scale tools

            _vpScale = new Control { Bounds = new UniRectangle(0, 0, 180, 180), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            var scaleAngleLabel = new LabelControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Scale factor:" };
            _scaleAngleInput = new InputControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = "1.01", LayoutFlags = ControlLayoutFlags.WholeRow };

            var scaleAll = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Scale" };
            scaleAll.Pressed += delegate { OnPartScale(EditorAxis.X | EditorAxis.Y | EditorAxis.Z); };

            var scaleX = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Scale X" };
            scaleX.Pressed += delegate { OnPartScale(EditorAxis.X); };
            var scaleY = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Scale Y" };
            scaleY.Pressed += delegate { OnPartScale(EditorAxis.Y); };
            var scaleZ = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Scale Z" };
            scaleZ.Pressed += delegate { OnPartScale(EditorAxis.Z); };

            var resetScale = new ButtonControl { Bounds = new UniRectangle(0, 0, 100, 20), Text = "Reset scale" };
            resetScale.Pressed += delegate { OnPartScale(EditorAxis.None); };

            _vpScale.Children.Add(scaleAngleLabel);
            _vpScale.Children.Add(_scaleAngleInput);

            _vpScale.Children.Add(scaleAll);
            _vpScale.Children.Add(scaleX);
            _vpScale.Children.Add(scaleY);
            _vpScale.Children.Add(scaleZ);
            _vpScale.Children.Add(resetScale);

            _vpScale.UpdateLayout();

            #endregion

            _layoutToolsGroup.Children.Add(layoutTools);
            _layoutToolsGroup.Children.Add(rotateTools);
            _layoutToolsGroup.Children.Add(scaleTools);
            #endregion

            toolsWindow.Children.Add(_modesButtonsGroup);

            toolsWindow.UpdateLayout();

            return toolsWindow;
        }

        private enum LayoutTool
        {
            Move,
            Rotate,
            Scale
        }

        private void OnLayoutGroupSelected(LayoutTool group)
        {
            _layoutTool = group;
            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            _toolsWindow.Children.Add(_layoutToolsGroup);
            _cursorMode = false;
            switch (group)
            {
                case LayoutTool.Move:
                    _toolsWindow.Children.Add(_vpLayout);
                    break;
                case LayoutTool.Rotate:
                    _cursorMode = true;
                    _toolsWindow.Children.Add(_vpRotate);
                    break;
                case LayoutTool.Scale:
                    _toolsWindow.Children.Add(_vpScale);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("group");
            }
            
            _toolsWindow.UpdateLayout();
        }

        private void OnFrameToolSelected(FrameEditorTools tool)
        {
            _frameEditorTool = tool;
            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            _toolsWindow.Children.Add(_frameToolsGroup);
            
            switch (tool)
            {
                case FrameEditorTools.Edit:
                    _toolsWindow.Children.Add(_tpMirror);
                    _toolsWindow.Children.Add(_tpEdit);
                    break;
                case FrameEditorTools.ColorBrush:
                    _toolsWindow.Children.Add(_tpMirror);
                    break;
                case FrameEditorTools.ColorFillBrush:
                    _toolsWindow.Children.Add(_tpSliceBrush);
                    break;
                case FrameEditorTools.BlockFillBrush:
                    _toolsWindow.Children.Add(_tpSliceBrush);
                    break;
                case FrameEditorTools.Preset:
                    _toolsWindow.Children.Add(_tpPreset);
                    break;
                case FrameEditorTools.Selection:
                    _toolsWindow.Children.Add(_tpEdit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tool");
            }

            _toolsWindow.Children.Add(_colorsGroup);
            _toolsWindow.UpdateLayout();
        }
        
        public void UpdateLayout()
        {
            _backButton.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, _d3DEngine.ViewPort.Height - 30, 120, 24);
            _modelNavigationWindow.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, 0, 200, _d3DEngine.ViewPort.Height - 40);
            _modelNavigationWindow.UpdateLayout();
            foreach (var group in _modelNavigationWindow.Children)
            {
                group.UpdateLayout();
            }

            _toolsWindow.UpdateLayout();
            foreach (var group in _toolsWindow.Children)
            {
                group.UpdateLayout();
            }
        }
    }

    public enum FrameEditorTools
    {
        None,
        Edit,
        ColorBrush,
        ColorFillBrush,
        Preset,
        Selection,
        BlockFillBrush
    }
}

