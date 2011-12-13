using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LostIsland.Client.Components;
using LostIsland.Shared.Web;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    public class SelectServerGameState : GameState
    {
        private readonly IKernel _iocContainer;

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
            var webApi = _iocContainer.Get<UtopiaWebApi>();

            EnabledComponents.Add(gui);
            EnabledComponents.Add(selection);

            VisibleComponents.Add(gui);

            selection.BackPressed += SelectionBackPressed;

            webApi.ServerListReceived += webApi_ServerListReceived;


        }

        void webApi_ServerListReceived(object sender, WebEventArgs<UtopiaApi.Models.ServerListResponce> e)
        {
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            if (e.Exception == null)
            {
                selection.List.Items.Clear();

                if (e.Responce.Servers != null)
                {

                    foreach (var serverInfo in e.Responce.Servers)
                    {
                        selection.List.Items.Add(serverInfo.ServerName);
                    }
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
            var webApi = _iocContainer.Get<UtopiaWebApi>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            selection.List.Items.Clear();
            selection.List.Items.Add("Loading...");

            webApi.GetServersList();
        }

        void SelectionBackPressed(object sender, System.EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
