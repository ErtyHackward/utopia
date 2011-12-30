using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using SharpDX;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;

namespace Utopia.Components
{
    public partial class ModelEditorComponent
    {
        private struct DialogModelEditStruct
        {
            public string Name;
        }

        private ButtonControl _backButton;
        private WindowControl _toolsWindow;
        private WindowControl _modelNavigationWindow;
        private DialogControl<DialogModelEditStruct> _modelEditDialog;
        private LabelControl _infoLabel;

        // navigation groups
        private Control _modelsGroup;
        private Control _animationsGroup;
        private Control _statesGroup;
        private Control _partsGroup;
        private Control _framesGroup;

        private ListControl _modelsList;
        private ListControl _animationsList;
        private ListControl _statesList;
        private ListControl _partsList;
        private ListControl _framesList;

        private void InitializeGui()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };

            _toolsWindow = CreateToolsWindow();
            _modelNavigationWindow = CreateNavigationWindow();
            _modelEditDialog = new DialogControl<DialogModelEditStruct>();
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
            _modelsList.SelectionChanged += delegate { SelectedAnimationIndex = _animationsList.SelectedItems.Count > 0 ? _animationsList.SelectedItems[0] : -1; };
            _modelsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _modelsGroup.Children.Add(modelsLabel);
            _modelsGroup.Children.Add(modelsAddButton);
            _modelsGroup.Children.Add(modelsEditButton);
            _modelsGroup.Children.Add(modelsDeleteButton);
            _modelsGroup.Children.Add(_modelsList);




            var animationsLabel = new LabelControl { Text = "Animations" };
            animationsLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var animationsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationsEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var animationsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };
            _animationsList = new ListControl { Name = "animationsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _animationsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _animationsList.SelectionMode = ListSelectionMode.Single;
            _animationsList.SelectionChanged += delegate { SelectedAnimationIndex = _animationsList.SelectedItems.Count > 0 ? _animationsList.SelectedItems[0] : -1; };
            _animationsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 0), LayoutFlags = ControlLayoutFlags.FreeHeight | ControlLayoutFlags.WholeRow };

            _animationsGroup.Children.Add(animationsLabel);
            _animationsGroup.Children.Add(animationsAddButton);
            _animationsGroup.Children.Add(animationsEditButton);
            _animationsGroup.Children.Add(animationsDeleteButton);
            _animationsGroup.Children.Add(_animationsList);


            var statesLabel = new LabelControl { Text = "States" };
            statesLabel.Bounds = new UniRectangle(0, 0, 60, 20);
            var statesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 35, 20) };
            var statesEditButton = new ButtonControl { Text = "Edit", Bounds = new UniRectangle(0, 0, 35, 20) };
            var statesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 35, 20) };
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
            UpdateLayout();
        }

        private void OnLayoutMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_statesGroup);
            _modelNavigationWindow.Children.Add(_partsGroup);
            _modelNavigationWindow.Children.Add(_framesGroup);
            UpdateLayout();
        }

        private void OnFrameMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_partsGroup);
            _modelNavigationWindow.Children.Add(_framesGroup);
            UpdateLayout();
        }

        private void OnAnimationMode()
        {
            _modelNavigationWindow.Children.Clear();
            _modelNavigationWindow.Children.Add(_animationsGroup);
            _modelNavigationWindow.Children.Add(_statesGroup);
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

        private WindowControl CreateToolsWindow()
        {
            var toolsWindow = new WindowControl { Title = "Tools" };
            toolsWindow.Bounds = new UniRectangle(0, 0, 200, 110);

            var modesLabel = new LabelControl { Text = "Modes" };
            modesLabel.Bounds = new UniRectangle(10, 25, 100, 20);

            var modesButtonsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 45), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };

            var viewModeButton = new ButtonControl { Text = "View" };
            viewModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            viewModeButton.Pressed += delegate { Mode = EditorMode.ModelView; OnViewMode(); };

            var layoutModeButton = new ButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            layoutModeButton.Pressed += delegate { Mode = EditorMode.ModelLayout; OnLayoutMode(); };

            var frameModeButton = new ButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            frameModeButton.Pressed += delegate { Mode = EditorMode.FrameEdit; OnFrameMode(); };

            var animationModeButton = new ButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            animationModeButton.Pressed += delegate { Mode = EditorMode.ModelLayout; OnAnimationMode(); };

            modesButtonsGroup.Children.Add(viewModeButton);
            modesButtonsGroup.Children.Add(layoutModeButton);
            modesButtonsGroup.Children.Add(frameModeButton);
            modesButtonsGroup.Children.Add(animationModeButton);
            


            modesButtonsGroup.UpdateLayout();

            toolsWindow.Children.Add(modesLabel);
            toolsWindow.Children.Add(modesButtonsGroup);

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
        }
    }
}
