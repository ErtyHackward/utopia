using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Network;
using LostIslandHD.Client.GUI.Forms.CustControls;
using System.IO;

namespace LostIsland.Client.GUI.Forms.CustControls
{
    public partial class MultiPlayer : UserControl
    {
        public MultiPlayer()
        {
            InitializeComponent();
        }

        private void pickTexturePack_Click(object sender, EventArgs e)
        {
            PickUpList popUp = new PickUpList();

            var files = Directory.GetDirectories(@"TexturesPacks\");

            popUp.SetItems(files);
            popUp.ShowDialog(this);

            txtTexturePack.Text = popUp.PickedPack != "" ? popUp.PickedPack : "Default";
        }

        private void PickEffectPack_Click(object sender, EventArgs e)
        {
            PickUpList popUp = new PickUpList();

            var files = Directory.GetDirectories(@"EffectsPacks\");

            popUp.SetItems(files);
            popUp.ShowDialog(this);

            txtEffectPack.Text = popUp.PickedPack != "" ? popUp.PickedPack : "Default";
        }
    }

    public class RefreshingListBox : ListBox
    {
        public new void RefreshItem(int index)
        {
            base.RefreshItem(index);
        }

        public new void RefreshItems()
        {
            base.RefreshItems();
        }
    }
}
