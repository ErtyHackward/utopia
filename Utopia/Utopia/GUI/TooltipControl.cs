using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;

namespace Utopia.GUI
{
    public class TooltipControl : WindowControl
    {
        private LabelControl _label;

        public TooltipControl()
        {
            IsClickTransparent = true;
            Bounds = new UniRectangle(0,0, 200, 100);

            _label = new LabelControl("description here") { Autosizing = true };
            _label.Bounds = new UniRectangle(new UniScalar(0, 10), new UniScalar(0,30), new UniScalar(1,0), new UniScalar(1,0));
            Children.Add(_label);
        }

        public void SetText(string title, string description)
        {
            Title = title;
            _label.Text = description;
        }
    }
}
