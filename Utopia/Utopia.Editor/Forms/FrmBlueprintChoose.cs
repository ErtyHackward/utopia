using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Utopia.Editor.Forms
{
    public partial class FrmBlueprintChoose : Form
    {
        public ushort SelectedBlueprint { get; set; }

        public FrmBlueprintChoose(IEnumerable<KeyValuePair<ushort,string>> items)
        {
            InitializeComponent();
            
            listBoxTypes.DisplayMember = "Value";
            foreach (var keyValuePair in items)
            {
                listBoxTypes.Items.Add(keyValuePair);
            }
        }

        private void listBoxTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedBlueprint = ((KeyValuePair<ushort, string>)listBoxTypes.SelectedItem).Key;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
