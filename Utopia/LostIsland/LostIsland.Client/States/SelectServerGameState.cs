using System;
using System.Collections.Generic;
using Ninject;
using Sandbox.Client.Components;
using Sandbox.Shared.Web;
using Sandbox.Shared.Web.Responces;
using Utopia;
using Utopia.GUI.D3D;

namespace Sandbox.Client.States
{
    public class SelectServerGameState : GameState
    {
        private readonly IKernel _iocContainer;

        public List<ServerInfo> ServerList { get; set; }

        public override string Name
        {
            get { return "SelectServer"; }
        }

        public SelectServerGameState(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize()
        {
            var gui = _iocContainer.Get<GuiManager>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            var webApi = _iocContainer.Get<ClientWebApi>();

            EnabledComponents.Add(gui);
            EnabledComponents.Add(selection);

            VisibleComponents.Add(gui);

            selection.BackPressed += SelectionBackPressed;
            selection.ConnectPressed += SelectionConnectPressed;

            webApi.ServerListReceived += WebApiServerListReceived;
        }

        void SelectionConnectPressed(object sender, EventArgs e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            var vars = _iocContainer.Get<RuntimeVariables>();

            vars.SinglePlayer = false;
            vars.CurrentServerAddress = ServerList[selection.List.SelectedItems[0]].ServerAddress;

            StatesManager.SetGameState("GameLoading");
        }

        void WebApiServerListReceived(object sender, WebEventArgs<ServerListResponce> e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            if (e.Exception == null)
            {
                selection.List.Items.Clear();

                if (e.Responce.Servers != null)
                {
                    foreach (var serverInfo in e.Responce.Servers)
                    {
                        selection.List.Items.Add(string.Format("{0} ({1})", serverInfo.ServerName, serverInfo.UsersCount) );
                    }
                    ServerList = e.Responce.Servers;
                }
            }
            else
            {
                selection.List.Items.Clear();
                selection.List.Items.Add("Error!");
            }

        }

        public override void OnEnabled(GameState previousState)
        {
            var webApi = _iocContainer.Get<ClientWebApi>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            selection.List.Items.Clear();
            selection.List.Items.Add("Loading...");

            webApi.GetServersListAsync();
        }

        void SelectionBackPressed(object sender, EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
