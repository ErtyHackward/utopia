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
        private struct DialogAnimationEditStruct
        {
            public string Name;
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
            public int SizeX;
            public int SizeY;
            public int SizeZ;
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
        private LabelControl _infoLabel;

        // navigation groups
        private Control _modelsGroup;
        private Control _animationsGroup;
        private Control _statesGroup;
        private Control _partsGroup;
        private Control _framesGroup;

        // tools group
        private Control _frameToolsGroup;
        private Control _layoutToolsGroup;
        private Control _modesButtonsGroup;
        private Control _colorPaletteGroup;

        private List<ColorButtonControl> _colorPalette = new List<ColorButtonControl>();
        private List<StickyButtonControl> _toolsButtons = new List<StickyButtonControl>();

        private ListControl _modelsList;
        private ListControl _animationsList;
        private ListControl _animationStepsList;
        private ListControl _statesList;
        private ListControl _partsList;
        private ListControl _framesList;
        

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
            _statesList.SelectionChanged += delegate { SelectedStateIndex = _statesList.SelectedItems.Count > 0 ? _statesList.SelectedItems[0] : -1; };
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



            var framesLabel = new LabelControl { Text = "Frames" };
            framesLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var framesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var framesEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var framesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };
            _framesList = new ListControl { Name = "framesList", LayoutFlags = ControlLayoutFlags.WholeRow };
            _framesList.Bounds = new UniRectangle(0, 0, 180, 50);
            _framesList.SelectionMode = ListSelectionMode.Single;
            _framesList.SelectionChanged += delegate { SelectedFrameIndex = _framesList.SelectedItems.Count > 0 ? _framesList.SelectedItems[0] : -1; };

            framesAddButton.Pressed += delegate { OnFrameAddPressed(); };
            framesEditButton.Pressed += delegate { OnFrameEditPressed(); };
            framesDeleteButton.Pressed += delegate { OnFrameDeletePressed(); };
            _framesGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 90), LayoutFlags = ControlLayoutFlags.WholeRow };

            _framesGroup.Children.Add(framesLabel);
            _framesGroup.Children.Add(framesAddButton);
            _framesGroup.Children.Add(framesEditButton);
            _framesGroup.Children.Add(framesDeleteButton);
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
        
        private void OnViewMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_modelsGroup);

            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            
            
            UpdateLayout();
        }

        private void OnLayoutMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_statesGroup);
            _modelNavigationWindow.Children.Add(_partsGroup);
            _modelNavigationWindow.Children.Add(_framesGroup);

            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            _toolsWindow.Children.Add(_layoutToolsGroup);

            UpdateLayout();
        }

        private void OnFrameMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_partsGroup);
            _modelNavigationWindow.Children.Add(_framesGroup);

            _toolsWindow.Children.Clear();
            _toolsWindow.Children.Add(_modesButtonsGroup);
            _toolsWindow.Children.Add(_frameToolsGroup);

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
            _frameToolsGroup.UpdateLayout();
        }
        
        private void OnColorSelected(object sender, EventArgs e)
        {
            var control = (ColorButtonControl)sender;
            int.TryParse(control.Name.Substring(5), out _selectedColorIndex);
        }

        private WindowControl CreateToolsWindow()
        {
            var toolsWindow = new WindowControl { Title = "Tools" };
            toolsWindow.Bounds = new UniRectangle(0, 0, 200, 400);

            _modesButtonsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 45), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            var viewModeButton = new StickyButtonControl { Text = "View", Sticked = true };
            viewModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            viewModeButton.Pressed += delegate { Mode = EditorMode.ModelView; OnViewMode(); };

            var layoutModeButton = new StickyButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            layoutModeButton.Pressed += delegate { Mode = EditorMode.ModelLayout; OnLayoutMode(); };

            var frameModeButton = new StickyButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            frameModeButton.Pressed += delegate { Mode = EditorMode.FrameEdit; OnFrameMode(); };

            var animationModeButton = new StickyButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            animationModeButton.Pressed += delegate { Mode = EditorMode.ModelView; OnAnimationMode(); };

            _modesButtonsGroup.Children.Add(viewModeButton);
            _modesButtonsGroup.Children.Add(layoutModeButton);
            _modesButtonsGroup.Children.Add(frameModeButton);
            _modesButtonsGroup.Children.Add(animationModeButton);
            
            _modesButtonsGroup.UpdateLayout();

            #region Frame tools
            _frameToolsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 310), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _frameToolsGroup.Children.Add(new LabelControl { Text = "Tools", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var toolEditButton = new StickyButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 70, 20), Sticked = true };
            toolEditButton.Pressed += delegate { OnToolSelected(0); };
            var toolColorBrushButton = new StickyButtonControl { Text = "Color brush", Bounds = new UniRectangle(0, 0, 70, 20) };
            toolColorBrushButton.Pressed += delegate { OnToolSelected(1); };
            var toolColorFillButton = new StickyButtonControl { Text = "Color fill", Bounds = new UniRectangle(0, 0, 70, 20) };
            toolColorFillButton.Pressed += delegate { OnToolSelected(2); };

            _frameToolsGroup.Children.Add(toolEditButton);
            _frameToolsGroup.Children.Add(toolColorBrushButton);
            _frameToolsGroup.Children.Add(toolColorFillButton);

            _toolsButtons.Add(toolEditButton);
            _toolsButtons.Add(toolColorBrushButton);
            _toolsButtons.Add(toolColorFillButton);
            

            _frameToolsGroup.Children.Add(new LabelControl { Text = "Presets", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            var cubePresetButton = new ButtonControl { Text = "Cube", Bounds = new UniRectangle(0, 0, 50, 20) };
            cubePresetButton.Pressed += delegate { OnCubePresetPressed(); };
            _frameToolsGroup.Children.Add(cubePresetButton);

            var spherePresetButton = new ButtonControl { Text = "Sphere", Bounds = new UniRectangle(0, 0, 50, 20) };
            spherePresetButton.Pressed += delegate { OnSpherePresetPressed(); };
            _frameToolsGroup.Children.Add(spherePresetButton);

            var outlinePresetButton = new ButtonControl { Text = "Outline", Bounds = new UniRectangle(0, 0, 50, 20) };
            outlinePresetButton.Pressed += delegate { OnOutlinePresetPressed(); };
            _frameToolsGroup.Children.Add(outlinePresetButton);


            _frameToolsGroup.Children.Add(new LabelControl { Text = "Colors", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            _colorPaletteGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 200), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _frameToolsGroup.Children.Add(_colorPaletteGroup);

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

            _frameToolsGroup.Children.Add(colorsButtons);

            _frameToolsGroup.UpdateLayout();
            #endregion


            #region Layout tools
            _layoutToolsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 100), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            _layoutToolsGroup.Children.Add(new LabelControl { Text = "View", Bounds = new UniRectangle(0, 0, 50, 20), LayoutFlags = ControlLayoutFlags.WholeRow });

            //var groundCheck = new OptionControl { Bounds = new UniRectangle(0,0, 70, 20), Text ="Ground" };
            //groundCheck.Changed += delegate { _drawGround = !_drawGround; };

            //var layoutCopy = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Copy" };

            

            //var layoutPaste = new ButtonControl { Bounds = new UniRectangle(0, 0, 70, 20), Text = "Paste" };


            //_layoutToolsGroup.Children.Add(layoutCopy);
            //_layoutToolsGroup.Children.Add(layoutPaste);
            //_layoutToolsGroup.Children.Add(groundCheck);
            #endregion

            toolsWindow.Children.Add(_modesButtonsGroup);

            toolsWindow.UpdateLayout();

            return toolsWindow;
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
}
