using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using SharpDX;

namespace Utopia.Components
{
    // this part

    public partial class ModelEditorComponent
    {
        private WindowControl CreateNavigationWindow()
        {
            var height = _d3DEngine.ViewPort.Height;

            var listWindow = new WindowControl { Title = "Navigation" };
            listWindow.Bounds = new UniRectangle(_d3DEngine.ViewPort.Width - 200, 0, 200, height - 40);


            var statesLabel = new LabelControl { Text = "States" };
            statesLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var statesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            var statesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            _statesList = new ListControl { Name = "statesList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _statesList.Bounds = new UniRectangle(0, 0, 180, 20);
            _statesList.SelectionMode = ListSelectionMode.Single;
            _statesList.SelectionChanged += delegate { SelectedStateIndex = _statesList.SelectedItems.Count > 0 ? _statesList.SelectedItems[0] : -1; };

            var partsLabel = new LabelControl { Text = "Parts" };
            partsLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var partsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            var partsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            _partsList = new ListControl { Name = "partsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            _partsList.Bounds = new UniRectangle(0, 0, 180, 20);
            _partsList.SelectionMode = ListSelectionMode.Single;
            _partsList.SelectionChanged += delegate { SelectedPartIndex = _partsList.SelectedItems.Count > 0 ? _partsList.SelectedItems[0] : -1; };

            var framesLabel = new LabelControl { Text = "Frames" };
            framesLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var framesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            var framesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            _framesList = new ListControl { Name = "framesList", LayoutFlags = ControlLayoutFlags.WholeRow };
            _framesList.Bounds = new UniRectangle(0, 0, 180, 50);
            _framesList.SelectionMode = ListSelectionMode.Single;
            _framesList.SelectionChanged += delegate { SelectedFrameIndex = _framesList.SelectedItems.Count > 0 ? _framesList.SelectedItems[0] : -1; };


            listWindow.Children.Add(statesLabel);
            listWindow.Children.Add(statesAddButton);
            listWindow.Children.Add(statesDeleteButton);
            listWindow.Children.Add(_statesList);

            listWindow.Children.Add(partsLabel);
            listWindow.Children.Add(partsAddButton);
            listWindow.Children.Add(partsDeleteButton);
            listWindow.Children.Add(_partsList);

            listWindow.Children.Add(framesLabel);
            listWindow.Children.Add(framesAddButton);
            listWindow.Children.Add(framesDeleteButton);
            listWindow.Children.Add(_framesList);

            listWindow.UpdateLayout();

            return listWindow;
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
            viewModeButton.Pressed += delegate { Mode = EditorMode.ModelView; };

            var layoutModeButton = new ButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            layoutModeButton.Pressed += delegate { Mode = EditorMode.ModelLayout; };

            var frameModeButton = new ButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            frameModeButton.Pressed += delegate { Mode = EditorMode.FrameEdit; };

            var animationModeButton = new ButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            animationModeButton.Pressed += delegate { Mode = EditorMode.ModelLayout; };

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
    }
}
