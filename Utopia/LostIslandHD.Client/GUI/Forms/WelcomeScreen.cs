using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LostIsland.Client.GUI.Forms.CustControls;
using Utopia.Network;
using System.Threading;
using Utopia.Settings;
using S33M3Engines.D3D;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using ErrorMessage = LostIsland.Client.GUI.Forms.CustControls.ErrorMessage;

namespace LostIsland.Client.GUI.Forms
{
    public partial class WelcomeScreen : Form, IDisposable
    {
        public FormData Data;
        private delegate void SetBoolCallback(bool text);
        private delegate void SetTextCallback(string text);
        private delegate void DefaultCallback();
        private SinglePlayer _singleChild = new SinglePlayer();
        private MultiPlayer _multiChild = new MultiPlayer();
        private ErrorMessage _errorMsg = new ErrorMessage();
        private Config _configChild = new Config();
        private Server _server;
        private TimerCallback _timerDelegate;
        private System.Threading.Timer _serverTime;

        System.Windows.Forms.Timer m_TimerFadeIn = new System.Windows.Forms.Timer()
        {
            Interval = 50
        };

        public GameExitReasonMessage ExitReason { get; set; }

        public WelcomeScreen(Server server, bool withFadeIn)
        {
            InitializeComponent();

            _timerDelegate = new TimerCallback(ServerTime_Tick);

            _server = server;
            _singleChild = new SinglePlayer();
            _multiChild = new MultiPlayer();
            _configChild = new Config();

            if(withFadeIn) FadeInWinForm();
            _singleChild.btNew.Click += new EventHandler(btNew_Click);
            _multiChild.btConnect.Click += new EventHandler(btConnect_Click);
            _multiChild.btSave.Click += new EventHandler(btSave_Click);
            _multiChild.srvList.KeyDown += new KeyEventHandler(srvList_KeyDown);
            _multiChild.srvList.SelectedIndexChanged += new EventHandler(srvList_SelectedIndexChanged);
            InitServerList();
        }

        public void CleanUp()
        {
            _singleChild.btNew.Click -= btNew_Click;
            _multiChild.btConnect.Click -= btConnect_Click;
            _multiChild.btSave.Click -= btSave_Click;
            _multiChild.srvList.KeyDown -= srvList_KeyDown;
            _multiChild.srvList.SelectedIndexChanged -= srvList_SelectedIndexChanged;
            _timerDelegate = null;
            _server = null;
            _singleChild = null;
            _multiChild = null;
            _configChild = null;
        }

        //MultiPlayer component =================================================================================

