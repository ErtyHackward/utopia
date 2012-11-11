using System;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Editor.Forms
{
    public partial class FrmItemAdd : Form
    {
        private WorldConfiguration _configuration;

        public WorldConfiguration Configuration
        {
            get { return _configuration; }
            set { 
                _configuration = value;

                cbEntityType.Items.Clear();

                // add cubes
                foreach (var profile in _configuration.GetAllCubesProfiles())
                {
                    cbEntityType.Items.Add(profile);
                }

                // add entities
                foreach (var pair in _configuration.BluePrints)
                {
                    if (pair.Value is IItem)
                    {
                        cbEntityType.Items.Add(pair.Value);
                    }
                }
            }
        }

        public ushort SelectedId
        {
            get 
            {
                var obj = cbEntityType.SelectedItem;

                if (obj is CubeProfile)
                {
                    var profile = (CubeProfile)obj;
                    return profile.Id;
                }

                if (obj is IItem)
                {
                    var item = (IItem)obj;
                    return item.BluePrintId;
                }

                throw new ApplicationException("Value is not supported");
            }
        }

        public int ItemsCount
        {
            get { return (int)numCount.Value; }
            set { numCount.Value = value; }
        }

        public FrmItemAdd()
        {
            InitializeComponent();
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            if (cbEntityType.SelectedIndex == -1)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
