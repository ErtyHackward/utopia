using System;
using System.Collections.Generic;
using Ninject;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Sandbox.Client.Components.GUI;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Net.Web.Responses;

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

        public SelectServerGameState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var gui = _iocContainer.Get<GuiManager>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            var webApi = _iocContainer.Get<ClientWebApi>();

            GameComponents.Add(gui);
            GameComponents.Add(selection);

            selection.BackPressed += SelectionBackPressed;
            selection.ConnectPressed += SelectionConnectPressed;

            webApi.ServerListReceived += WebApiServerListReceived;

            base.Initialize(context);
        }

        void SelectionConnectPressed(object sender, EventArgs e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            var vars = _iocContainer.Get<RuntimeVariables>();

            vars.SinglePlayer = false;
            vars.CurrentServerAddress = ServerList[selection.List.SelectedItems[0]].ServerAddress;

            StatesManager.ActivateGameStateAsync("LoadingGame");
        }

        void WebApiServerListReceived(object sender, WebEventArgs<ServerListResponse> e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            if (e.Exception == null)
            {
                selection.List.Items.Clear();

                if (e.Response.Servers != null)
                {
                    foreach (var serverInfo in e.Response.Servers)
                    {
                        selection.List.Items.Add(string.Format("{0} ({1})", serverInfo.ServerName, serverInfo.UsersCount) );
                    }
                    ServerList = e.Response.Servers;
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
            StatesManager.ActivateGameStateAsync("MainMenu");
        }
    }
}
