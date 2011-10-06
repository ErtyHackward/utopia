using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Network;

namespace LostIsland.Client.GUI.Forms.CustControls
{
    public partial class MultiPlayer : UserControl
    {
        public MultiPlayer()
        {
            InitializeComponent();
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
