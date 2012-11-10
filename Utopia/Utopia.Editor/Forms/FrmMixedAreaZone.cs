using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utopia.Editor
{
    public partial class FrmMixedAreaZone : Form
    {
        public int ZoneValue
        {
            get { return (int)TransitionAreaValue.Value; }
            set { TransitionAreaValue.Value = value; }
        }

        public FrmMixedAreaZone()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