        void srvList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_multiChild.srvList.SelectedIndex >= 0)
            {
                ServerSetting serverItem = (ServerSetting)_multiChild.srvList.Items[_multiChild.srvList.SelectedIndex];
                _multiChild.txtSrvAdress.Text = serverItem.IPAddress;
                _multiChild.txtServerName.Text = serverItem.ServerName;
                _multiChild.txtUser.Text = serverItem.DefaultUser;
                _multiChild.txtPassword.Text = "";
            }
        }

        void btConnect_Click(object sender, EventArgs e)
        {
            _multiChild.btConnect.Enabled = false;


            //Validate the TCP IP adress
            if (_server.BindingServer(_multiChild.txtSrvAdress.Text))
            {
                RegisterEvents();
            }
            _server.ConnectToServer(_multiChild.txtUser.Text, _multiChild.txtPassword.Text, _multiChild.chkRegistering.Checked);

            _serverTime = new System.Threading.Timer(_timerDelegate, null, 100, 100);
        }

        private void InitServerList()
        {
            if (ClientSettings.Current.Settings.ServersList == null) ClientSettings.Current.Settings.ServersList = new ServersList();
            foreach (var server in ClientSettings.Current.Settings.ServersList.Servers)
            {
                AddServerToList(server);
            }

            if (_multiChild.srvList.Items.Count > 0)
            {
                _multiChild.srvList.SelectedIndex = 0;
            }
        }

        void btSave_Click(object sender, EventArgs e)
        {
            AddServerToList();
            SaveConfigurationFile();
        }

        private void AddServerToList(ServerSetting server = null)
        {
            if (server == null)
            {
                //Is the server already existing (With same IP) ??
                if (ClientSettings.Current.Settings.ServersList.Servers.Where(x => x.IPAddress == _multiChild.txtSrvAdress.Text).Count() > 0)
                {
                    //Server already existing => Udpate it
                    server = ClientSettings.Current.Settings.ServersList.Servers.Find(x => x.IPAddress == _multiChild.txtSrvAdress.Text);
                    server.ServerName = _multiChild.txtServerName.Text;
                    server.DefaultUser = _multiChild.txtUser.Text;

                    foreach (var serverInListBoxItem in _multiChild.srvList.Items)
                    {
                        if (((ServerSetting)serverInListBoxItem).IPAddress == _multiChild.txtSrvAdress.Text)
                        {
                            ((ServerSetting)serverInListBoxItem).ServerName = _multiChild.txtServerName.Text;
                            ((ServerSetting)serverInListBoxItem).DefaultUser = _multiChild.txtUser.Text;
                            _multiChild.srvList.RefreshItems();
                        }
                    }

                    return;
                }

                server = new ServerSetting() { IPAddress = _multiChild.txtSrvAdress.Text, ServerName = _multiChild.txtServerName.Text, DefaultUser = _multiChild.txtUser.Text };
                ClientSettings.Current.Settings.ServersList.Servers.Add(server);
            }

            _multiChild.srvList.Items.Add(server);
        }

        void srvList_KeyDown(object sender, KeyEventArgs e)
        {
            //Get selected listbox item
            if (e.KeyCode == Keys.Delete && _multiChild.srvList.SelectedIndex >= 0)
            {
                //Get Item
                ServerSetting serverItem = (ServerSetting)_multiChild.srvList.Items[_multiChild.srvList.SelectedIndex];
                _multiChild.srvList.Items.RemoveAt(_multiChild.srvList.SelectedIndex);

                if (_multiChild.srvList.Items.Count > 0) _multiChild.srvList.SelectedIndex = 0;
                else _multiChild.srvList.SelectedIndex = -1;

                ClientSettings.Current.Settings.ServersList.Servers.RemoveAll(x => x.ServerName == serverItem.ServerName && x.IPAddress == serverItem.IPAddress);
                SaveConfigurationFile();
            }
            e.Handled = true;
        }

        private void SaveConfigurationFile()
        {
            ClientSettings.Current.Save();
        }

        //MultiPlayer component =================================================================================

        private void RegisterEvents()
        {
            _server.ServerConnection.ConnectionStatusChanged += ServerConnection_ConnectionStatusChanged;
            _server.ServerConnection.MessageError += ServerConnection_MessageError;
            _server.ServerConnection.MessageLoginResult += ServerConnection_MessageLoginResult;
            _server.ServerConnection.MessageGameInformation += ServerConnection_MessageGameInformation;
            _server.ServerConnection.MessagePing += ServerConnection_MessagePing;
            //_server.ServerConnection.MessageEntityIn += ServerConnection_MessageEntityIn;
            _server.ServerConnection.MessageDateTime += ServerConnection_MessageDateTime;
        }

        private void UnregisterEvents()
        {
            _server.ServerConnection.ConnectionStatusChanged -= ServerConnection_ConnectionStatusChanged;
            _server.ServerConnection.MessageError -= ServerConnection_MessageError;
            _server.ServerConnection.MessageLoginResult -= ServerConnection_MessageLoginResult;
            _server.ServerConnection.MessageGameInformation -= ServerConnection_MessageGameInformation;
            _server.ServerConnection.MessagePing -= ServerConnection_MessagePing;
            //_server.ServerConnection.MessageEntityIn -= ServerConnection_MessageEntityIn;
            _server.ServerConnection.MessageDateTime -= ServerConnection_MessageDateTime;
        }

        //void ServerConnection_MessageEntityIn(object sender, ProtocolMessageEventArgs<EntityInMessage> e)
        //{
        //    _server.Player = (Utopia.Shared.Chunks.Entities.PlayerCharacter)e.Message.Entity;
        //}

        void ServerConnection_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            _server.WorldDateTime = e.Message.DateTime;
            _server.TimeFactor = e.Message.TimeFactor;

            UnregisterEvents();
            _serverTime.Dispose();
            HideWindows();
        }

        void ServerConnection_MessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            Console.WriteLine("Ping Time : " + (double)(System.Diagnostics.Stopwatch.GetTimestamp() - e.Message.Token) / System.Diagnostics.Stopwatch.Frequency + " sec.");
        }

        void ServerConnection_MessageGameInformation(object sender, ProtocolMessageEventArgs<GameInformationMessage> e)
        {
            AddTextToListBox("Game Information received - starting game ... ");
            _server.MaxServerViewRange = e.Message.MaxViewRange;
            _server.ChunkSize = e.Message.ChunkSize;
            _server.SeaLevel = e.Message.WaterLevel;
            _server.WorldSeed = e.Message.WorldSeed;
        }

        void ServerConnection_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            string ErrorString = "";
            if (e.Exception != null)
            {
                ErrorString = " => " + e.Exception.Message;
                NetworkConnectBtState(true);
                _serverTime.Dispose();
            }
            AddTextToListBox("Connection status : " + e.Status.ToString() + ErrorString);
        }

        //Handle server Error Message
        void ServerConnection_MessageError(object sender, ProtocolMessageEventArgs<Utopia.Shared.Net.Messages.ErrorMessage> e)
        {
            AddTextToListBox(e.Message.Message);
            NetworkConnectBtState(true);
            _serverTime.Dispose();
        }

        void ServerConnection_MessageLoginResult(object sender, ProtocolMessageEventArgs<LoginResultMessage> e)
        {
            AddTextToListBox("Login result : " + e.Message.Logged.ToString());
            //_serverTime = new System.Threading.Timer(_timerDelegate, null, 100, 100 );
        }

        private void ServerTime_Tick(object stateInfo)
        {
            _server.ServerConnection.FetchPendingMessages(1);
        }

        private void HideWindows()
        {
            if (_multiChild.lstServerCom.InvokeRequired)
            {
                DefaultCallback d = new DefaultCallback(HideWindows);
                this.Invoke(d);
            }
            else
            {
                //Create the Single Player NEW world data message
                Data = new FormData();
                Data.RequestAction = FormRequestedAction.StartMultiPlayer;
                Data.SinglePlData = new FormData.SinglePlayerData()
                {
                    isNew = true,
                    SavedGameId = "",
                    Seed = _singleChild.txtSeed.Text,
                    WorldName = "",
                    NewStruct = false
                };
                this.Hide();
            }
        }

        private void AddTextToListBox(string text)
        {
            if (_multiChild.lstServerCom.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AddTextToListBox);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                _multiChild.lstServerCom.Items.Add(text);
                _multiChild.lstServerCom.SelectedIndex = _multiChild.lstServerCom.Items.Count - 1;
                _multiChild.lstServerCom.SelectedIndex = -1;
            }
        }

        private void NetworkConnectBtState(bool state)
        {
            if (_multiChild.btConnect.InvokeRequired)
            {
                SetBoolCallback d = new SetBoolCallback(NetworkConnectBtState);
                this.Invoke(d, new object[] { state });
            }
            else
            {
                _multiChild.btConnect.Enabled = state;
            }
        }


        private void FadeInWinForm()
        {
            m_TimerFadeIn.Start();

            this.Opacity = 0;
            m_TimerFadeIn.Tick += delegate(object sender, EventArgs e)
            {
                this.Opacity += .05;
                if (this.Opacity >= 1)
                    m_TimerFadeIn.Stop();
            };
        }

        void btNewArch_Click(object sender, EventArgs e)
        {
            //Create the Single Player NEW world data message
            Data = new FormData();
            Data.RequestAction = FormRequestedAction.StartSinglePlayer;
            Data.SinglePlData = new FormData.SinglePlayerData()
            {
                isNew = true,
                SavedGameId = "",
                Seed = _singleChild.txtSeed.Text,
                WorldName = "",
                NewStruct = true
            };

            this.Hide();
        }

        void btNew_Click(object sender, EventArgs e)
        {
            //Create the Single Player NEW world data message
            Data = new FormData();
            Data.RequestAction = FormRequestedAction.StartSinglePlayer;
            Data.SinglePlData = new FormData.SinglePlayerData()
            {
                isNew = true,
                SavedGameId = "",
                Seed = _singleChild.txtSeed.Text,
                WorldName = "",
                NewStruct = false
            };

            this.Hide();
        }

        private void btSinglePlayer_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(128, 255, 255, 255);
            this.ChildContainer.Controls.Add(_singleChild);
        }

        private void btMultiPlayer_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(128, 255, 255, 255);
            this.ChildContainer.Controls.Add(_multiChild);
        }

        private void btConfig_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(64, 255, 255, 255);
            _configChild.RefreshAllBindingLists(Utopia.Settings.ClientSettings.Current.Settings);
            this.ChildContainer.Controls.Add(_configChild);
        }

        private void WelcomeScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Create the Single Player NEW world data message
            Data = new FormData();
            Data.RequestAction = FormRequestedAction.ExitGame;
        }

        private void AnalyseExitReason(GameExitReasonMessage exitReason)
        {
            if (exitReason.GameExitReason == S33M3Engines.D3D.ExitReason.Error)
            {
                this.ChildContainer.Controls.Clear();
                this.ChildContainer.BackColor = Color.FromArgb(0, 255, 255, 255);
                this.ChildContainer.Controls.Add(_errorMsg);

                _errorMsg.Message.Text = exitReason.MainMessage;
                _errorMsg.MessageDetail.Text = exitReason.DetailedMessage;
            }
        }

        private void WelcomeScreen_Shown(object sender, EventArgs e)
        {
            AnalyseExitReason(ExitReason);
        }

    }

    public enum FormRequestedAction
    {
        ExitGame,
        StartSinglePlayer,
        StartMultiPlayer
    }

    public class FormData
    {
        public FormRequestedAction RequestAction;
        public SinglePlayerData SinglePlData;
        public MultiplayerData MultiPlData;

        //Single Player Parameters
        public struct SinglePlayerData
        {
            public string Seed;
            public string WorldName;
            public bool isNew;
            public string SavedGameId;
            public bool NewStruct;
        }

        public struct MultiplayerData
        {
        }
    }
}
