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

        private SinglePlayer singleChild = new SinglePlayer();
        private MultiPlayer multiChild = new MultiPlayer();
        private Config configChild = new Config();
        private Server server;

        Timer m_TimerFadeIn = new Timer()
        {
            Interval = 50
        };

        public WelcomeScreen(bool withFadeIn)
        {
            InitializeComponent();
            if(withFadeIn) FadeInWinForm();
            singleChild.btNew.Click += new EventHandler(btNew_Click);
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
                Seed = singleChild.txtSeed.Text,
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
                Seed = singleChild.txtSeed.Text,
                WorldName = "",
                NewStruct = false
            };

            this.Hide();
        }

        private void btSinglePlayer_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(128, 255, 255, 255);
            this.ChildContainer.Controls.Add(singleChild);
        }

        private void btMultiPlayer_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(128, 255, 255, 255);
            this.ChildContainer.Controls.Add(multiChild);
        }

        private void btConfig_Click(object sender, EventArgs e)
        {
            this.ChildContainer.Controls.Clear();
            this.ChildContainer.BackColor = Color.FromArgb(64, 255, 255, 255);
            configChild.RefreshAllBindingLists(Utopia.Settings.ClientSettings.Current.Settings);
            this.ChildContainer.Controls.Add(configChild);
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
