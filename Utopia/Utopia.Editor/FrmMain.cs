using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Utopia.Shared;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Editor
{
    public partial class FrmMain : Form
    {
        private string _filePath;

        private int _entitiesOffset;

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
                    treeView1.Enabled = true;
                }
                else
                {
                    Text = "Utopia realm editor";

                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    treeView1.Enabled = false;
                }
                UpdateList();
            }
        }

        public FrmMain()
        {
            InitializeComponent();

            _entitiesOffset = imageList1.Images.Count;

            foreach (var file in Program.ModelsRepository.ModelsFiles)
            {
                imageList1.Images.Add(Image.FromFile(file));
            }

            treeView1.ImageList = imageList1;

            if (Program.ModelsRepository.ModelsFiles.Count == 0)
            {
                MessageBox.Show(
                    "Unable to find models images. Use 'export all' feature of the model editor to create them. And restart the program",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void UpdateList()
        {
            //treeView1.Nodes.Clear();

            if (_configuration == null)
                return;

            treeView1.Nodes["General"].Tag = _configuration;

            var entitiesNode = treeView1.Nodes["Entities"];

            entitiesNode.Nodes.Clear();

            for (var i = 0; i < _configuration.EntityExamples.Count; i++)
            {
                var entity = _configuration.EntityExamples[i];
                var item = new TreeNode(entity.DisplayName);
                item.Tag = entity;

                if (entity is IVoxelEntity)
                {
                    var voxelEntity = entity as IVoxelEntity;
                    item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1: _entitiesOffset + i;
                    item.SelectedImageIndex = item.ImageIndex;
                }
                entitiesNode.Nodes.Add(item);
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

                

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = treeView1.SelectedNode.Tag;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new FrmEntityChoose();

            if (form.ShowDialog() == DialogResult.OK)
            {
                var type = form.SelectedType;

                var instance = (IEntity)Activator.CreateInstance(type);

                Configuration.EntityExamples.Add(instance);

                UpdateList();

                treeView1.SelectedNode = FindByTag(instance);
            }
            
        }

        private TreeNode FindByTag(object tag, TreeNode node = null)
        {
            TreeNodeCollection nodes;
            if (node == null)
                nodes = treeView1.Nodes;
            else
                nodes = node.Nodes;

            foreach (TreeNode node1 in nodes)
            {
                if (node1.Tag == tag)
                    return node1;
            }

            foreach (TreeNode node1 in nodes)
            {
                var result = FindByTag(tag, node1);

                if (result != null)
                    return result;
            }

            return null;
        }


        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "ModelName")
            {
                var item = treeView1.SelectedNode;

                var entity = propertyGrid1.SelectedObject;
                
                var voxelEntity = entity as IVoxelEntity;
                item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1 : _entitiesOffset + Program.ModelsRepository.ModelsFiles.FindIndex(i => Path.GetFileNameWithoutExtension(i) == voxelEntity.ModelName);
                item.SelectedImageIndex = item.ImageIndex;
            }

            if (e.ChangedItem.Label == "UniqueName")
            {
                var item = treeView1.SelectedNode;

                item.Text = (string)e.ChangedItem.Value;
            }
        }
    }
}
