using Nuclex.UserInterface.Controls.Desktop;

namespace Utopia.GUI.NuclexUIPort.Controls.Desktop
{
    public class StickyButtonControl : ButtonControl
    {
        public bool Sticked { get; set; }

        protected override void OnPressed()
        {
            base.OnPressed();
            Sticked = true;

            // unstick all other buttons at our parent

            if (Parent != null)
            {
                foreach (var control in Parent.Children)
                {
                    if (control != this && control is StickyButtonControl)
                    {
                        var stick = (StickyButtonControl)control;
                        stick.Release();
                    }
                }
            }

        }

        public void Release()
        {
            Sticked = false;
        }
    }
}
