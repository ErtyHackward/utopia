using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using S33M3CoreComponents.Config;
using Utopia.Editor.Properties;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Net.Web.Responses;
using Utopia.Shared.Services;
using Utopia.Shared.Settings;
using System.Linq;
using Utopia.Shared.Tools;
using Utopia.Shared.LandscapeEntities.Trees;
using System.Threading;
using Utopia.Editor.DataPipe;
using System.IO.Pipes;
using ProtoBuf;
using Utopia.Shared.Tools.XMLSerializer;
using S33M3Resources.Structs;
using Container = Utopia.Shared.Entities.Concrete.Container;

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
        private AsyncOperation _ao;

        #region Public Properties

        public WorldConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;

                if (_configuration != null)
                {

                    CheckFileIntegrity();

                    Text = _configuration.ConfigurationName + " with processor : " + _configuration.WorldProcessor +
                           " - realm editor";
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

                    containerEditor.Configuration = _configuration;
                    containerEditor.Icons = _icons;
                    ContainerSetSelector.Configuration = _configuration;
                    CubeSelector.Configuration = _configuration;
                    BlueprintTextHintConverter.Configuration = _configuration;
                    BlueprintTextHintConverter.Images = _icons;

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

        private void CheckFileIntegrity()
        {
            //Upgrade to version 2
            if (Configuration.Version < 2)
            {
                //Init Textures arrays
                foreach (var blockp in Configuration.BlockProfiles.Where(x => x.Textures == null))
                {
                    blockp.Textures = new TextureData[6];
                    for (int i = 0; i < 6; i++)
                    {
                        blockp.Textures[i] = new TextureData();
                    }
                }
                //Configuration.Version = 2;
            }
        }

        private void UpdateImageList()
        {
            //Removes all image above _entitiesOffset
            while (imageList1.Images.Count > _entitiesOffset)
                imageList1.Images.RemoveAt(imageList1.Images.Count - 1);

            largeImageList.Images.Clear();

            ModelSelector.Models.Clear();
            ModelSelector.MultiStatesModels.Clear();
            TextureSelector.TextureIcons.Clear();

            //Create Items icons
            foreach (var visualVoxelModel in Program.IconManager.ModelManager.Enumerate())
            {
                foreach (var voxelModelState in visualVoxelModel.VoxelModel.States)
                {
                    var id = visualVoxelModel.VoxelModel.Name +
                             (voxelModelState.IsMainState ? "" : ":" + voxelModelState.Name);

                    imageList1.Images.Add(_icons[id]);
                    largeImageList.Images.Add(id, _icons[id]);

                    _icons[id].Tag = imageList1.Images.Count - 1; //Add Image Index in imageList

                    if (voxelModelState.IsMainState)
                    {
                        ModelSelector.Models.Add(visualVoxelModel.VoxelModel.Name,
                            _icons[visualVoxelModel.VoxelModel.Name]);
                        if (visualVoxelModel.VoxelModel.States.Count > 1)
                            ModelSelector.MultiStatesModels.Add(visualVoxelModel.VoxelModel.Name,
                                _icons[visualVoxelModel.VoxelModel.Name]);
                    }
                }
            }

            _cubeOffset = imageList1.Images.Count;
            //Create blocks icons
            foreach (var cubeprofiles in _configuration.GetAllCubesProfiles())
            {
                if (cubeprofiles.Id == WorldConfiguration.CubeId.Air) continue;
                imageList1.Images.Add(_icons["CubeResource_" + cubeprofiles.Name]);
                largeImageList.Images.Add("CubeResource_" + cubeprofiles.Name,
                    _icons["CubeResource_" + cubeprofiles.Name]);
                _icons["CubeResource_" + cubeprofiles.Name].Tag = imageList1.Images.Count - 1;
            }

            //Create Texture icons
            foreach (var icon in _icons.Where(x => x.Key.StartsWith("TextureCube_")))
            {
                string[] IconName = icon.Key.Replace("TextureCube_", "").Split('@');
                string finalName;
                if (int.Parse(IconName[1]) > 1)
                {
                    finalName = string.Format("{0} [anim. {1} frames]", IconName[0], IconName[1]);
                }
                else finalName = string.Format("{0}", IconName[0]);

                TextureSelector.TextureIcons.Add(finalName, icon.Value);
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

            UpdateRecent();

            _ao = AsyncOperationManager.CreateOperation(null);
        }

        #region Private Methods

        #region Menu related

        private void UpdateRecent()
        {
            var recent = Settings.Default.RecentConfigurations;

            if (recent != null)
            {
                for (int i = fileToolStripMenuItem.DropDownItems.Count - 1; i >= 0; i--)
                {
                    ToolStripItem item = fileToolStripMenuItem.DropDownItems[i];
                    if (item.Tag is string)
                        fileToolStripMenuItem.DropDownItems.RemoveAt(i);
                }

                var insertIndex = fileToolStripMenuItem.DropDownItems.IndexOf(recentToolStripMenuItem) + 1;

                foreach (var recentConfiguration in recent)
                {
                    var item = new ToolStripMenuItem(Path.GetFileName(recentConfiguration));
                    item.Tag = recentConfiguration;
                    item.Click += (sender, args) => OpenConfiguration((sender as ToolStripItem).Tag as string);
                    fileToolStripMenuItem.DropDownItems.Insert(insertIndex++, item);
                }
                recentToolStripMenuItem.Visible = true;
            }
            else
            {
                recentToolStripMenuItem.Visible = false;
            }
        }

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
                    newConfiguration = new FlatWorldConfiguration(withHelperAssignation: true);
                    ((FlatWorldConfiguration)newConfiguration).ProcessorParam.CreateDefaultValues();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    newConfiguration = new UtopiaWorldConfiguration(withHelperAssignation: true);
                    ((UtopiaWorldConfiguration)newConfiguration).ProcessorParam.CreateDefaultValues();
                    break;
                default:
                    break;
            }

            newConfiguration.InjectMandatoryObjects();
            newConfiguration.ConfigurationName = "noname";
            newConfiguration.CreatedAt = DateTime.Now;
            newConfiguration.WorldProcessor = processorChoose.SelectedProcessor;
            newConfiguration.Version = 2;

            processorChoose.Dispose();

            Configuration = newConfiguration;
        }

        //Open
        private void openRealmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenConfiguration(openFileDialog1.FileName);
            }
        }

        private FrmLoading _loadingForm = new FrmLoading();

        private void OpenConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                var recent = Settings.Default.RecentConfigurations;

                if (recent != null)
                {
                    recent.Remove(fileName);
                }
                UpdateRecent();

                MessageBox.Show("File doesn't exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowLoadingForm();

            new ThreadStart(delegate
            {
                try
                {
                    var configuration = WorldConfiguration.LoadFromFile(fileName, true);

                    var availableModels =
                        Program.IconManager.ModelManager.Enumerate().Select(m => m.VoxelModel.Name).ToList();

                    var needToLoadModels =
                        configuration.GetUsedModelsNames().Where(m => !availableModels.Contains(m)).ToList();

                    _ao.Post(delegate
                    {
                        _loadingForm.infoLabel.Text = "Downloading models...";
                    }, null);

                    if (needToLoadModels.Count > 0)
                    {
                        for (int i = 0; i < needToLoadModels.Count; i++)
                        {
                            var needToLoadModel = needToLoadModels[i];
                            _ao.Post(delegate
                            {
                                _loadingForm.infoLabel.Text = string.Format("Downloading model: {1}/{2} {0}",
                                    needToLoadModel, i + 1, needToLoadModels.Count);
                            }, null);

                            Program.IconManager.ModelManager.DownloadModel(needToLoadModel);
                        }
                    }

                    _ao.Post(delegate {
                                          _loadingForm.infoLabel.Text = "Rendering icons...";
                    }, null);

                    _icons = Program.IconManager.GenerateIcons(configuration);

                    _ao.Post(delegate {
                                          Configuration = configuration;
                    }, null);

                    _filePath = fileName;


                    var recent = Settings.Default.RecentConfigurations ?? new StringCollection();
                    recent.Remove(_filePath);
                    recent.Insert(0, _filePath);

                    while (recent.Count > 3)
                    {
                        recent.RemoveAt(recent.Count - 1);
                    }

                    Settings.Default.RecentConfigurations = recent;
                    Settings.Default.Save();
                    _ao.Post(delegate
                    {
                        UpdateRecent();
                        HideLoadingForm();
                    }, null);
                }
                catch (Exception x)
                {
                    _ao.Post(delegate
                    {
                        HideLoadingForm();
                        MessageBox.Show("Error: " + x.Message, "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }, null);
                }

            }).BeginInvoke(null, null);
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
            //Create System mandatory entities
            CreateMandatorySystemEntities();

            tvMainCategories.BeginUpdate();

            object selectedTag = null;

            if (tvMainCategories.SelectedNode != null)
                selectedTag = tvMainCategories.SelectedNode.Tag;

            //Bind Configuration object to General root Node
            tvMainCategories.Nodes["General"].Tag = _configuration;

            //Get Entities Root node collection
            var entitiesRootNode = tvMainCategories.Nodes["Entities"];
            entitiesRootNode.Nodes.Clear();

            //Add new Entities nodes

            // fill empty categories
            foreach (var entity in _configuration.BluePrints.Values)
            {
                if (string.IsNullOrWhiteSpace(entity.GroupName))
                    entity.GroupName = _pluralization.Pluralize(entity.GetType().Name);
            }

            // get entities types
            var entitiesGroups = _configuration.BluePrints.Values.Select(e => e.GroupName).Distinct().ToArray();

            // add categories

            foreach (var entityGroup in entitiesGroups)
            {
                var categoryNode = AddSubNode(entitiesRootNode, entityGroup, entityGroup);

                categoryNode.ContextMenuStrip = contextMenuCategories;
                categoryNode.ImageIndex = 9;
                categoryNode.SelectedImageIndex = 10;

                foreach (var entity in _configuration.BluePrints.Values.Where(e => e.GroupName == entityGroup))
                {
                    string iconName = null;

                    if (entity is IVoxelEntity)
                    {
                        var voxelEntity = entity as IVoxelEntity;
                        iconName = GetVoxelEntityImgName(voxelEntity);
                    }

                    var node = AddSubNode(categoryNode, entity.Name, entity, iconName);

                    node.ContextMenuStrip = contextMenuEntity;
                }

            }

            //Clear all the Cube node items
            var cubesRootNode = tvMainCategories.Nodes["Cubes"];
            cubesRootNode.Nodes.Clear();
            for (var i = 0; i < _configuration.BlockProfiles.Where(x => x != null).Count(); i++)
            {
                var blockProfile = _configuration.BlockProfiles[i];
                if (blockProfile.Name == "System Reserved") continue;

                var node = AddSubNode(cubesRootNode, blockProfile.Name, blockProfile,
                    "CubeResource_" + blockProfile.Name);
                node.ContextMenuStrip = contextMenuEntity;
                node.Tag = blockProfile;
            }

            //Trees Landscape Entities
            var LandEntitiesRootNode = tvMainCategories.Nodes["LandscapeEntities"];
            var TreesRootNode = LandEntitiesRootNode.Nodes["Trees"];
            TreesRootNode.Nodes.Clear();
            foreach (var tree in _configuration.TreeBluePrints)
            {
                var node = AddSubNode(TreesRootNode, tree.Name, tree, null);
                node.ContextMenuStrip = contextMenuEntity;
            }

            #region Sets

            var setsRootNode = tvMainCategories.Nodes["Container sets"];
            setsRootNode.Nodes.Clear();

            foreach (var containerSet in _configuration.ContainerSets)
            {
                var node = AddSubNode(setsRootNode, containerSet.Key, containerSet.Value);
                node.ContextMenuStrip = contextMenuEntity;
            }

            #endregion

            #region Recipes

            var recipesNode = tvMainCategories.Nodes["Recipes"];
            recipesNode.Nodes.Clear();

            //Remove all recipte that are using a not existing bluePrintId
            _configuration.Recipes.RemoveAll(
                x => _configuration.BluePrints.Keys.Contains(x.ResultBlueprintId) == false && x.ResultBlueprintId >= 256);

            foreach (var recipeGroup in _configuration.Recipes.GroupBy(g => g.ContainerBlueprintId))
            {
                var groupNode = AddSubNode(recipesNode,
                    recipeGroup.Key == 0 ? "Player" : _configuration.BluePrints[recipeGroup.Key].Name, recipeGroup.Key, recipeGroup.Key == 0 ? "" : GetVoxelEntityImgName((IVoxelEntity)_configuration.BluePrints[recipeGroup.Key]));
                groupNode.ContextMenuStrip = contextMenuCategories;
                groupNode.Tag = recipeGroup.Key;

                foreach (var recipe in recipeGroup)
                {
                    string iconName = null;
                    if (recipe.ResultBlueprintId != 0)
                    {
                        if (recipe.ResultBlueprintId < 256)
                        {
                            iconName = "CubeResource_" + _configuration.BlockProfiles[recipe.ResultBlueprintId].Name;
                        }
                        else
                        {
                            var entity = _configuration.BluePrints[recipe.ResultBlueprintId];
                            if (entity is IVoxelEntity)
                            {
                                var voxelEntity = entity as IVoxelEntity;
                                iconName = voxelEntity.ModelName;
                            }
                        }
                    }

                    var node = AddSubNode(groupNode, recipe.Name, recipe, iconName);
                    node.ContextMenuStrip = contextMenuEntity;
                }
            }

            #endregion

            #region Services

            var servicesNode = tvMainCategories.Nodes["Services"];
            servicesNode.Nodes.Clear();

            foreach (var service in _configuration.Services)
            {
                var node = AddSubNode(servicesNode, service.GetType().Name, service);
                node.ContextMenuStrip = contextMenuEntity;
                node.ImageIndex = 12;
                node.SelectedImageIndex = 12;
            }

            #endregion

            if (selectedTag != null)
            {
                tvMainCategories.SelectedNode = FindByTag(selectedTag);
            }

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

        private void CreateMandatorySystemEntities()
        {
            //Create System mandatory entities

            //The SoulStone
            if (_configuration.BluePrints.Values.Select(x => x.Name).Where(x => x == "SoulStone").Count() == 0)
            {
                var entityInstance = Configuration.CreateNewEntity(typeof(Shared.Entities.Concrete.System.SoulStone));
            }
        }

        private bool CheckConfiguration()
        {
            bool result = true;
            //The SoulStone
            if (_configuration.BluePrints.Values.OfType<Utopia.Shared.Entities.Concrete.System.SoulStone>().Count() != 1)
            {
                MessageBox.Show(string.Format("Mandatory soulstone entity is missing !"), "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            //Check If the per block per texture all speed animation are the same.
            foreach (var profile in _configuration.BlockProfiles.Where(x => x != null && x.Textures != null))
            {
                if (profile.Name == "Air" || profile.Name == "System Reserved") continue;

                Dictionary<string, TextureData> BlockTextures = new Dictionary<string, TextureData>();
                foreach (var BlockTexture in profile.Textures.Where(x => x != null))
                {
                    if (BlockTexture.Texture.Name == null)
                    {
                        MessageBox.Show(
                            string.Format("The block {0} doesn't have all its texture assigned !", profile.Name),
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    TextureData d;
                    if (BlockTextures.TryGetValue(BlockTexture.Texture.Name, out d) == false)
                    {
                        //Is not existing add it
                        BlockTextures[BlockTexture.Texture.Name] = BlockTexture;
                        continue;
                    }

                    //Existing texture
                    if (d.AnimationSpeed != BlockTexture.AnimationSpeed)
                    {
                        MessageBox.Show(
                            string.Format(
                                "The texture {0} for the block {1} is used multiple times with different animation speed, the speed must be equal !",
                                BlockTexture.Texture.Name, profile.Name), "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            //Clean up file name
            foreach (var blockp in Configuration.BlockProfiles.Where(x => x != null))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (blockp.Textures[i].Texture.Name != null)
                    {
                        var array = blockp.Textures[i].Texture.Name.Split(' ');
                        blockp.Textures[i].Texture.Name = array[0];
                    }
                }
            }

            return result;
        }

        private void Save(string filePath)
        {
            try
            {
                if (CheckConfiguration())
                {

                    // don't store default groups
                    foreach (var bluePrint in Configuration.BluePrints.Values)
                    {
                        if (bluePrint.GroupName == _pluralization.Pluralize(bluePrint.GetType().Name))
                            bluePrint.GroupName = null;
                    }

                    Configuration.UpdatedAt = DateTime.Now;
                    Configuration.SaveToFile(filePath);
                    _filePath = filePath;
                }
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

                var t = FindByTag(tag, node1);

                if (t != null)
                    return t;
            }

            return null;
        }

        #region GUI events Handling

        //Called when the ADD button is pushed on a Main Category treeview
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvMainCategories.SelectedNode == null)
                return;

            switch (tvMainCategories.SelectedNode.Name)
            {
                case "Entities":
                    AddEntity();
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

                    var possibleBlueprints =
                        _configuration.BluePrints.Values.Where(v => v is Container)
                            .Select(bp => bp.BluePrintId)
                            .ToList();

                    var resultBpId = (ushort)0;

                    if (possibleBlueprints.Count != 0)
                    {
                        // select base entity
                        var frm =
                            new FrmBlueprintChoose(
                                new[] { new KeyValuePair<ushort, string>(0, "Player") }.Concat(
                                    possibleBlueprints.Select(
                                        bpid =>
                                            new KeyValuePair<ushort, string>(bpid, _configuration.BluePrints[bpid].Name))));

                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            resultBpId = frm.SelectedBlueprint;
                        }
                        else
                            return;
                    }

                    var recipe = new Recipe
                    {
                        Name = "noname",
                        ContainerBlueprintId = resultBpId
                    };
                    _configuration.Recipes.Add(recipe);
                    UpdateTree();
                    tvMainCategories.SelectedNode = FindByTag(recipe);

                    break;
                case "Trees":
                    var tree = new TreeBluePrint()
                    {
                        Name = "Tree",
                        Angle = 30,
                        Iteration = 3,
                        IterationRndLevel = 0,
                        SmallBranches = true,
                        TrunkType = TrunkType.Single,
                        FoliageGenerationStart = 1,
                        Axiom = "FFF",
                        FoliageSize = new Vector3I(1, 1, 1)
                    };
                    tree.Id = _configuration.GetNextLandscapeEntityId();
                    _configuration.TreeBluePrints.Add(tree);
                    UpdateTree();
                    tvMainCategories.SelectedNode = FindByTag(tree);
                    break;
                case "Services":
                    var frmServiceAdd = new FrmServiceAdd(_configuration);

                    if (frmServiceAdd.ShowDialog() == DialogResult.OK)
                    {
                        var service = (Service)Activator.CreateInstance(frmServiceAdd.SelectedType);
                        _configuration.Services.Add(service);
                        UpdateTree();
                        tvMainCategories.SelectedNode = FindByTag(service);
                    }

                    break;
            }
            if (tvMainCategories.SelectedNode == null)
                return;

            if (tvMainCategories.SelectedNode.Parent == tvMainCategories.Nodes["Recipes"])
            {
                var recipe = new Recipe
                {
                    Name = "noname",
                    ContainerBlueprintId = (ushort)tvMainCategories.SelectedNode.Tag
                };

                _configuration.Recipes.Add(recipe);
                UpdateTree();
                tvMainCategories.SelectedNode = FindByTag(recipe);
                return;
            }

            if (tvMainCategories.SelectedNode.Tag is string)
            {
                AddEntity();
            }
        }

        private Type _lastType;

        private void AddEntity()
        {
            var form = new FrmEntityChoose(_configuration) { SelectedType = _lastType };
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lastType = form.SelectedType;
                var entityInstance = Configuration.CreateNewEntity(form.SelectedType);
                UpdateTree();
                tvMainCategories.SelectedNode = FindByTag(entityInstance);
            }
        }

        //Called when the DELETE button is pushed on a Main Category treeview
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tag = tvMainCategories.SelectedNode.Tag;
            if (tag is Entity)
            {
                var entity = (Entity)tvMainCategories.SelectedNode.Tag;
                if (!entity.IsSystemEntity) _configuration.BluePrints.Remove(entity.BluePrintId);
                else return;
            }
            else if (tag is BlockProfile)
            {
                var blockProfile = (BlockProfile)tvMainCategories.SelectedNode.Tag;
                if (!_configuration.DeleteBlockProfile(blockProfile))
                {
                    return;
                }
            }
            else if (tag is SlotContainer<BlueprintSlot>)
            {
                var key = _configuration.ContainerSets.Where(p => p.Value == tag).Select(p => p.Key).First();
                _configuration.ContainerSets.Remove(key);
            }
            else if (tag is Recipe)
            {
                _configuration.Recipes.Remove((Recipe)tag);
            }
            if (tag is TreeBluePrint)
            {
                _configuration.TreeBluePrints.Remove((TreeBluePrint)tag);
            }

            tvMainCategories.SelectedNode.Remove();
        }

        private void SendTreeTemplateForVisualization(TreeBluePrint template)
        {
            //Tree blue print properties have been change ...
            if (Pipe.RunningLtree != null && Pipe.RunningLtree.HasExited == false)
            {
                //Serialize object
                string xmlobj = XmlSerialize.XmlSerializeToString(template);
                xmlobj = xmlobj.Replace(Environment.NewLine, "|");
                Pipe.MessagesQueue.Enqueue(xmlobj);
            }
        }

        private int GetVoxelEntityImgIndex(IVoxelEntity voxelEntity)
        {
            var name = GetVoxelEntityImgName(voxelEntity);
            return string.IsNullOrEmpty(name) ? -1 : (int)_icons[name].Tag;
        }

        private string GetVoxelEntityImgName(IVoxelEntity voxelEntity)
        {
            if (!string.IsNullOrEmpty(voxelEntity.ModelName))
            {
                if (!string.IsNullOrEmpty(voxelEntity.ModelState) && voxelEntity.ModelState != "Default")
                {
                    return voxelEntity.ModelName + ":" + voxelEntity.ModelState;
                }
                return voxelEntity.ModelName;
            }

            return null;
        }

        /// <summary>
        /// Event raised when a property change in the Details property grid.
        /// </summary>
        private void pgDetails_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (((PropertyGrid)sender).SelectedObject.GetType() == typeof(TreeBluePrint))
            {
                SendTreeTemplateForVisualization((TreeBluePrint)((PropertyGrid)sender).SelectedObject);
            }

            //Update ListBox Icon when the modelName is changing
            if (e.ChangedItem.Label == "ModelName")
            {
                //Get the currently changed treeNode
                TreeNode item = tvMainCategories.SelectedNode;

                //Get the underlying entity
                var entity = pgDetails.SelectedObject;

                //Check if this entity can have a Voxel body
                var voxelEntity = entity as IVoxelEntity;

                if (voxelEntity.ModelState != null)
                {
                    voxelEntity.ModelState = null;
                }

                //Look at currently existing Voxel Models following Entity Name

                item.ImageIndex = GetVoxelEntityImgIndex(voxelEntity);
                item.SelectedImageIndex = item.ImageIndex;

                ModelStateConverter.PossibleValues = null;

                if (voxelEntity.ModelName != null)
                {

                    var model = Program.IconManager.ModelManager.GetModel(voxelEntity.ModelName);

                    if (model != null)
                    {
                        ModelStateConverter.PossibleValues =
                            new string[] { null }.Concat(model.VoxelModel.States.Select(s => s.Name)).ToArray();
                    }
                }
            }

            if (e.ChangedItem.Label == "ModelState")
            {
                var item = tvMainCategories.SelectedNode;
                var entity = pgDetails.SelectedObject;
                var voxelEntity = entity as IVoxelEntity;

                item.ImageIndex = GetVoxelEntityImgIndex(voxelEntity);
                item.SelectedImageIndex = item.ImageIndex;
            }

            if (e.ChangedItem.Label == "Name")
            {
                UpdateTree();
            }

            if (e.ChangedItem.Label == "GroupName")
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
                else if (selectedObject is string)
                {
                    ShowMainControl(entityListView);

                    // show entities in that category
                    entityListView.Groups.Clear();
                    entityListView.Items.Clear();
                    foreach (
                        var entity in
                            _configuration.BluePrints.Values.Where(en => en.GroupName == (string)selectedObject))
                    {
                        string imgKey = null;

                        if (entity is IVoxelEntity)
                        {
                            imgKey = (entity as IVoxelEntity).ModelName;
                        }

                        var lvi = new ListViewItem
                        {
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
                    var entitiesGroups =
                        _configuration.BluePrints.Values.Select(en => en.GroupName).Distinct().ToArray();

                    // add categories

                    foreach (var entityGroup in entitiesGroups)
                    {
                        var group = new ListViewGroup(entityGroup);
                        //group.HeaderAlignment = HorizontalAlignment.Left;
                        entityListView.Groups.Add(group);

                        foreach (var entity in _configuration.BluePrints.Values.Where(en => en.GroupName == entityGroup)
                            )
                        {
                            string imgKey = null;

                            if (entity is IVoxelEntity)
                            {
                                imgKey = (entity as IVoxelEntity).ModelName;
                            }

                            var lvi = new ListViewItem
                            {
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
                        pgDetails.Enabled = !((BlockProfile)tvMainCategories.SelectedNode.Tag).IsSystemCube;
                    }
                    else
                    {
                        pgDetails.Enabled = true;
                    }
                    if ((ModifierKeys & Keys.Control) != 0) pgDetails.Enabled = true;
                    pgDetails.SelectedObject = tvMainCategories.SelectedNode.Tag;

                    if (selectedObject is IVoxelEntity)
                    {
                        var voxelEntity = selectedObject as IVoxelEntity;

                        ModelStateConverter.PossibleValues = null;

                        if (voxelEntity.ModelName != null)
                        {

                            var model = Program.IconManager.ModelManager.GetModel(voxelEntity.ModelName);

                            if (model != null)
                            {
                                ModelStateConverter.PossibleValues =
                                    new string[] { null }.Concat(model.VoxelModel.States.Select(s => s.Name)).ToArray();
                            }
                        }
                    }

                    if (selectedObject is TreeBluePrint)
                    {
                        SendTreeTemplateForVisualization((TreeBluePrint)selectedObject);
                    }
                }
            }

        }

        private void ShowMainControl(Control control)
        {
            pgDetails.Hide();
            if (_processorControl != null) _processorControl.Hide();
            containerEditor.Hide();
            entityListView.Hide();

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

        private Thread _pipeThread;
        private Pipe _dataPipe = new Pipe();

        private void ltreeVisualizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_pipeThread == null ||
                (_pipeThread.ThreadState != System.Threading.ThreadState.Running &&
                 _pipeThread.ThreadState != System.Threading.ThreadState.WaitSleepJoin))
            {
                if (_pipeThread != null) Console.WriteLine(_pipeThread.ThreadState);
                _pipeThread = new Thread(_dataPipe.Start);
                _pipeThread.Start();
            }

            if (Pipe.RunningLtree == null || Pipe.RunningLtree.HasExited == true)
                Pipe.RunningLtree = System.Diagnostics.Process.Start(@"LtreeVisualizer.exe");
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Pipe.RunningLtree != null && Pipe.RunningLtree.HasExited == false) Pipe.RunningLtree.Kill();
            if (_pipeThread != null &&
                (_pipeThread.ThreadState == System.Threading.ThreadState.Running ||
                 _pipeThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin))
            {
                Pipe.StopThread = true;
                try
                {
                    using (NamedPipeClientStream npcs = new NamedPipeClientStream("UtopiaEditor"))
                    {
                        npcs.Connect(100);
                    }

                }
                catch (Exception)
                {
                }

            }
        }

        private void tvMainCategories_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void tvMainCategories_MouseDown(object sender, MouseEventArgs e)
        {
            var node = tvMainCategories.GetNodeAt(e.Location);

            if (node != null)
                tvMainCategories.SelectedNode = node;
        }

        private void checkConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_configuration == null)
                return;

            var sb = new StringBuilder();
            var problemFound = false;
            sb.AppendLine("Configuration check report");
            sb.AppendLine();

            #region check how much items without recipes

            var items =
                _configuration.BluePrints.Values.OfType<Item>()
                    .Where(i => !_configuration.Recipes.Exists(r => r.ResultBlueprintId == i.BluePrintId))
                    .ToList();
            if (items.Count > 0)
            {
                problemFound = true;
                sb.AppendLine("Detected uncraftable items:");

                foreach (var item in items)
                {
                    sb.AppendLine(item.ToString());
                }
                sb.AppendLine();
            }

            #endregion

            if (!problemFound)
            {
                sb.AppendLine("No problems found!");
            }
            else
            {
                sb.AppendLine("No more problems");
            }

            var filePath = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
            File.WriteAllText(filePath, sb.ToString());
            Process.Start(filePath).WaitForExit();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tag = tvMainCategories.SelectedNode.Tag;
            if (tag is Entity)
            {
                var entity = (Entity)tag;
                entity = (Entity)entity.Clone();
                entity.Name += " (copy)";
                Configuration.AddNewEntity(entity);
                UpdateTree();
                tvMainCategories.SelectedNode = FindByTag(entity);
            }
            else if (tag is BlockProfile)
            {
                var profile = (BlockProfile)tag;
                profile = Serializer.DeepClone(profile);
                profile.Name += " (copy)";

                if (Configuration.BlockProfiles.Length >= 256)
                {
                    MessageBox.Show("Only 255 blocks are possible");
                    return;
                }

                Configuration.CreateNewCube(profile);

                UpdateTree();
                tvMainCategories.SelectedNode = FindByTag(profile);
            }
            else if (tag is Recipe)
            {
                var recipe = Serializer.DeepClone((Recipe)tag);
                recipe.Name += " (copy)";
                Configuration.Recipes.Add(recipe);
                UpdateTree();
                tvMainCategories.SelectedNode = FindByTag(recipe);
            }
            else
            {
                MessageBox.Show("Sorry, copying of this is not yet implemented, ask Erty Hackward on the forum");
            }
        }

        private void entityListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                pgDetails.SelectedObjects =
                    entityListView.SelectedItems.Cast<ListViewItem>().Select(i => (Entity)i.Tag).ToArray();

                ShowMainControl(pgDetails);
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            Activate();
        }

        private void ShowLoadingForm()
        {
            _loadingForm.StartPosition = FormStartPosition.Manual;
            _loadingForm.Location = new Point(this.Location.X + (this.Width - _loadingForm.Width) / 2,
                this.Location.Y + (this.Height - _loadingForm.Height) / 2);
            _loadingForm.Show(this);
            _loadingForm.infoLabel.Text = "";
            _loadingForm.Refresh();
        }

        private void HideLoadingForm()
        {
            _loadingForm.Hide();
        }

        private ManualResetEvent waitModels = new ManualResetEvent(false);
        private ModelsListResponse _modelsListResponse;

        private void downloadAllModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            ClientSettings.Current = new XmlSettingsManager<ClientConfig>(@"client.config", SettingsStorage.CustomPath,
                appdata + @"\Realms\Client");
            ClientSettings.Current.Load();

            if (string.IsNullOrEmpty(ClientSettings.Current.Settings.Token))
            {
                MessageBox.Show("Unable to find login credentials. Please login to the game, exit. And try here again.");
                return;
            }

            ShowLoadingForm();

            new ThreadStart(delegate
            {
                try
                {
                    var webApi = new ClientWebApi();
                    webApi.TokenVerified += webApi_TokenVerified;
                    webApi.OauthVerifyTokenAsync(ClientSettings.Current.Settings.Token);
                    waitModels.WaitOne();

                    if (string.IsNullOrEmpty(webApi.Token))
                    {
                        RunInMainThread(() => MessageBox.Show(this, "Authorization failed."));
                        return;
                    }

                    RunInMainThread(() => _loadingForm.infoLabel.Text = "Downloading list...");

                    webApi.GetModelsListAsync(webApi_ModelsReceived);
                    waitModels.Reset();
                    waitModels.WaitOne();

                    var availableModels = Program.IconManager.ModelManager.Enumerate().ToList();
                    var needToDownload = new List<string>();
                    foreach (var modelInfo in _modelsListResponse.Models)
                    {
                        var m = availableModels.FirstOrDefault(mo => mo.VoxelModel.Name == modelInfo.Name);

                        if (m == null)
                        {
                            needToDownload.Add(modelInfo.Name);
                            continue;
                        }

                        m.VoxelModel.UpdateHash();
                        if (!string.IsNullOrEmpty(modelInfo.Hash) && m.VoxelModel.Hash.ToString() != modelInfo.Hash)
                            needToDownload.Add(modelInfo.Name);
                    }

                    if (needToDownload.Count == 0)
                    {
                        RunInMainThread(() => MessageBox.Show(this, "All models were already downloaded"));
                        return;
                    }

                    for (int i = 0; i < needToDownload.Count; i++)
                    {
                        var modelName = needToDownload[i];
                        RunInMainThread(
                            () =>
                                _loadingForm.infoLabel.Text =
                                    string.Format("Downloading model {0}/{1} {2}", i + 1, needToDownload.Count,
                                        modelName));
                        Program.IconManager.ModelManager.DownloadModel(modelName);
                    }

                    if (_configuration != null)
                    {
                        RunInMainThread(() => _loadingForm.infoLabel.Text = "Rendering images...");
                        _icons = Program.IconManager.GenerateIcons(_configuration);
                        RunInMainThread(() =>
                        {
                            UpdateImageList();
                            UpdateTree();
                        });
                    }
                    RunInMainThread(() => MessageBox.Show(this, needToDownload.Count + " models were loaded."));
                }
                finally
                {
                    RunInMainThread(HideLoadingForm);
                }
            }).BeginInvoke(null, null);

        }

        public void RunInMainThread(System.Action action)
        {
            _ao.Post((o) => action(), null);
        }


        private void webApi_TokenVerified(object sender, VerifyResponse e)
        {
            waitModels.Set();
        }

        private void webApi_ModelsReceived(ModelsListResponse response)
        {
            _modelsListResponse = response;
            waitModels.Set();
        }

        private void tvMainCategories_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var item = ((TreeNode)e.Item).Tag;

            if (!(item is Recipe))
                return;

            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void tvMainCategories_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tvMainCategories_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
                TreeNode newNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

                bool allowed = false;

                var recipe = newNode.Tag as Recipe;

                if (recipe != null)
                {
                    if (destinationNode.Parent != tvMainCategories.Nodes["Recipes"])
                        return;

                    recipe.ContainerBlueprintId = (ushort)destinationNode.Tag;
                    allowed = true;
                }

                if (allowed)
                {
                    destinationNode.Nodes.Add((TreeNode)newNode.Clone());
                    destinationNode.Expand();
                    //Remove Original Node
                    newNode.Remove();
                }
            }

        }

        private void tvMainCategories_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            var node = tvMainCategories.GetNodeAt(pt);

            if (node != null)
                tvMainCategories.SelectedNode = node;
        }
    }
}
