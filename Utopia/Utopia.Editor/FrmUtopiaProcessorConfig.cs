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
using Utopia.Shared.World.Processors.Utopia.LandformFct;

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

            worldType.Items.Clear();

            foreach (string worldtype in Enum.GetNames(typeof(enuWorldType)))
            {
                worldType.Items.Add(worldtype);
            }

            worldType.SelectedIndex = 0;

            worldType.SelectedIndexChanged += worldType_SelectedIndexChanged;
        }

        private void worldType_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshValueWorldTypeValue();
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

            foreach (string value in worldType.Items)
            {
                if (value == param.WorldType.ToString())
                {
                    worldType.SelectedItem = value;
                }
            }

            maxHeight.Value = param.WorldHeight;
            trackBar2.Value = param.WaterLevel;

            udPlainFreq.Value = (decimal)param.PlainCtrlFrequency;
            udPlainOct.Value = (decimal)param.PlainCtrlOctave;

            udGroundFeq.Value = (decimal)param.GroundCtrlFrequency;
            udGroundOct.Value = (decimal)param.GroundCtrlOctave;

            udIslandSize.Value = (decimal)param.IslandCtrlSize;

            udContinentFreq.Value = (decimal)param.WorldCtrlFrequency;
            udContinentOct.Value = (decimal)param.WorldCtrlOctave;

            this.maxHeight_ValueChanged(this, null);
            this.trackBar2_ValueChanged(this, null);
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

        private void worldType_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (worldType.SelectedItem.ToString() == "Island")
            {
                pIsland.Visible = true;
                pContinent.Visible = false;
            }
            else
            {
                pIsland.Visible = false;
                pContinent.Visible = true;
            }
        }

        private void RefreshValueWorldTypeValue()
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.WorldType = (enuWorldType)Enum.Parse(typeof(enuWorldType), worldType.SelectedItem.ToString());
        }

        private void maxHeight_ValueChanged(object sender, EventArgs e)
        {
            wHeight.Text = maxHeight.Value.ToString();
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.WorldHeight = maxHeight.Value;
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            OceanHeight.Text = trackBar2.Value.ToString();
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.WaterLevel = trackBar2.Value;
        }

        private void udPlainFreq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.PlainCtrlFrequency = (double)udPlainFreq.Value;
        }

        private void udPlainOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.PlainCtrlOctave = (int)udPlainOct.Value;
        }

        private void udGroundFeq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.GroundCtrlFrequency = (double)udGroundFeq.Value;
        }

        private void udGroundOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.GroundCtrlOctave = (int)udGroundOct.Value;
        }

        private void udContinentFreq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.WorldCtrlFrequency = (double)udContinentFreq.Value;
        }

        private void udContinentOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.WorldCtrlOctave = (int)udContinentOct.Value;
        }

        private void udIslandSize_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.UtopiaProcessorParam.IslandCtrlSize = (double)udIslandSize.Value;
        }

    }
}
