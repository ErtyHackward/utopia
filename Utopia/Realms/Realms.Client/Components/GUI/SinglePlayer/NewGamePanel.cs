using S33M3CoreComponents.GUI.Nuclex.Controls;
using Utopia.Shared.World;
using S33M3CoreComponents.GUI;

namespace Realms.Client.Components.GUI.SinglePlayer
{
    public partial class NewGamePanel : Control
    {
        private SandboxCommonResources _commonResources;
        private WorldParameters _currentWorldParameter;
        private RuntimeVariables _vars;
        private GuiManager _guiManager;
        
        public NewGamePanel(SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RuntimeVariables vars, GuiManager guiManager)
        {
            _vars = vars;
            _guiManager = guiManager;
            _commonResources = commonResources;
            _currentWorldParameter = currentWorldParameter;
            InitializeComponent();

            this.IsVisible = true;
            this.IsRendable = false;
        }

    }
}
