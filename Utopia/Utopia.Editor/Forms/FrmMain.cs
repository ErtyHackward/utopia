using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;
using System.Linq;
using Utopia.Shared.Tools;

namespace Utopia.Editor.Forms
{
    public partial class FrmMain : Form
    {
        private string _filePath;
        private int _entitiesOffset;
        private int _cubeOffset;
        private WorldConfiguration _configuration;
        private UserControl _processorControl;
        private Dictionary<string, Image> _icons;
        private PluralizationService _pluralization;

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
                            AttachProcessorFrame(new FrmUtopiaProcessorConfig(value as UtopiaWorldConfiguration));
                            break;
                    }

                    // generate icons for the configuration
                    _icons = Program.IconManager.GenerateIcons(_configuration);

                    containerEditor.Configuration = _configuration;
                    containerEditor.Icons = _icons;
                    ContainerSetSelector.Configuration = _configuration;
                    BlueprintSelector.Configuration = _configuration;

                    UpdateImageList();

                    

                }
                else
                {
                    Text = "Utopia realm editor";
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    tvMainCategories.Enabled = false;
                }

                UpdateTree();
            }
        }

        private void UpdateImageList()
        {
            //Removes all image above _entitiesOffset
            while (imageList1.Images.Count > _entitiesOffset) 
                imageList1.Images.RemoveAt(imageList1.Images.Count - 1);

            largeImageList.Images.Clear();

            ModelSelector.Models.Clear();

            //Create Items icons
            foreach (var visualVoxelModel in Program.IconManager.ModelManager.Enumerate())
            {
                imageList1.Images.Add(_icons[visualVoxelModel.VoxelModel.Name]);
                largeImageList.Images.Add(visualVoxelModel.VoxelModel.Name, _icons[visualVoxelModel.VoxelModel.Name]);

                _icons[visualVoxelModel.VoxelModel.Name].Tag = imageList1.Images.Count - 1; //Add Image Index in imageList

                ModelSelector.Models.Add(visualVoxelModel.VoxelModel.Name, _icons[visualVoxelModel.VoxelModel.Name]);
            }

            _cubeOffset = imageList1.Images.Count;
            //Create blocks icons
            foreach (var cubeprofiles in _configuration.GetAllCubesProfiles())
            {
                if (cubeprofiles.Id == WorldConfiguration.CubeId.Air) continue;
                imageList1.Images.Add(_icons["CubeResource_" + cubeprofiles.Name]);
                largeImageList.Images.Add("CubeResource_" + cubeprofiles.Name, _icons["CubeResource_" + cubeprofiles.Name]);
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

            _pluralization = PluralizationService.CreateService(
                new CultureInfo("en"));
            
        }

        #region Private Methods

        #region Menu related

        //Events from FILE ===========================

        //New
        private void newRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmProcessorChoose processorChoose = new FrmProcessorChoose();

            if (processorChoose.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            WorldConfiguration newConfiguration = null;

            switch (processorChoose.SelectedProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    newConfiguration = new FlatWorldConfiguration();
                    ((FlatWorldConfiguration)newConfiguration).ProcessorParam.CreateDefaultValues();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    newConfiguration = new UtopiaWorldConfiguration();
                    ((UtopiaWorldConfiguration)newConfiguration).ProcessorParam.CreateDefaultValues();
                    break;
                default:
                    break;
            }

            newConfiguration.InjectMandatoryObjects();
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
        private void UpdateTree()
        {
            if (_configuration == null)
                return;

            tvMainCategories.BeginUpdate();

            //Bind Configuration object to General root Node
            tvMainCategories.Nodes["General"].Tag = _configuration;

            //Get Entities Root node collection
            var entitiesRootNode = tvMainCategories.Nodes["Entities"];
            entitiesRootNode.Nodes.Clear();

            //Add new Entities nodes

            // get entities types
            var entitiesTypes = _configuration.BluePrints.Values.Select(e => e.GetType()).Distinct().ToArray();

            // add categories

            foreach (var entitiesType in entitiesTypes)
            {
                var categoryNode = AddSubNode(entitiesRootNode, _pluralization.Pluralize(entitiesType.Name), entitiesType);

                categoryNode.ImageIndex = 9;
                categoryNode.SelectedImageIndex = 10;

                foreach (var entity in _configuration.BluePrints.Values.Where(e => e.GetType() == entitiesType))
                {
                    string iconName = null;

                    if (entity is IVoxelEntity)
                    {
                        var voxelEntity = entity as IVoxelEntity;
                        iconName = voxelEntity.ModelName;
                    }

                    var node = AddSubNode(categoryNode, entity.Name, entity, iconName);

                    node.ContextMenuStrip = contextMenuEntity;
                }

            }



            foreach (var pair in _configuration.BluePrints)
            {

            }


            //Clear all the Cube node items
            var cubesRootNode = tvMainCategories.Nodes["Cubes"];
            cubesRootNode.Nodes.Clear();
            for (var i = 0; i < _configuration.BlockProfiles.Where(x => x != null).Count(); i++)
            {
                var blockProfile = _configuration.BlockProfiles[i];
                if (blockProfile.Name == "System Reserved") continue;

                AddSubNode(cubesRootNode, blockProfile.Name, blockProfile, "CubeResource_" + blockProfile.Name);
            }

            #region Sets

            var setsRootNode = tvMainCategories.Nodes["Container sets"];
            setsRootNode.Nodes.Clear();

            foreach (var containerSet in _configuration.ContainerSets)
            {
                AddSubNode(setsRootNode, containerSet.Key, containerSet.Value);
            }

            #endregion

            #region Recipes

            var recipesNode = tvMainCategories.Nodes["Recipes"];
            recipesNode.Nodes.Clear();

            foreach (var recipe in _configuration.Recipes)
            {
                string iconName = null;
                if (recipe.ResultBlueprintId != 0)
                {
                    var entity = _configuration.BluePrints[recipe.ResultBlueprintId];

                    if (entity is IVoxelEntity)
                    {
                        var voxelEntity = entity as IVoxelEntity;
                        iconName = voxelEntity.ModelName;
                    }

                }

                AddSubNode(recipesNode, recipe.Name, recipe, iconName);
            }

            #endregion

            tvMainCategories.EndUpdate();
        }

        private TreeNode AddSubNode(TreeNode parentNode, string label, object tag, string iconName = null)
        {
            var imgIndex = -1;

            if (!string.IsNullOrEmpty(iconName))
            {
                Image img;
                if (_icons.TryGetValue(iconName, out img))
                {
                    imgIndex = (int)img.Tag;
                }
            }

            var item = new TreeNode(label)
            {
                Tag = tag,
                SelectedImageIndex = imgIndex,
                ImageIndex = imgIndex
            };

            parentNode.Nodes.Add(item);

            return item;
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
                        UpdateTree();
                        tvMainCategories.SelectedNode = FindByTag(entityInstance);
                    }
                    break;
                case "Cubes":
                    var cubeInstance = Configuration.CreateNewCube();
                    UpdateTree();
                    tvMainCategories.SelectedNode = FindByTag(cubeInstance);
                    break;
                case "Container sets":
                    var newValue = new SlotContainer<BlueprintSlot>();
                    _configuration.ContainerSets.Add(GetFreeName("Set", _configuration.ContainerSets), newValue);
                    UpdateTree();
                    tvMainCategories.SelectedNode = FindByTag(newValue);
                    break;
                case "Recipes":
                    var recipe = new Recipe { Name = "noname" };
                    _configuration.Recipes.Add(recipe);
                    UpdateTree();
                    tvMainCategories.SelectedNode = FindByTag(recipe);
                    break;
                default:
                    break;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var entity = (Entity)tvMainCategories.SelectedNode.Tag;
            _configuration.BluePrints.Remove(entity.BluePrintId);
            tvMainCategories.SelectedNode.Remove();
        }

        /// <summary>
        /// Event raised when a property change in the Details property grid.
        /// </summary>
        private void pgDetails_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
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
                item.ImageIndex = string.IsNullOrEmpty(voxelEntity.ModelName) ? -1 : (int)_icons[voxelEntity.ModelName].Tag;
                item.SelectedImageIndex = item.ImageIndex;

                ModelStateSelector.PossibleValues = null;

                if (voxelEntity.ModelName != null)
                {

                    var model = Program.IconManager.ModelManager.GetModel(voxelEntity.ModelName);

                    if (model != null)
                    {
                        ModelStateSelector.PossibleValues =
                            model.VoxelModel.States.Select(s => s.Name).ToArray();
                    }
                }
            }

            if (e.ChangedItem.Label == "Name")
            {
                UpdateTree();
            }
        }

        private void tvMainCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvMainCategories.SelectedNode.Name == "WorldProcessor Params")
            {
                if (_processorControl != null)
                {
                    ShowMainControl(_processorControl);
                }
            }
            else
            {
                var selectedObject = tvMainCategories.SelectedNode.Tag;

                if (selectedObject is SlotContainer<BlueprintSlot>)
                {
                    var content = selectedObject as SlotContainer<BlueprintSlot>;

                    ShowMainControl(containerEditor);

                    containerEditor.Content = content;
                }
                else if (selectedObject is Type)
                {
                    ShowMainControl(entityListView);

                    // show entities in that category
                    entityListView.Groups.Clear();
                    entityListView.Items.Clear();
                    foreach (var entity in _configuration.BluePrints.Values.Where(en => en.GetType() == (Type)selectedObject))
                    {
                        string imgKey = null;

                        if (entity is IVoxelEntity)
                        {
                            imgKey = (entity as IVoxelEntity).ModelName;
                        }

                        var lvi = new ListViewItem { 
                            Text = entity.Name,
                            ImageKey = imgKey,
                            Tag = entity
                        };

                        entityListView.Items.Add(lvi);
                    }

                }
                else if (selectedObject == null && tvMainCategories.SelectedNode.Name == "Entities")
                {
                    // show all entities, grouped

                    ShowMainControl(entityListView);
                    entityListView.Groups.Clear();
                    entityListView.Items.Clear();

                    // get entities types
                    var entitiesTypes = _configuration.BluePrints.Values.Select(en => en.GetType()).Distinct().ToArray();

                    // add categories

                    foreach (var entitiesType in entitiesTypes)
                    {
                        var group = new ListViewGroup(_pluralization.Pluralize(entitiesType.Name));
                        //group.HeaderAlignment = HorizontalAlignment.Left;
                        entityListView.Groups.Add(group);

                        foreach (var entity in _configuration.BluePrints.Values.Where(en => en.GetType() == entitiesType))
                        {
                            string imgKey = null;

                            if (entity is IVoxelEntity)
                            {
                                imgKey = (entity as IVoxelEntity).ModelName;
                            }

                            var lvi = new ListViewItem { 
                                Text = entity.Name,
                                ImageKey = imgKey,
                                Tag = entity
                            };

                            entityListView.Items.Add(lvi).Group = group;
                        }

                        
                    }


                }
                else
                {
                    // show property grid of this object
                    ShowMainControl(pgDetails);
                    pgDetails.Visible = true;
                    if (selectedObject is BlockProfile)
                    {
                        pgDetails.Enabled = !( (BlockProfile)tvMainCategories.SelectedNode.Tag ).IsSystemCube;
                    }
                    else
                    {
                        pgDetails.Enabled = true;
                    }
                    if (( ModifierKeys & Keys.Control ) != 0) pgDetails.Enabled = true;
                    pgDetails.SelectedObject = tvMainCategories.SelectedNode.Tag;

                    if (selectedObject is IVoxelEntity)
                    {
                        var voxelEntity = selectedObject as IVoxelEntity;

                        ModelStateSelector.PossibleValues = null;

                        if (voxelEntity.ModelName != null)
                        {

                            var model = Program.IconManager.ModelManager.GetModel(voxelEntity.ModelName);

                            if (model != null)
                            {
                                ModelStateSelector.PossibleValues =
                                    model.VoxelModel.States.Select(s => s.Name).ToArray();
                            }
                        }
                    }
                }
            }

        }

        private void ShowMainControl(Control control)
        {
            pgDetails.Hide();
            if (_processorControl != null)
                _processorControl.Hide();
            containerEditor.Hide();

            control.Show();
        }

        private string GetFreeName<TValue>(string nameBase, IDictionary<string, TValue> dictionary)
        {
            var index = 0;
            var name = nameBase;

            while (dictionary.ContainsKey(name))
            {
                index++;
                name = nameBase + index;
            }

            return name;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        private void ContainerEditorItemNeeded(object sender, ItemNeededEventArgs e)
        {
            using (var frmAdd = new FrmItemAdd())
            {
                frmAdd.Configuration = Configuration;

                if (frmAdd.ShowDialog() == DialogResult.OK)
                {
                    e.BlueprintId = frmAdd.SelectedId;
                    e.Count = frmAdd.ItemsCount;
                }
            }
        }

        #endregion

        private void entityListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (entityListView.SelectedItems.Count == 1)
            {
                var lvi = entityListView.SelectedItems[0];

                tvMainCategories.SelectedNode = FindByTag(lvi.Tag);
            }
        }



    }
}
