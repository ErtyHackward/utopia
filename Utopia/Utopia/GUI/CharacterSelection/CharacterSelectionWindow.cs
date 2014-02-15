using System;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using Utopia.Entities;
using Utopia.GUI.Crafting;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Configuration;

namespace Utopia.GUI.CharacterSelection
{
    public class CharacterSelectionWindow : WindowControl
    {
        private readonly WorldConfiguration _conf;
        private IconFactory _iconFactory;

        protected ListControl _classList;
        private ModelControl _characterModel;
        protected ButtonControl _selectButton;

        public ListControl ClassList
        {
            get { return _classList; }
        }

        public ButtonControl SelectionButton
        {
            get { return _selectButton; }
        }

        public HLSLVoxelModel VoxelEffect
        {
            get { return _characterModel.VoxelEffect; }
            set { _characterModel.VoxelEffect = value; }
        }

        public ModelControl CharacterModel
        {
            get { return _characterModel; }
        }

        public CharacterSelectionWindow(WorldConfiguration conf, IconFactory iconFactory)
        {
            _conf = conf;
            _iconFactory = iconFactory;
            Bounds.Size = new UniVector(627, 388);
        }

        protected virtual void InitializeComponent()
        {
            _classList = new ListControl { SelectionMode = ListSelectionMode.Single };
            _classList.Bounds = new UniRectangle(20, 50, 200, 300);
            _classList.SelectionChanged += ClassListOnSelectionChanged;

            Children.Add(_classList);

            foreach (var charClass in _conf.CharacterClasses)
            {
                _classList.Items.Add(charClass);
            }

            _characterModel = new ModelControl(_iconFactory.VoxelModelManager)
            {
                Bounds = new UniRectangle(300, 30, 230, 270),
                AlterTransform = Matrix.Identity
            };

            Children.Add(_characterModel);

            const int buttonWidth = 212;
            const int buttomHeight = 40;

            _selectButton = new ButtonControl
            {
                Text = "Select",
                Bounds = new UniRectangle(340, 300, buttonWidth, buttomHeight)
            };

            Children.Add(_selectButton);
        }

        public void Update()
        {
            ClassListOnSelectionChanged(null, null);
        }

        private void ClassListOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (_classList.SelectedItem == null)
                return;

            var item = (CharacterClassItem)_classList.SelectedItem;
            _characterModel.SetModel(item.ModelName);
        }
    }
}
