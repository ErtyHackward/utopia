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
    public partial class FrmUtopiaConfig : UserControl
    {
        public FrmUtopiaConfig()
        {
            InitializeComponent();
            worldType.SelectedIndex = 0;
        }

        public FrmUtopiaConfig(UtopiaProcessorParams param)
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
    }
}
