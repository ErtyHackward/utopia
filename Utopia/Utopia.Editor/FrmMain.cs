using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Utopia.Shared;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Editor
{
    public partial class FrmMain : Form
    {
        private string _filePath;

        private RealmConfiguration _configuration;
        public RealmConfiguration Configuration
        {
            get { return _configuration; }
            set { 
                _configuration = value;

                if (_configuration != null)
                {
                    Text = _configuration.RealmName + " - Utopia realm editor";

                    saveToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    buttonAdd.Enabled = true;
                    buttonRemove.Enabled = true;
                }
                else
                {
                    Text = "Utopia realm editor";

                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    buttonAdd.Enabled = false;
                    buttonRemove.Enabled = false;
                }
                UpdateList();
            }
        }

        public FrmMain()
        {
            InitializeComponent();
        }

        private void UpdateList()
        {
            listView1.Items.Clear();

            if (_configuration == null)
                return;

            {
                var item = new ListViewItem("General");
                item.Tag = _configuration;
                listView1.Items.Add(item);
            }

            foreach (var entity in _configuration.EntityExamples)
            {
                var item = new ListViewItem(entity.DisplayName);
                item.Tag = entity;
                listView1.Items.Add(item);
            }

        }

        private void officialSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://utopiarealms.com");
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Utopia Realms configuration editor. v" + Assembly.GetExecutingAssembly().GetName().Version);
        }

        private void newRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration = new RealmConfiguration {RealmName = "noname", CreatedAt = DateTime.Now};
        }

        private void openRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Configuration = RealmConfiguration.LoadFromFile(openFileDialog1.FileName);
                    _filePath = openFileDialog1.FileName;
                }
                catch (Exception x)
                {
                    MessageBox.Show("Error: " + x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = Configuration.RealmName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Save(saveFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                saveAsToolStripMenuItem_Click(null, null);
                return;
            }

            Save(_filePath);
        }

        private void Save(string filePath)
        {
            try
            {
                Configuration.UpdatedAt = DateTime.Now;
                Configuration.SaveToFile(filePath);
                _filePath = filePath;
            }
            catch (Exception x)
            {
                MessageBox.Show("Error: " + x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var selectedItem = listView1.SelectedItems[0];
                propertyGrid1.SelectedObject = selectedItem.Tag;
            }
            else
            {
                propertyGrid1.SelectedObject = null;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var form = new FrmEntityChoose();

            if (form.ShowDialog() == DialogResult.OK)
            {
                var type = form.SelectedType;

                var instance = (IEntity)Activator.CreateInstance(type);

                Configuration.EntityExamples.Add(instance);

                UpdateList();
            }
        }
    }
}
