using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.GUI.Forms.CustControls;
using Utopia.Network;

namespace Utopia.GUI.Forms
{
    public partial class WelcomeScreen : Form
    {
        internal FormData Data;

        private delegate void SetTextCallback(string text);
        private delegate void DefaultCallback();
        private SinglePlayer _singleChild = new SinglePlayer();
        private MultiPlayer _multiChild = new MultiPlayer();
        private Config _configChild = new Config();
        private Server _server;

        Timer m_TimerFadeIn = new Timer()
        {
            Interval = 50
        };

        public WelcomeScreen(Server server, bool withFadeIn)
        {
            InitializeComponent();

            _server = server;
            _singleChild = new SinglePlayer();
            _multiChild = new MultiPlayer();
            _configChild = new Config();

            if(withFadeIn) FadeInWinForm();
            _singleChild.btNew.Click += new EventHandler(btNew_Click);
            _multiChild.btConnect.Click += new EventHandler(btConnect_Click);
        }

        void btConnect_Click(object sender, EventArgs e)
        {
            _server.BindingServer(_multiChild.txtSrvAdress.Text, 4815);
            RegisterEvents();
            _server.ConnectToServer(_multiChild.txtUser.Text, _multiChild.txtPassword.Text, _multiChild.chkRegistering.Checked);
        }

        private void RegisterEvents()
        {
            _server.ServerConnection.MessageError += ServerConnection_MessageError;
            _server.ServerConnection.MessageLoginResult += ServerConnection_MessageLoginResult;
        }

        //Handle server Error Message
        void ServerConnection_MessageError(object sender, Net.Connections.ProtocolMessageEventArgs<Net.Messages.ErrorMessage> e)
        {
            AddTextToListBox(e.Message.Message);
        }

        void ServerConnection_MessageLoginResult(object sender, Net.Connections.ProtocolMessageEventArgs<Net.Messages.LoginResultMessage> e)
        {
            if (e.Message.Logged) HideWindows();
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
