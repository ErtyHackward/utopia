using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Editor
{
    public partial class FrmUtopiaProcessorConfig : UserControl
    {
        public TreeView tvBiomesList { get { return this.tvBiomeList; } }
        public PropertyGrid gpBiome { get { return this.gpBiome; } }
        public RealmConfiguration Configuration { get; set; }

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

        //Add new
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Biome biome = Configuration.CreateNewBiome();

            var item = new TreeNode(biome.Name);
            item.Tag = biome;
            tvBiomeList.Nodes.Add(item);

            //Select last node
            tvBiomeList.SelectedNode = tvBiomeList.Nodes[tvBiomeList.Nodes.Count - 1];
        }

        //Remove Biome
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = tvBiomeList.SelectedNode;
            if (selectedNode != null && tvBiomeList.Nodes.Count > 1)
            {
                Biome biome = (Biome)selectedNode.Tag;
                Configuration.RealmBiomes.Remove(biome);
                tvBiomeList.Nodes.Remove(selectedNode);
            }
        }

        private void pgBiomes_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "Name")
            {
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            //Clear all the Biomes node items
            tvBiomeList.Nodes.Clear();

            for (var i = 0; i < Configuration.RealmBiomes.Count; i++)
            {
                var biome = Configuration.RealmBiomes[i];
                var item = new TreeNode(biome.Name);
                item.Tag = biome;
                tvBiomeList.Nodes.Add(item);
            }
        }


    }
}
