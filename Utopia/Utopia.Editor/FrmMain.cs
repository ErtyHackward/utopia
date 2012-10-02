using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Utopia.Shared;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Editor
{
    public partial class FrmMain : Form
    {
        #region Private Variables
        private string _filePath;
        private int _entitiesOffset;
        private RealmConfiguration _configuration;
        private FrmUtopiaConfig _utopiaConfig;
        #endregion

        #region Public Properties
        public RealmConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;

                if (_configuration != null)
                {
                    Text = _configuration.RealmName + " - Utopia realm editor";
                    saveToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    tvMainCategories.Enabled = true;
                }
                else
                {
                    Text = "Utopia realm editor";
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    tvMainCategories.Enabled = false;
                }
                UpdateList();
            }
        }
        #endregion

        public FrmMain()
        {
            InitializeComponent();

            _utopiaConfig = new FrmUtopiaConfig();
            _utopiaConfig.Visible = false;
            splitContainer1.Panel2.Controls.Add(_utopiaConfig);

            _entitiesOffset = imageList1.Images.Count;

            foreach (var file in Program.ModelsRepository.ModelsFiles)
            {
                imageList1.Images.Add(Image.FromFile(file));
            }

            tvMainCategories.ImageList = imageList1;

            if (Program.ModelsRepository.ModelsFiles.Count == 0)
            {
                MessageBox.Show(
                    "Unable to find models images. Use 'export all' feature of the model editor to create them. And restart the program",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        #region Public Methods
        #endregion

        #region Private Methods

        #region Menu related
        
        //Events from FILE ===========================

        //New
        private void newRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration = new RealmConfiguration(withDefaultValueCreation: true) { RealmName = "noname", CreatedAt = DateTime.Now };
        }

        //Open
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

        //Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                saveAsToolStripMenuItem_Click(null, null);
                return;
            }

            Save(_filePath);
        }

        //SaveAs
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = Configuration.RealmName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Save(saveFileDialog1.FileName);
            }
        }

        // ===========================================

        //Events from HELP ===========================

        //Official Site
        private void officialSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://utopiarealms.com");
        }

        //About...
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Utopia Realms configuration editor. v" + Assembly.GetExecutingAssembly().GetName().Version);
        }
        // ===========================================

        
        #endregion

        //Create the various SubNode (Items, Cubes, ...) in the TreeView
        private void UpdateList()
        {
            if (_configuration == null)
                return;

            //Bind Configuration object to General root Node
            tvMainCategories.Nodes["General"].Tag = _configuration;

            //Get Entities Root node collection
            TreeNode entitiesRootNode = tvMainCategories.Nodes["Entities"];
            entitiesRootNode.Nodes.Clear();

            //Add new Entities nodes
            for (var i = 0; i < _configuration.RealmEntities.Count; i++)
            {
                var entity = _configuration.RealmEntities[i];
                var item = new TreeNode(entity.DisplayName);
                item.Tag = entity;

                if (entity is IVoxelEntity)
                {
                    var voxelEntity = entity as IVoxelEntity;
                    item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1 : _entitiesOffset + i;
                    item.SelectedImageIndex = item.ImageIndex;
                }
                entitiesRootNode.Nodes.Add(item);
            }

            //Clear all the Cube node items
            TreeNode cubesRootNode = tvMainCategories.Nodes["Cubes"];
            cubesRootNode.Nodes.Clear();
            for (var i = 0; i < _configuration.RealmCubeProfiles.Count; i++)
            {
                var cubeProfile = _configuration.RealmCubeProfiles[i];
                if (cubeProfile.Name == "System Reserved") continue;
                var item = new TreeNode(cubeProfile.Name);
                item.Tag = cubeProfile;
                cubesRootNode.Nodes.Add(item);
            }

            //Clear all the Biomes node items
            TreeNode biomesRootNode = tvMainCategories.Nodes["Biomes"];
            biomesRootNode.Nodes.Clear();

            for (var i = 0; i < _configuration.RealmBiomes.Count; i++)
            {
                var biome = _configuration.RealmBiomes[i];
                var item = new TreeNode(biome.Name);
                item.Tag = biome;
                biomesRootNode.Nodes.Add(item);
            }

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

        private void tvMainCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvMainCategories.SelectedNode.Name == "Landscape Generator")
            {
                pgDetails.Visible = false;
                _utopiaConfig.Visible = true;
            }
            else
            {
                _utopiaConfig.Visible = false;
                pgDetails.Visible = true;
                if (tvMainCategories.SelectedNode.Tag is CubeProfile)
                {
                    if (((CubeProfile)tvMainCategories.SelectedNode.Tag).CanBeModified == false) pgDetails.Enabled = false;
                    else pgDetails.Enabled = true;
                }
                else
                {
                    pgDetails.Enabled = true;
                }
                pgDetails.SelectedObject = tvMainCategories.SelectedNode.Tag;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
        }

        //Called when the ADD button is cliques on a Main Category treeview
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (tvMainCategories.SelectedNode.Name)
            {
                case "Entities":
                        var form = new FrmEntityChoose();
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            var entityInstance = Configuration.CreateNewEntity(form.SelectedType);
                            UpdateList();
                            tvMainCategories.SelectedNode = FindByTag(entityInstance);
                        }
                    break;
                case "Cubes":
                    var cubeInstance = Configuration.CreateNewCube();
                    UpdateList();
                    tvMainCategories.SelectedNode = FindByTag(cubeInstance);
                    break;

                default:
                    break;
            }
        }

        private TreeNode FindByTag(object tag, TreeNode node = null)
        {
            TreeNodeCollection nodes;
            if (node == null)
                nodes = tvMainCategories.Nodes;
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        private void pgDetails_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "ModelName")
            {
                var item = tvMainCategories.SelectedNode;

                var entity = pgDetails.SelectedObject;

                var voxelEntity = entity as IVoxelEntity;
                item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1 : _entitiesOffset + Program.ModelsRepository.ModelsFiles.FindIndex(i => Path.GetFileNameWithoutExtension(i) == voxelEntity.ModelName);
                item.SelectedImageIndex = item.ImageIndex;
            }

            if (e.ChangedItem.Label == "Name")
            {
                UpdateList();
            }
        }        
    }
}
