using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sandbox.Client.GUI.Forms.CustControls
{
    public partial class PickUpList : Form
    {
        public string PickedPack;

        public PickUpList()
        {
            InitializeComponent();
        }

        public void SetItems(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                listBox1.Items.Add(item.Split('\\')[1]);
            }

            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                PickedPack = listBox1.SelectedItem.ToString();
            }
            this.Hide();
        }
    }
}
