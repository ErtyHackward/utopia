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
        public UtopiaWorldConfiguration Configuration { get; set; }

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

        public FrmUtopiaProcessorConfig(UtopiaWorldConfiguration param)
            :this()
        {
            Configuration = param;
            LoadConfigParam(param);
        }

        //Clear all the Biomes node items

        private void LoadConfigParam(UtopiaWorldConfiguration param)
        {
            tvBiomeList.Nodes.Clear();

            for (var i = 0; i < Configuration.ProcessorParam.Biomes.Count; i++)
            {
                var biome = Configuration.ProcessorParam.Biomes[i];
                var item = new TreeNode(biome.Name);
                item.Tag = biome;
                tvBiomeList.Nodes.Add(item);
            }

            rangeBarBasicPlain.Ranges.Clear();
            foreach (var item in param.ProcessorParam.BasicPlain)
            {
                rangeBarBasicPlain.Ranges.Add(item);
            }

            rangeBarBasicMidLand.Ranges.Clear();
            foreach (var item in param.ProcessorParam.BasicMidland)
            {
                rangeBarBasicMidLand.Ranges.Add(item);
            }

            rangeBarBasicMontain.Ranges.Clear();
            foreach (var item in param.ProcessorParam.BasicMontain)
            {
                rangeBarBasicMontain.Ranges.Add(item);
            }

            rangeBarBasicOcean.Ranges.Clear();
            foreach (var item in param.ProcessorParam.BasicOcean)
            {
                rangeBarBasicOcean.Ranges.Add(item);
            }

            rangeBarGround.Ranges.Clear();
            foreach (var item in param.ProcessorParam.Ground)
            {
                rangeBarGround.Ranges.Add(item);
            }

            rangeBarOcean.Ranges.Clear();
            foreach (var item in param.ProcessorParam.Ocean)
            {
                rangeBarOcean.Ranges.Add(item);
            }

            rangeBarWorld.Ranges.Clear();
            foreach (var item in param.ProcessorParam.World)
            {
                rangeBarWorld.Ranges.Add(item);
            }

            foreach (string value in worldType.Items)
            {
                if (value == param.ProcessorParam.WorldType.ToString())
                {
                    worldType.SelectedItem = value;
                }
            }

            maxHeight.Value = param.ProcessorParam.WorldGeneratedHeight;
            trackBar2.Value = param.ProcessorParam.WaterLevel;

            udPlainFreq.Value = (decimal)param.ProcessorParam.PlainCtrlFrequency;
            udPlainOct.Value = (decimal)param.ProcessorParam.PlainCtrlOctave;

            udGroundFeq.Value = (decimal)param.ProcessorParam.GroundCtrlFrequency;
            udGroundOct.Value = (decimal)param.ProcessorParam.GroundCtrlOctave;

            udIslandSize.Value = (decimal)param.ProcessorParam.IslandCtrlSize;

            udContinentFreq.Value = (decimal)param.ProcessorParam.WorldCtrlFrequency;
            udContinentOct.Value = (decimal)param.ProcessorParam.WorldCtrlOctave;

            udOctTemp.Value = (decimal)param.ProcessorParam.TempCtrlOctave;
            udFreqTemp.Value = (decimal)param.ProcessorParam.TempCtrlFrequency;

            udOctMoist.Value = (decimal)param.ProcessorParam.MoistureCtrlOctave;
            udFreqMoist.Value = (decimal)param.ProcessorParam.MoistureCtrlFrequency;

            udFreqZone.Value = param.ProcessorParam.ZoneCtrlFrequency > 0 ? (decimal)param.ProcessorParam.ZoneCtrlFrequency : (decimal)0.01;

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
            Biome biome = Configuration.ProcessorParam.CreateNewBiome();

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
                Configuration.ProcessorParam.Biomes.Remove(biome);
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

            for (var i = 0; i < Configuration.ProcessorParam.Biomes.Count; i++)
            {
                var biome = Configuration.ProcessorParam.Biomes[i];
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
            Configuration.ProcessorParam.WorldType = (enuWorldType)Enum.Parse(typeof(enuWorldType), worldType.SelectedItem.ToString());
        }

        private void maxHeight_ValueChanged(object sender, EventArgs e)
        {
            maxHeight.Value = (int)Math.Round((maxHeight.Value / (double)16)) * 16;

            wHeight.Text = maxHeight.Value.ToString();
            if (Configuration == null) return;

            if (maxHeight.Value > Configuration.WorldHeight) maxHeight.Value = Configuration.WorldHeight;

            Configuration.ProcessorParam.WorldGeneratedHeight = maxHeight.Value;
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            OceanHeight.Text = trackBar2.Value.ToString();
            if (Configuration == null) return;
            Configuration.ProcessorParam.WaterLevel = trackBar2.Value;
        }

        private void udPlainFreq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.PlainCtrlFrequency = (double)udPlainFreq.Value;
        }

        private void udPlainOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.PlainCtrlOctave = (int)udPlainOct.Value;
        }

        private void udGroundFeq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.GroundCtrlFrequency = (double)udGroundFeq.Value;
        }

        private void udGroundOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.GroundCtrlOctave = (int)udGroundOct.Value;
        }

        private void udContinentFreq_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.WorldCtrlFrequency = (double)udContinentFreq.Value;
        }

        private void udContinentOct_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.WorldCtrlOctave = (int)udContinentOct.Value;
        }

        private void udIslandSize_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.IslandCtrlSize = (double)udIslandSize.Value;
        }

        private void udFreqTemp_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.TempCtrlFrequency = (double)udFreqTemp.Value;
        }

        private void udOctTemp_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.TempCtrlOctave = (int)udOctTemp.Value;
        }

        private void udFreqMoist_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.MoistureCtrlFrequency = (double)udFreqMoist.Value;
        }

        private void udOctMoist_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.MoistureCtrlOctave = (int)udOctMoist.Value;
        }

        private void udFreqZone_ValueChanged(object sender, EventArgs e)
        {
            if (Configuration == null) return;
            Configuration.ProcessorParam.ZoneCtrlFrequency = (double)udFreqZone.Value;
        }
    }
}
