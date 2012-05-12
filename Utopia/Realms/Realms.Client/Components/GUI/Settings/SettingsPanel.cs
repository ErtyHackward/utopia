using S33M3CoreComponents.GUI.Nuclex.Controls;

namespace Realms.Client.Components.GUI.Settings
{
    public abstract partial class SettingsPanel : Control
    {
        #region Private Variables
        private SettingsComponent _parent;
        private string _panelName;
        #endregion

        #region Public Variables
        #endregion

        public SettingsPanel(SettingsComponent parent, object SettingParameters, string panelName)
        {
            _panelName = panelName;
            _parent = parent;
            this.IsVisible = true;
            this.IsRendable = false;
            //initialize the graphical component of the pannel
            InitializeComponent(SettingParameters);
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
