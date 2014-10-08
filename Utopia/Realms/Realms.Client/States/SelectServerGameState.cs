using System;
using System.Collections.Generic;
using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Net.Web.Responses;

namespace Realms.Client.States
{
    public class SelectServerGameState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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

        void List_SelectionChanged(object sender, EventArgs e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            if (selection.List.SelectedItems.Count != 1)
            {
                selection.Description = "";
                return;
            }

            var server = ServerList[selection.List.SelectedItems[0]];
            selection.Description = server.Description;
        }

        void SelectionConnectPressed(object sender, EventArgs e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            var vars = _iocContainer.Get<RealmRuntimeVariables>();

            vars.SinglePlayer = false;

            var item = ServerList[selection.List.SelectedItems[0]];
            vars.CurrentServerAddress = item.ServerAddress + ":" + item.Port;
            vars.CurrentServerLocalAddress = string.IsNullOrEmpty(item.LocalAddress) ? null : item.LocalAddress + ":" + item.Port;

            logger.Info("Connecting to {0} {1}", item.ServerName, item.ServerAddress);

            StatesManager.ActivateGameStateAsync("LoadingGame");
        }
        
        void WebApiServerListReceived(object sender, ServerListResponse e)
        {
            var gui = _iocContainer.Get<GuiManager>();

            gui.RunInGuiThread(() => { 
                var selection = _iocContainer.Get<ServerSelectionComponent>();

                if (e.Exception == null)
                {
                    selection.List.Items.Clear();

#if DEBUG
                    if (e.Servers == null)
                        e.Servers = new List<ServerInfo>();
                    e.Servers.Add(new ServerInfo { ServerAddress = "127.0.0.1", Port = 4815, ServerName = "localhost" });
#endif

                    if (e.Servers != null)
                    {
                        foreach (var serverInfo in e.Servers)
                        {
                            selection.List.Items.Add(string.Format("{0} ({1})", serverInfo.ServerName, serverInfo.UsersCount));
                        }
                        ServerList = e.Servers;
                    }
                }
                else
                {
                    selection.List.Items.Clear();
                    selection.List.Items.Add("Error!");
                }
            });
        }

        public override void OnEnabled(GameState previousState)
        {
            var webApi = _iocContainer.Get<ClientWebApi>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            selection.List.Items.Clear();
            selection.List.Items.Add("Loading...");

            selection.List.SelectionChanged += List_SelectionChanged;

            webApi.GetServersListAsync();
        }

        public override void OnDisabled(GameState nextState)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();
            selection.List.SelectionChanged -= List_SelectionChanged;
        }

        void SelectionBackPressed(object sender, EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.ActivateGameStateAsync("MainMenu");
        }
    }
}
