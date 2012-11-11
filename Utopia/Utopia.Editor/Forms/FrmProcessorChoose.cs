using System;
using System.Windows.Forms;
using Utopia.Shared.Configuration;

namespace Utopia.Editor.Forms
{
    public partial class FrmProcessorChoose : Form
    {
        public WorldConfiguration.WorldProcessors SelectedProcessor
        {
            get { return (WorldConfiguration.WorldProcessors)comboBox1.SelectedItem; }
        }

        public FrmProcessorChoose()
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
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
