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
        }

        public void Release()
        {
            Sticked = false;
        }
    }
}
