using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Network;

namespace Utopia.GUI.Forms.CustControls
{
    public partial class MultiPlayer : UserControl
    {
        private Server _server;

        public MultiPlayer()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            _server = new Server(txtSrvAdress.Text, 4815);
            _server.ConnectToServer(txtUser.Text, txtPassword.Text, chkRegistering.Checked);
        }
    }
}
