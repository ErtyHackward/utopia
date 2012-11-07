using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;
using System.Linq;
using Utopia.Entities.Voxel;
using Utopia.Entities;
using Utopia.Editor.Properties;

namespace Utopia.Editor
{
    public partial class FrmMain : Form
    {
        private string _filePath;
        private int _entitiesOffset;
        private int _cubeOffset;
        private WorldConfiguration _configuration;
        private UserControl _processorControl;
        private Dictionary<string, Image> _icons;

        #region Public Properties
        public WorldConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;

                if (_configuration != null)
                {
                    Text = _configuration.ConfigurationName + " with processor : " + _configuration.WorldProcessor + " - realm editor";
                    saveToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    tvMainCategories.Enabled = true;

                    //Create NEW processor control windows following the WorldConfiguration processor type

                    switch (_configuration.WorldProcessor)
                    {
                        case WorldConfiguration.WorldProcessors.Flat:
                            break;
                        case WorldConfiguration.WorldProcessors.Utopia:
                            if (_processorControl != null) _processorControl.Dispose();
                            AttachProcessorFrame(new FrmUtopiaProcessorConfig(value as WorldConfiguration<UtopiaProcessorParams>));
                            break;
                        case WorldConfiguration.WorldProcessors.Plan:
                            break;
                    }

                    // generate icons for the configuration
                    _icons = Program.IconManager.GenerateIcons(_configuration);

                    UpdateImageList();
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

        private void UpdateImageList()
        {
            //Removes all image above _entitiesOffset
            while (imageList1.Images.Count > _entitiesOffset) imageList1.Images.RemoveAt(imageList1.Images.Count - 1);

            //Create Items icons
            foreach (var visualVoxelModel in Program.IconManager.ModelManager.Enumerate())
            {
                imageList1.Images.Add(_icons[visualVoxelModel.VoxelModel.Name]);
                _icons[visualVoxelModel.VoxelModel.Name].Tag = imageList1.Images.Count - 1; //Add Image Index in imageList
            }

            _cubeOffset = imageList1.Images.Count;
            //Create blocks icons
            foreach (var cubeprofiles in _configuration.GettAllCubesProfiles())
            {
                if (cubeprofiles.Id == WorldConfiguration.CubeId.Air) continue;
                imageList1.Images.Add(_icons["CubeResource_" + cubeprofiles.Name]);
                _icons["CubeResource_" + cubeprofiles.Name].Tag = imageList1.Images.Count - 1;
            }

            tvMainCategories.Refresh();
        }
        #endregion

        public FrmMain()
        {
            InitializeComponent();

            _entitiesOffset = imageList1.Images.Count;

            tvMainCategories.ImageList = imageList1;
        }

        #region Public Methods
        #endregion

        #region Private Methods

        #region Menu related

        //Events from FILE ===========================

        //New
        private void newRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmProcessorChoose processorChoose = new frmProcessorChoose();
            processorChoose.ShowDialog(this);

            string processorType = "Utopia.Shared.Configuration." + processorChoose.SelectedProcessor.ToString() + "ProcessorParams, Utopia.Shared";

            //Create new instance of Worldconfiguration dynamicaly, following Processor type !            
            Type type = typeof(WorldConfiguration<>).MakeGenericType(Type.GetType(processorType));
            WorldConfiguration newConfiguration = (WorldConfiguration)Activator.CreateInstance(type, null, true, false);

            newConfiguration.ConfigurationName = "noname";
            newConfiguration.CreatedAt = DateTime.Now;
            newConfiguration.WorldProcessor = processorChoose.SelectedProcessor;

            processorChoose.Dispose();

            Configuration = newConfiguration;
        }

        //Open
        private void openRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Configuration = WorldConfiguration.LoadFromFile(openFileDialog1.FileName, withHelperAssignation: true);
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
            saveFileDialog1.FileName = Configuration.ConfigurationName;
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

