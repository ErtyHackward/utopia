using System;
using Utopia.Shared;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using S33M3DXEngine.Main;

namespace Utopia.Network
{
    /// <summary>
    /// Handles the server connection
    /// </summary>
    public class ServerComponent : GameComponent
    {
        
        public string Address { get; set; }
        public ServerConnection ServerConnection { get; set; }

        //Initilialization received Data, should be move inside a proper class/struct !
        public PlayerCharacter Player { get; set; }
        public DateTime WorldDateTime { get; set; }
        public double TimeFactor { get; set; }
        public GameInformationMessage GameInformations { get; set; }

        public string Login { get; set; }
        public string DisplayName { get; set; }

        //===============================================================================================

        /// <summary>
        /// Occurs when component initiliaze the connection
        /// </summary>
        public event EventHandler<ServerComponentConnectionInitializeEventArgs> ConnectionInitialized;

        protected void OnConnectionInitialized(ServerComponentConnectionInitializeEventArgs ea)
        {
            if(ea.ServerConnection != null)
                ea.ServerConnection.MessageGameInformation += ServerConnection_MessageGameInformation;
            if (ea.PrevoiusConnection != null)
                ea.PrevoiusConnection.MessageGameInformation -= ServerConnection_MessageGameInformation;

            var handler = ConnectionInitialized;
            if (handler != null) handler(this, ea);
        }

        void ServerConnection_MessageGameInformation(object sender, ProtocolMessageEventArgs<GameInformationMessage> e)
        {
            GameInformations = e.Message;
        }


        public ServerComponent()
        {
        }

        public bool BindingServer(string address)
        {
            if (Address == address) return false;
            if (ServerConnection != null && ServerConnection.ConnectionStatus == ConnectionStatus.Connected) ServerConnection.Disconnect();

            Address = address;

            var prev = ServerConnection;

            ServerConnection = new ServerConnection(address);
            OnConnectionInitialized(new ServerComponentConnectionInitializeEventArgs { ServerConnection = ServerConnection, PrevoiusConnection = prev });
            return true;
        }

        public override void Dispose()
        {
            if (ServerConnection != null &&
               ServerConnection.ConnectionStatus != ConnectionStatus.Disconnected &&
               ServerConnection.ConnectionStatus != ConnectionStatus.Disconnecting)
            {
                //ServerConnection.MessageBlockChange -= _server_MessageBlockChange;
                ServerConnection.Disconnect();
            }

            if (ServerConnection != null) ServerConnection.Dispose();

            base.Dispose();
        }

        #region Public Methods
        public void ConnectToServer(string userName, string displayName, string passwordHash)
        {
            Login = userName;
            DisplayName = displayName;

            if(ServerConnection.LoggedOn)
                ServerConnection.Disconnect();
            
            ServerConnection.Login = userName;
            ServerConnection.DisplayName = displayName;
            ServerConnection.Password = passwordHash;
            ServerConnection.ClientVersion = 1;
            ServerConnection.Register = false;

            if (ServerConnection.ConnectionStatus != ConnectionStatus.Connected)
            {
                ServerConnection.ConnectAsync();
            }
            else
            {
                ServerConnection.Authenticate();
            }
        }

        public override void Update( GameTime timeSpend)
        {
            if(ServerConnection != null)
                ServerConnection.FetchPendingMessages();
        }

        #endregion

        public string GetInfo()
        {
            return string.Format("Received: {1} Receive speed: {0}", BytesHelper.FormatBytes(ServerConnection.AverageReceiveSpeed), BytesHelper.FormatBytes(ServerConnection.TotalBytesReceived));
        }
    }

    public class ServerComponentConnectionInitializeEventArgs : EventArgs
    {
        public ServerConnection PrevoiusConnection { get; set; }
        public ServerConnection ServerConnection { get; set; }

    }
}
