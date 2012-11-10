using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;

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
            get { return ((IItem)cbEntityType.SelectedItem).BluePrintId; }
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

        private void ButtonAddClick(object sender, System.EventArgs e)
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
