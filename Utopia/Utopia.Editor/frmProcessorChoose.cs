using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Utopia.Shared.Configuration;

namespace Utopia.Editor
{
    public partial class frmProcessorChoose : Form
    {
        public WorldConfiguration.WorldProcessors SelectedProcessor
        {
            get { return (WorldConfiguration.WorldProcessors)comboBox1.SelectedItem; }
        }

        public frmProcessorChoose()
        {
            InitializeComponent();

            int UtopiaIndex = 0;
            int cpt = 0;

            foreach (var processor in Enum.GetValues(typeof(Utopia.Shared.Configuration.WorldConfiguration.WorldProcessors)))
            {
                comboBox1.Items.Add(processor);
                if (processor.ToString() == "Utopia") UtopiaIndex = cpt; 
                cpt++;
            }
            comboBox1.SelectedIndex = UtopiaIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
