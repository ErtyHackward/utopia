using System;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using SharpDX;
using SharpDX.Direct3D11;

namespace Utopia.GUI.NuclexUIPort.Controls.Desktop
{
    internal static class DialogHelper
    {
        /// <summary>
        /// This control prevents interaction with other active GUI while dialog is shown
        /// </summary>
        internal static Control DialogBg = new Control { Bounds = new UniRectangle(0, 0, 10000, 10000) };
    }

    /// <summary>
    /// Allows to show custom dialog
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DialogControl<T> : WindowControl where T : new()
    {
        private readonly ButtonControl _buttonOk;
        private readonly ButtonControl _buttonCancel;
        private Action<T> _okAction;

        public DialogControl()
        {
            // generate inputs controls
            // supported types: string, int, float

            var type = typeof(T);
            var fieldInfos = type.GetFields();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(int))
                {
                    var label = new LabelControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = fieldInfo.Name };
                    Children.Add(label);
                    var edit = new InputControl { Name = fieldInfo.Name, Bounds = new UniRectangle(0, 0, 60, 20), IsNumeric = true };
                    Children.Add(edit);
                    Bounds.Size.Y += 25;
                }

                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(float))
                {
                    var label = new LabelControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = fieldInfo.Name };
                    Children.Add(label);
                    var edit = new InputControl { Name = fieldInfo.Name, Bounds = new UniRectangle(0, 0, 60, 20), IsNumeric = true };
                    Children.Add(edit);
                    Bounds.Size.Y += 25;
                }

                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(string))
                {
                    var label = new LabelControl { Bounds = new UniRectangle(0, 0, 60, 20), Text = fieldInfo.Name };
                    Children.Add(label);
                    var edit = new InputControl { Name = fieldInfo.Name, Bounds = new UniRectangle(0,0, 60, 20) };
                    Children.Add(edit);
                    Bounds.Size.Y += 25;
                }
            }

            _buttonOk = new ButtonControl { Text = "Ok", Bounds = new UniRectangle(0, 0, 50, 20) };
            _buttonOk.Pressed += delegate { ApplyDialog(); };
            _buttonCancel = new ButtonControl { Text = "Cancel", Bounds = new UniRectangle(0, 0, 50, 20) };
            _buttonCancel.Pressed += delegate { HideDialog(); };

            var buttonsGroup = new Control { Bounds = new UniRectangle(0, 0, 105, 20), LayoutFlags = ControlLayoutFlags.WholeRowCenter, LeftTopMargin = new Vector2() };

            buttonsGroup.Children.Add(_buttonOk);
            buttonsGroup.Children.Add(_buttonCancel);
            buttonsGroup.UpdateLayout();

            Children.Add(buttonsGroup);

            Bounds.Size.Y += 60;
            Bounds.Size.X = 200;

            UpdateLayout();
        }

        private void ApplyDialog()
        {
            var t = new T();
            var type = typeof(T);

            foreach (var control in Children)
            {
                if (!string.IsNullOrEmpty(control.Name))
                {
                    // map a value to return object
                    var fi = type.GetField(control.Name);

                    if (fi.FieldType == typeof(int))
                    {
                        var edit = (InputControl)control;
                        int value;
                        int.TryParse(edit.Text, out value);
                        fi.SetValueDirect(__makeref(t), value);
                    }

                    if (fi.FieldType == typeof(float))
                    {
                        var edit = (InputControl)control;
                        float value;
                        float.TryParse(edit.Text, out value);
                        fi.SetValueDirect(__makeref(t), value);
                    }

                    if (fi.FieldType == typeof(string))
                    {
                        var edit = (InputControl)control;
                        fi.SetValueDirect(__makeref(t), edit.Text);
                    }
                }
            }


            if (_okAction != null) 
                _okAction(t);

            HideDialog();
        }

        private void HideDialog()
        {
            DialogHelper.DialogBg.Clicked -= DialogBgClicked;
            Screen.Desktop.Children.Remove(DialogHelper.DialogBg); 
            Hide();
        }

        /// <summary>
        /// Shows a dialog with some initial values
        /// </summary>
        /// <param name="port"> </param>
        /// <param name="t"></param>
        /// <param name="screen"> </param>
        /// <param name="title"> </param>
        /// <param name="okAction"> </param>
        public void ShowDialog(Screen screen, Viewport port, T t, string title, Action<T> okAction)
        {
            Title = title;

            _okAction = okAction;
            var type = typeof(T);
            var fieldInfos = type.GetFields();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(int))
                {
                    var edit = Children.Get<InputControl>(fieldInfo.Name);
                    edit.Text = fieldInfo.GetValue(t).ToString();
                }

                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(float))
                {
                    var edit = Children.Get<InputControl>(fieldInfo.Name);
                    edit.Text = fieldInfo.GetValue(t).ToString();
                }

                if (fieldInfo.IsPublic && fieldInfo.FieldType == typeof(string))
                {
                    var edit = Children.Get<InputControl>(fieldInfo.Name);
                    var obj =  fieldInfo.GetValue(t);
                    edit.Text = obj == null ? null : obj.ToString();
                }
            }

            screen.Desktop.Children.Add(DialogHelper.DialogBg);
            DialogHelper.DialogBg.Clicked += DialogBgClicked;
            DialogHelper.DialogBg.BringToFront();

            Show(screen, port);

            foreach (var control in Children)
            {
                if (control is InputControl)
                {
                    screen.FocusedControl = control;
                    break;
                }
            }
        }

        void DialogBgClicked(object sender, EventArgs e)
        {
            BringToFront();
        }
    }
}
