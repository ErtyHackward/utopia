using System;
using System.Windows.Forms;

namespace Utopia.Editor.Forms
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
