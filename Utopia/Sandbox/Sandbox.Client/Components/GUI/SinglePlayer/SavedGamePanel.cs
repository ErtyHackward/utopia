using System;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine.Threading;
using Utopia.Shared.World;
using Utopia.Shared.Settings;
using System.IO;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class SavedGamePanel : Control
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private SandboxCommonResources _commonResources;
        private WorldParameters _currentWorldParameter;
        private RuntimeVariables _vars;
        #endregion

        public bool NeedShowResults { get; set; }

        public SavedGamePanel(SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RuntimeVariables vars)
        {
            _vars = vars;
            _currentWorldParameter = currentWorldParameter;
            _commonResources = commonResources;
            InitializeComponent();
            this.IsVisible = true;
            this.IsRendable = false;
        }

        #region Public methods
        #endregion

        #region Private methods
        private void DeleteWorld(LocalWorlds.LocalWorldsParam info)
        {
            try
            {
                //Try to delete the Server directory
                Directory.Delete(info.WorldServerRootPath.FullName, true);

                //Try to delete the Client directory
                Directory.Delete(info.WorldClientRootPath.FullName, true);

                //Recreate the list of all existing Worlds, as one has been deleted
                LocalWorlds.LocalWorldsParams = LocalWorlds.GetAllSinglePlayerWorldsParams(_vars.ApplicationDataPath);

                RefreshWorldListAsync();
                _currentWorldParameter.Clear();
            }
            catch (Exception e)
            {
                logger.Error("Error while trying to delete the files from the {0} world : {1}", info.WorldParameters.WorldName, e.Message);
                throw;
            }
        }

        private void RefreshWorldList()
        {
            LocalWorlds.LocalWorldsParams = LocalWorlds.GetAllSinglePlayerWorldsParams(_vars.ApplicationDataPath);
            NeedShowResults = true;
        }

        public void ShowResults()
        {
            //Refresh the items in the list box
            //Insert the various single world present on the computer
            _savedGameList.Items.Clear();
            _savedGameList.SelectItem = -1;
            foreach (var worldp in LocalWorlds.LocalWorldsParams)
            {
                _savedGameList.Items.Add(worldp);
            }

            if (_savedGameList.Items.Count > 0)
            {
                _savedGameList.SelectItem = 0;
            }
            NeedShowResults = false;
        }

        public void RefreshWorldListAsync()
        {
            _savedGameList.Items.Clear();
            _savedGameList.SelectItem = -1;
            _savedGameList.Items.Add("Loading...");
            NeedShowResults = false;
            ThreadsManager.RunAsync(RefreshWorldList);
        }
        #endregion
    }
}
