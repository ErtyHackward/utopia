using System;
using System.Collections.Generic;
using System.Linq;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Inputs;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.GUI.Inventory;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.Crafting
{
    public class CraftingWindow : WindowControl
    {
        private readonly WorldConfiguration _conf;
        private readonly PlayerEntityManager _player;
        private readonly IconFactory _iconFactory;
        private readonly InputsManager _inputsManager;

        protected ListControl _recipesList;
        protected List<InventoryCell> _ingredientCells;
        protected ModelControl _resultModel;
        protected ButtonControl _craftButton;
        protected RectangleF _ingredientsRect;
        private bool _canCraft;

        public ModelControl ModelControl
        {
            get { return _resultModel; }
        }

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
            set { _resultModel.VoxelEffect = value; }
        }

        public CraftingWindow(WorldConfiguration conf, PlayerEntityManager player, IconFactory iconFactory, InputsManager inputsManager)
        {
            _conf = conf;
            _player = player;
            _iconFactory = iconFactory;
            _inputsManager = inputsManager;
        }
        
        public virtual void InitializeComponent()
        {
            Children.Clear();
            
            _ingredientsRect = new RectangleF(240, 275, 360, 42);

            _recipesList = new ListControl { SelectionMode = ListSelectionMode.Single };
            _recipesList.Bounds = new UniRectangle(20, 50, 200, 300);
            _recipesList.SelectionChanged += RecipesListOnSelectionChanged;

            foreach (var recipe in _conf.Recipes.Where(r => r.ContainerBlueprintId == 0))
            {
                _recipesList.Items.Add(recipe);
            }

            Children.Add(_recipesList);

            // cells

            _resultModel = new ModelControl(_iconFactory.VoxelModelManager) 
            { 
                Bounds = new UniRectangle(300, 10, 230, 230) 
            };

            Children.Add(_resultModel);

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
                Children.Add(cell);
            }

            // craft button

            const int buttonWidth = 212;
            const int buttomHeight = 40;

            _craftButton = new ButtonControl
            {
                Text = "Craft",
                Bounds = new UniRectangle(340, 300, buttonWidth, buttomHeight)
            };
            
            Children.Add(_craftButton);
        }

        public void Update()
        {
            RecipesListOnSelectionChanged(null, null);
        }

        private void RecipesListOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (_recipesList.SelectedItem == null)
                return;
            
            var recipe = (Recipe)_recipesList.SelectedItem;

            var bp = _conf.BluePrints[recipe.ResultBlueprintId];

            var voxelEntity = bp as IVoxelEntity;

            if (voxelEntity != null)
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
                    var haveItems = _player.PlayerCharacter.Slots().Where(s => s.Item.BluePrintId == bpId).Sum(s => s.ItemsCount);

                    if (haveItems < needItems)
                        _canCraft = false;

                    cell.Slot = new ContainedSlot
                        {
                            Item = (IItem)_player.PlayerCharacter.EntityFactory.CreateFromBluePrint(bpId)
                        };

                    cell.CountString = string.Format("{0} / {1}", needItems, haveItems);
                    
                    var offsetX = ( _ingredientsRect.Width - recipe.Ingredients.Count * 50 ) / 2;
                    cell.Bounds = new UniRectangle(_ingredientsRect.X + offsetX + i * 50, _ingredientsRect.Y, 42, 42);
                }
            }
        }
    }
}
