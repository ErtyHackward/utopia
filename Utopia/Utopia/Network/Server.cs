﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.DebugTools;
using Utopia.Shared;
using Utopia.Shared.ClassExt;
using S33M3Engines.D3D;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;

namespace Utopia.Network
{
    public class Server : GameComponent, IDisposable ,IDebugInfo
    {
        #region Private variables
        #endregion

        #region Public properties/variables
        public string Address { get; set; }
        public bool Connected { get; set; }
        public ServerConnection ServerConnection { get; set; }

        //Initilialization received Data, should be move inside a proper class/struct !
        public PlayerCharacter Player { get; set; }
        public DateTime WorldDateTime { get; set; }
        public double TimeFactor { get; set; }
        public GameInformationMessage GameInformations { get; set; }
        //===============================================================================================
        #endregion

        public Server()
        {
            Connected = false;
        }

        public bool BindingServer(string address)
        {
            if (Address == address) return false;
            if (ServerConnection != null && ServerConnection.ConnectionStatus == ConnectionStatus.Connected) ServerConnection.Disconnect();

            Address = address;

            ServerConnection = new ServerConnection(address);
            //Register Login Events
            //ServerConnection.MessageLoginResult += _server_MessageLoginResult;
            //ServerConnection.ConnectionStatusChanged += _server_ConnectionStatusChanged;
            //ServerConnection.MessageBlockChange += _server_MessageBlockChange;
            //ServerConnection.MessageChat += _server_MessageChat;
            //ServerConnection.MessageChunkData += _server_MessageChunkData;
            //ServerConnection.MessageDateTime += _server_MessageDateTime;
            //ServerConnection.MessageDirection += _server_MessageDirection;
            //ServerConnection.MessageError += _server_MessageError;
            //ServerConnection.MessageGameInformation += _server_MessageGameInformation;
            //ServerConnection.MessageEntityIn += _server_MessagePlayerIn;
            //ServerConnection.MessageEntityOut += _server_MessagePlayerOut;
            //ServerConnection.MessagePosition += _server_MessagePosition;
            //ServerConnection.MessageDateTime += new ServerConnection_MessageDateTime;
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
        }

        #region Events Handlings Methods

        void ServerConnection_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            Console.WriteLine("hours");
        }

        void _server_MessageLoginResult(object sender, ProtocolMessageEventArgs<LoginResultMessage> e)
        {
            //if (e.Message.Logged)
            //{
            //    Console.WriteLine("I'm Logged");
            //}
        }

        void _server_MessagePosition(object sender, ProtocolMessageEventArgs<EntityPositionMessage> e)
        {
            //throw new NotImplementedException();
        }

        void _server_MessagePlayerOut(object sender, ProtocolMessageEventArgs<EntityOutMessage> e)
        {
            //throw new NotImplementedException();
        }

        void _server_MessagePlayerIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        {
            //Console.WriteLine("_server_MessagePlayerIn : " + e.Message.Entity.DisplayName);
        }

        void _server_MessageGameInformation(object sender, ProtocolMessageEventArgs<GameInformationMessage> e)
        {
            //Console.WriteLine("_server_MessageGameInformation : " + e.Message.ChunkSize.ToString());
        }

        void _server_MessageError(object sender, ProtocolMessageEventArgs<ErrorMessage> e)
        {
            Console.WriteLine("_server_MessageGameInformation : " + e.Message.Message.ToString());
        }

        void _server_MessageDirection(object sender, ProtocolMessageEventArgs<EntityDirectionMessage> e)
        {
            //Console.WriteLine("_server_MessageGameInformation : " + e.Message.Message.ToString());
        }

        void _server_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            //throw new NotImplementedException();
        }

        void _server_MessageChunkData(object sender, ProtocolMessageEventArgs<ChunkDataMessage> e)
        {
            //Console.WriteLine("_server_MessageChunkData : " + e.Message.Position.ToString() + " " + e.Message.Data.Length + " bytes");
        }

        void _server_MessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            //throw new NotImplementedException();
        }

        void _server_MessageBlockChange(object sender, ProtocolMessageEventArgs<BlocksChangedMessage> e)
        {
            //Console.WriteLine("_server_MessageBlockChange : ");
        }

        void _server_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatus.Disconnected)
            {
                //Display Error message !!! 
                
            }
        }

        #endregion

        #region Public Methods
        public void ConnectToServer(string UserName, string Password, bool withRegistering)
        {
            if(ServerConnection.LoggedOn)
                ServerConnection.Disconnect();
            
            ServerConnection.Login = UserName;
            ServerConnection.Password = Password.GetMd5Hash();
            ServerConnection.ClientVersion = 1;
            ServerConnection.Register = withRegistering;

            if (ServerConnection.ConnectionStatus != ConnectionStatus.Connected)
            {
                ServerConnection.ConnectAsync();
            }
            else
            {
                ServerConnection.Authenticate();
            }
        }

        public override void Update(ref GameTime TimeSpend)
        {
            ServerConnection.FetchPendingMessages();
        }

        #endregion

        #region Private Methods
        //Raise when a block has been changed ==> To be sent to the server
        private void ChunkContainer_BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            //for(int blockChangeIndex = 0; blockChangeIndex < e.Count; blockChangeIndex++)
            //{
            //    ServerConnection.SendAsync(new BlockChangeMessage()
            //    {
            //        BlockPosition = e.Locations[blockChangeIndex],
            //        BlockType = e.Bytes[blockChangeIndex]
            //    });
            //    Console.WriteLine("Fired");
            //}
        }
        #endregion



        public string GetInfo()
        {
            return string.Format("Received: {1} Receive speed: {0}", BytesHelper.FormatBytes(ServerConnection.AverageReceiveSpeed), BytesHelper.FormatBytes(ServerConnection.TotalBytesReceived));
        }
    }
}
