using System.Collections.Generic;
using System.Linq;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.GUI.Crafting;
using Utopia.GUI.Inventory;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Realms.Client.Components.GUI
{
    public class SandboxToolBar : ToolBarUi
    {
        private readonly PlayerCharacter _player;
        private readonly GodEntity _godEntity;
        private readonly EntityFactory _factory;
        private readonly DynamicEntityManager _dynamicEntityManager;

        public override int DrawGroupId
        {
            get
            {
                return 1;
            }
        }

        readonly SpriteTexture _stBackground;
        readonly SpriteTexture _stToolbarSlot;
        readonly SpriteTexture _stToolbatSlotHover;

        private readonly List<LabelControl> _numbersLabels = new List<LabelControl>();
        private ModelControl _modelControl;

        public HLSLVoxelModel VoxelEffect
        {
            get { return _modelControl.VoxelEffect; }
            set { _modelControl.VoxelEffect = value; }
        }

        public List<LabelControl> NumbersLabels
        {
            get { return _numbersLabels; }
        }

        public SandboxToolBar(D3DEngine engine,
                              PlayerCharacter player, 
                              GodEntity godEntity, 
                              IconFactory iconFactory, 
                              InputsManager inputManager, 
                              EntityFactory factory, 
                              VoxelModelManager voxelModelManager,
                              DynamicEntityManager dynamicEntityManager
            ) : base(player, iconFactory, inputManager, factory)
        {
            _player = player;
            _godEntity = godEntity;
            _factory = factory;
            _dynamicEntityManager = dynamicEntityManager;
            _stBackground       = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_bg.png");
            _stToolbarSlot      = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot.png");
            _stToolbatSlotHover = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot_active.png");

            Background = _stBackground;
            DisplayBackground = true;

            Bounds = new UniRectangle(0, new UniScalar(0.8f, 0), new UniScalar(1, 0), new UniScalar(0.21f, 0));


            _modelControl = new ModelControl(voxelModelManager)
                                {
                                    Bounds = new UniRectangle(0, new UniScalar(-0.1f, 0), new UniScalar(0.2f, 0), new UniScalar(1.1f, 0)) 
                                };

            Children.Add(_modelControl);

            var stuffButton = new ButtonControl {
                Text = "Stuff",
                Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.05f, 0), new UniScalar(0, 32), new UniScalar(0, 32)) 
            };
            
            Children.Add(stuffButton);
            
            var panel = new Control();

            panel.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.3f, 0), new UniScalar(0.8f, 0), new UniScalar(0.7f, 0));

            Children.Add(panel);

            int buttonIndex = 0;

            var allButtons = factory.Config.BluePrints.Values.Where(e => e.ShowInToolbar).ToList();

            var sideOffset = 0.00f;

            var buttonWidth = (1f - sideOffset * 2) / 10;

            foreach (var entity in allButtons)
            {
                var button = new ButtonControl { 
                    Bounds = new UniRectangle(
                        new UniScalar(sideOffset + buttonIndex * buttonWidth, 0),
                        new UniScalar(sideOffset, 0),
                        new UniScalar(buttonWidth, 0),
                        new UniScalar(1f - sideOffset * 2, 0))
                };

                int arrayIndex;
                SpriteTexture texture;

                iconFactory.Lookup((IItem)entity, out texture, out arrayIndex);

                button.CusomImageLabel = texture;

                panel.Children.Add(button);
                buttonIndex++;
            }

            SlotChanged += SandboxToolBar_SlotChanged;
        }

        public void Update()
        {
            if (_godEntity.SelectedEntities.Count > 0)
            {
                var firstEntity = _godEntity.SelectedEntities.First();
                var entity = firstEntity.Resolve<IVoxelEntity>(_factory);
                _modelControl.SetModel(entity.ModelName);
            }
            else
            {
                _modelControl.SetModel(null);
            }
        }

        void SandboxToolBar_SlotChanged(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (!DisplayBackground)
            {
                var index = _toolbarSlots.IndexOf(e.Cell);
                _numbersLabels[index].IsVisible = e.Cell.Slot != null;
            }
        }
    }

    public class SandboxToolBarRenderer : IFlatControlRenderer<SandboxToolBar>
    {
        public void Render(SandboxToolBar control, IFlatGuiGraphics graphics)
        {
            if (control.DisplayBackground)
            {
                var absoluteBounds = control.GetAbsoluteBounds();
                graphics.DrawElement("toolbar", ref absoluteBounds);
            }
        }
    }
}