        private void AttachProcessorFrame(UserControl control)
        {
            if (_processorControl != null)
            {
                splitContainer1.Panel2.Controls.Remove(_processorControl);
                _processorControl.Dispose();
            }
            _processorControl = control;
            _processorControl.Visible = false;
            splitContainer1.Panel2.Controls.Add(control);
            _processorControl.Dock = DockStyle.Fill;
        }

        //Create the various SubNode (Items, Cubes, ...) in the TreeView
        private void UpdateList()
        {
            if (_configuration == null)
                return;
            Image result;

            //Bind Configuration object to General root Node
            tvMainCategories.Nodes["General"].Tag = _configuration;

            //Get Entities Root node collection
            TreeNode entitiesRootNode = tvMainCategories.Nodes["Entities"];
            entitiesRootNode.Nodes.Clear();

            //Add new Entities nodes
            for (var i = 0; i < _configuration.Entities.Count; i++)
            {
                var entity = _configuration.Entities[i];
                var item = new TreeNode(entity.Name);
                item.Tag = entity;

                if (entity is IVoxelEntity)
                {
                    var voxelEntity = entity as IVoxelEntity;

                    item.ImageIndex = _icons.TryGetValue(voxelEntity.ModelName, out result) ? (int)result.Tag : -1;

                    item.SelectedImageIndex = item.ImageIndex;
                }
                entitiesRootNode.Nodes.Add(item);
            }

            //Clear all the Cube node items
            TreeNode cubesRootNode = tvMainCategories.Nodes["Cubes"];
            cubesRootNode.Nodes.Clear();
            int iconcubeImageId = _cubeOffset;
            for (var i = 0; i < _configuration.CubeProfiles.Where(x => x != null).Count(); i++)
            {
                var cubeProfile = _configuration.CubeProfiles[i];
                if (cubeProfile.Name == "System Reserved") continue;
                var item = new TreeNode(cubeProfile.Name);

                item.ImageIndex = _icons.TryGetValue("CubeResource_" + cubeProfile.Name, out result) ? (int)result.Tag : -1;

                item.SelectedImageIndex = item.ImageIndex;
                
                item.Tag = cubeProfile;
                cubesRootNode.Nodes.Add(item);
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

        #region GUI events Handling
        //Called when the ADD button is cliques on a Main Category treeview
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvMainCategories.SelectedNode == null) return;
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

        /// <summary>
        /// Event raised when a property change in the Details property grid.
        /// </summary>
        private void pgDetails_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //Update ListBox Icon when the modelName is changing
            if (e.ChangedItem.Label == "ModelName")
            {
                //Get the currently changed treeNode
                TreeNode item = tvMainCategories.SelectedNode;

                //Get the underlying entity
                var entity = pgDetails.SelectedObject;

                //Check if this entity can have a Voxel body
                var voxelEntity = entity as IVoxelEntity;

                //Look at currently existing Voxel Models following Entity Name
                item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1 : _entitiesOffset + Program.ModelsRepository.ModelsFiles.FindIndex(i => Path.GetFileNameWithoutExtension(i) == voxelEntity.ModelName);
                item.SelectedImageIndex = item.ImageIndex;
            }

            if (e.ChangedItem.Label == "Name")
            {
                UpdateList();
            }
        }

        private void tvMainCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvMainCategories.SelectedNode.Name == "WorldProcessor Params")
            {
                //Display Utopia World Processor
                pgDetails.Visible = false;
                _processorControl.Visible = true;
            }
            else
            {
                _processorControl.Visible = false;
                pgDetails.Visible = true;
                if (tvMainCategories.SelectedNode.Tag is CubeProfile)
                {
                    if (((CubeProfile)tvMainCategories.SelectedNode.Tag).IsSystemCube == true) pgDetails.Enabled = false;
                    else pgDetails.Enabled = true;
                }
                else
                {
                    pgDetails.Enabled = true;
                }
                if ((ModifierKeys & Keys.Control) != 0) pgDetails.Enabled = true;
                pgDetails.SelectedObject = tvMainCategories.SelectedNode.Tag;
            }

        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #endregion
    }
}
