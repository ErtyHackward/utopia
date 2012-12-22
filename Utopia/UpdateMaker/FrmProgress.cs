using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UpdateMaker
{
    public partial class FrmProgress : Form
    {
        public string Label
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public float Progress
        {
            get { return progressBar1.Value / 100f; }
            set { progressBar1.Value = (int)(value * 100f); }
        }

        public event EventHandler Cancel;

        protected virtual void OnCancel()
        {
            EventHandler handler = Cancel;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public FrmProgress()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnCancel();
        }
    }
}
