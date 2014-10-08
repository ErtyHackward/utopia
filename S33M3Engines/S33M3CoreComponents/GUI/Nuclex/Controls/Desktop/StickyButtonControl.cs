using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;

namespace Utopia.GUI.NuclexUIPort.Controls.Desktop
{
    public class StickyButtonControl : ButtonControl
    {
        public bool Sticked { get; set; }

        /// <summary>
        /// Gets or sets value indicating if the button should not automatically unstick when other stick button in the group is pressed
        /// </summary>
        public bool Separate { get; set; }

        protected override void OnPressed()
        {
            Sticked = true;
            
            // unstick all other buttons at our parent

            if (Parent != null && !Separate)
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

            base.OnPressed();
        }

        public void Release()
        {
            Sticked = false;
        }
    }
}
