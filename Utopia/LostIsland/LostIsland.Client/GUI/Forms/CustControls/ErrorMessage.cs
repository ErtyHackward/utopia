using System;
using System.Windows.Forms;

namespace Sandbox.Client.GUI.Forms.CustControls
{
    public partial class ErrorMessage : UserControl
    {
        public ErrorMessage()
        {
            InitializeComponent();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
