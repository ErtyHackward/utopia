using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
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

        #region Public variable/properties
        #endregion

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
                Directory.Delete(info.ServerRootPath.FullName, true);

                //Try to delete the Client directory
                Directory.Delete(info.ClientRootPath.FullName, true);

                //Recreate the list of all existing Worlds, as one has been deleted
                GameSystemSettings.LocalWorldsParams = LocalWorlds.GetAllSinglePlayerWorldsParams(_vars.ApplicationDataPath);

                RefreshWorldList();
                _currentWorldParameter.Clear();
            }
            catch (Exception e)
            {
                logger.Error("Error while trying to delete the files from the {0} world : {1}", info.WorldParameters.WorldName, e.Message);
                throw;
            }
        }

        public void RefreshWorldList()
        {
            GameSystemSettings.LocalWorldsParams = LocalWorlds.GetAllSinglePlayerWorldsParams(_vars.ApplicationDataPath);
            //Refresh the items in the list box
            //Insert the various single world present on the computer
            _savedGameList.Items.Clear();
            _savedGameList.SelectItem = -1;
            foreach (LocalWorlds.LocalWorldsParam worldp in GameSystemSettings.LocalWorldsParams)
            {
                _savedGameList.Items.Add(worldp);
            }

            if (_savedGameList.Items.Count > 0)
            {
                _savedGameList.SelectItem = 0;
            }
        }
        #endregion
    }
}
