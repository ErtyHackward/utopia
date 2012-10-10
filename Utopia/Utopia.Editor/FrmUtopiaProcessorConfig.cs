using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Configuration;

namespace Utopia.Editor
{
    public partial class FrmUtopiaProcessorConfig : UserControl
    {
        public TreeView tvBiomesList { get { return this.tvBiomeList; } }
        public PropertyGrid gpBiome { get { return this.gpBiome; } }

        public FrmUtopiaProcessorConfig()
        {
            InitializeComponent();
            worldType.SelectedIndex = 0;
        }

        public FrmUtopiaProcessorConfig(UtopiaProcessorParams param)
            :base()
        {
            LoadConfigParam(param);
        }

        public void LoadConfigParam(UtopiaProcessorParams param)
        {
            rangeBarBasicPlain.Ranges.Clear();
            foreach (var item in param.BasicPlain)
            {
                rangeBarBasicPlain.Ranges.Add(item);
            }

            rangeBarBasicMidLand.Ranges.Clear();
            foreach (var item in param.BasicMidland)
            {
                rangeBarBasicMidLand.Ranges.Add(item);
            }

            rangeBarBasicMontain.Ranges.Clear();
            foreach (var item in param.BasicMontain)
            {
                rangeBarBasicMontain.Ranges.Add(item);
            }

            rangeBarBasicOcean.Ranges.Clear();
            foreach (var item in param.BasicOcean)
            {
                rangeBarBasicOcean.Ranges.Add(item);
            }

            rangeBarGround.Ranges.Clear();
            foreach (var item in param.Ground)
            {
                rangeBarGround.Ranges.Add(item);
            }

            rangeBarOcean.Ranges.Clear();
            foreach (var item in param.Ocean)
            {
                rangeBarOcean.Ranges.Add(item);
            }

            rangeBarWorld.Ranges.Clear();
            foreach (var item in param.World)
            {
                rangeBarWorld.Ranges.Add(item);
            }
        }

        private void tvBiomeList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pgBiomes.SelectedObject = tvBiomeList.SelectedNode.Tag;
        }
    }
}
