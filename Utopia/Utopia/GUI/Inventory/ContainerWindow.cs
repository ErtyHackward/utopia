using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Point = System.Drawing.Point;
using RectangleF = S33M3CoreComponents.GUI.Nuclex.RectangleF;

namespace Utopia.GUI.Inventory
{
    public class ContainerWindow : InventoryWindow
    {
        private readonly WorldConfiguration _conf;
        private readonly PlayerEntityManager _player;
        private readonly IconFactory _iconFactory;
        private readonly InputsManager _inputsManager;
        private Container _container;

        protected ListControl _recipesList;
        protected List<InventoryCell> _ingredientCells;
        protected ModelControl _hostModel;
        protected ModelControl _resultModel;
        protected ButtonControl _craftButton;
        protected RectangleF _ingredientsRect;
        private bool _canCraft;

        public ListControl RecipesList
        {
            get { return _recipesList; }
        }

        public ButtonControl CraftButton
        {
            get { return _craftButton; }
        }

        public bool CanCraft
        {
            get { return _canCraft; }
        }

        public PlayerCharacter Player
        {
            get { return _player.PlayerCharacter; }
        }

        public HLSLVoxelModel VoxelEffect
        {
            get { return _resultModel.VoxelEffect; }
            set { 
                _resultModel.VoxelEffect = value;
                _hostModel.VoxelEffect = value;
            }
        }

        public CubeRenderer CubeRenderer {
            get { return _resultModel.CubeRenderer; }
            set { _resultModel.CubeRenderer = value; }
        }

        public ModelControl HostModelControl
        {
            get { return _hostModel; }
        }

        public ModelControl ResultModelControl
        {
            get { return _resultModel; }
        }

        public Container Container
        {
            get { return _container; }
            set { 
                _container = value;

                if (Content != null)
                {
                    Content.ItemPut -= Content_ItemsChanged;
                    Content.ItemTaken -= Content_ItemsChanged;
                    Content.ItemExchanged -= Content_ItemsChanged;
                }

                Content = _container.Content;

                if (Content != null)
                {
                    Content.ItemPut += Content_ItemsChanged;
                    Content.ItemTaken += Content_ItemsChanged;
                    Content.ItemExchanged += Content_ItemsChanged;
                }
                
                _recipesList.Items.Clear();
                _recipesList.SelectItem(-1);
                foreach (var recipe in _conf.Recipes.Where(r => r.ContainerBlueprintId == _container.BluePrintId))
                {
                    _recipesList.Items.Add(recipe);
                }

                _hostModel.SetModel(_container.ModelName);

                if (_recipesList.Items.Count > 0)
                {
                    if (!Children.Contains(_recipesList))
                    {
                        Children.Add(_recipesList);
                        Children.Add(_craftButton);
                        Children.Add(_resultModel);

                        foreach (var cell in _ingredientCells)
                        {
                            Children.Add(cell);
                        }
                    }

                    _resultModel.SetModel(null);
                    _ingredientCells.ForEach( c => c.IsVisible = false);
                }
                else
                {
                    if (Children.Contains(_recipesList))
                    {
                        Children.Remove(_recipesList);
                        Children.Remove(_craftButton);
                        Children.Remove(_resultModel);

                        foreach (var cell in _ingredientCells)
                        {
                            Children.Remove(cell);
                        }
                    }
                }
            }
        }

        void Content_ItemsChanged(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            RecipesListOnSelectionChanged(null, null);
        }

        public ContainerWindow(WorldConfiguration conf, PlayerEntityManager player, IconFactory iconFactory, InputsManager inputsManager) : 
            base(null, iconFactory, new Point(0,0), new Point(40, 200), inputsManager)
        {
            _conf = conf;
            _player = player;
            _iconFactory = iconFactory;
            _inputsManager = inputsManager;
            Bounds.Size = new UniVector(435, 387);
        }

        public virtual void InitializeComponent()
        {
            Children.Clear();

            _recipesList = new ListControl { SelectionMode = ListSelectionMode.Single };
            _recipesList.Bounds = new UniRectangle(200, 200, 200, 120);
            _recipesList.SelectionChanged += RecipesListOnSelectionChanged;

            _hostModel = new ModelControl(_iconFactory.VoxelModelManager)
            {
                Bounds = new UniRectangle(20, 20, 210, 170)
            };
            _hostModel.AlterTransform = Matrix.Identity;

            Children.Add(_hostModel);
           
                
            // craft button

            const int buttonWidth = 185;
            const int buttomHeight = 50;

            _craftButton = new ButtonControl
            {
                Text = "Craft",
                Bounds = new UniRectangle(244, 330, buttonWidth, buttomHeight)
            };
            
            _resultModel = new ModelControl(_iconFactory.VoxelModelManager)
            {
                Bounds = new UniRectangle(210, 20, 190, 120)
            };

            _hostModel.Bounds.Size.X = 200;
                
            _ingredientsRect = new RectangleF(200, 145, 200, 42);

            _ingredientCells = new List<InventoryCell>();
            for (int i = 0; i < 6; i++)
            {
                var cell = new InventoryCell(null, _iconFactory, new Vector2I(), _inputsManager)
                {
                    DrawIconsGroupId = 5,
                    DrawIconsActiveCellId = 6,
                    IsVisible = false
                };

                _ingredientCells.Add(cell);
            }
            
        }

        public void Update()
        {
            RecipesListOnSelectionChanged(null, null);
            _recipesList.updateSlider();
            _recipesList.Slider.ThumbPosition = 0;
        }

        private void RecipesListOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (_recipesList.Items.Count == 0 || _recipesList.SelectedItem == null)
                return;

            var recipe = (Recipe)_recipesList.SelectedItem;

            var bp = _player.PlayerCharacter.EntityFactory.CreateFromBluePrint(recipe.ResultBlueprintId);

            var voxelEntity = bp as IVoxelEntity;
            var cube = bp as CubeResource;

            if (cube != null)
            {
                _resultModel.SetCube(cube);
            } 
            else if (voxelEntity != null)
            {
                _resultModel.SetModel(voxelEntity.ModelName);
            }

            // show ingredients slots
            // and position it in the center of the ingredients rect

            _canCraft = true;


            for (var i = 0; i < _ingredientCells.Count; i++)
            {
                var cell = _ingredientCells[i];
                cell.IsVisible = i < recipe.Ingredients.Count;

                if (cell.IsVisible)
                {
                    var bpId = recipe.Ingredients[i].BlueprintId;

                    var needItems = recipe.Ingredients[i].Count;
                    var haveItems = _container.Content.Where(s => s.Item.BluePrintId == bpId).Sum(s => s.ItemsCount);

                    if (haveItems < needItems)
                        _canCraft = false;

                    cell.Slot = new ContainedSlot
                    {
                        Item = (IItem)_player.PlayerCharacter.EntityFactory.CreateFromBluePrint(bpId)
                    };

                    cell.CountString = string.Format("{0} / {1}", needItems, haveItems);

                    var offsetX = (_ingredientsRect.Width - recipe.Ingredients.Count * 50) / 2;
                    cell.Bounds = new UniRectangle(_ingredientsRect.X + offsetX + i * 50, _ingredientsRect.Y, 42, 42);
                }
            }
        }
    }

    public class ContainerWindowControlRenderer : IFlatControlRenderer<ContainerWindow>
    {
        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
            ContainerWindow control, IFlatGuiGraphics graphics
            )
        {
            var controlBounds = control.GetAbsoluteBounds();
            graphics.DrawElement("utopiawindow", ref controlBounds);
        }

    }
}
