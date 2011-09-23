using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utopia.GUI.Forms.CustControls
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
