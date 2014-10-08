using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Inputs;


namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents a toolbar control
    /// </summary>
    public class ToolBarUi : ContainerControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly PlayerEntityManager _playerManager;
        private readonly EntityFactory _factory;
        const int ButtonSize = 46;

        protected readonly List<InventoryCell> _toolbarSlots;

        public List<InventoryCell> Slots {
            get { return _toolbarSlots; }
        }

        /// <summary>
        /// Occurs when some slot get clicked
        /// </summary>
        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotClicked;

        protected void OnSlotClicked(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotMouseUp;

        protected void OnSlotMouseUp(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotMouseUp;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotEnter;

        protected void OnSlotEnter(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotEnter;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotLeave;

        protected void OnSlotLeave(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotLeave;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotChanged;

        protected virtual void OnSlotChanged(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotChanged;
            if (handler != null) handler(this, e);
        }


        public ToolBarUi(PlayerEntityManager player, IconFactory iconFactory, InputsManager inputManager, EntityFactory factory)
        {
            _playerManager = player;
            _factory = factory;

            Name = "Toolbar";

            int nbrButton = _playerManager.PlayerCharacter.Toolbar.Count;
            _toolbarSlots = new List<InventoryCell>(nbrButton);

            //float fromX = ((bounds.Right.Offset - bounds.Left.Offset) - (ButtonSize * (nbrButton))) / 2;

            for (int x = 0; x < nbrButton; x++)
            {

                var btn = new InventoryCell(null, iconFactory, new Vector2I(0, x), inputManager)
                              {
                                  //Bounds = new UniRectangle(fromX + (x * ButtonSize), 0, ButtonSize, ButtonSize)
                                  DrawCellBackground = false
                              };
                btn.MouseDown  += BtnMouseDown;
                btn.MouseUp    += btn_MouseUp;
                btn.MouseEnter += btn_MouseEnter;
                btn.MouseLeave += btn_MouseLeave;

                var bluePrintId = _playerManager.PlayerCharacter.Toolbar[x];

                if (bluePrintId != 0)
                {
                    btn.Slot = _playerManager.PlayerCharacter.FindSlot(s => s.Item.BluePrintId == bluePrintId);

                    if (btn.Slot == null)
                    {
                        try
                        {
                            btn.Slot = new ContainedSlot
                            {
                                Item = (IItem)_factory.CreateFromBluePrint(bluePrintId)
                            };
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            _playerManager.PlayerCharacter.Toolbar[x] = 0;
                            logger.Error("Unable to create entity from Id = {0}. Configuration was probably changed.", bluePrintId);
                        }
                    }
                }

                _toolbarSlots.Add(btn);

                Children.Add(btn);
            }


        }

        void btn_MouseLeave(object sender, EventArgs e)
        {
            OnSlotLeave(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        void btn_MouseEnter(object sender, EventArgs e)
        {
            OnSlotEnter(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        void btn_MouseUp(object sender, MouseDownEventArgs e)
        {
            OnSlotMouseUp(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        void BtnMouseDown(object sender, MouseDownEventArgs e)
        {
            OnSlotClicked(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        public void SetSlot(int i, ContainedSlot slot)
        {
            _toolbarSlots[i].Slot = slot;

            OnSlotChanged(new InventoryWindowCellMouseEventArgs { Cell = _toolbarSlots[i] });
        }

        public void Resized()
        {
            float fromX = ((Bounds.Right.Offset - Bounds.Left.Offset) - (ButtonSize * _toolbarSlots.Count)) / 2;
            int btNbr = 0;
            foreach (var bt in _toolbarSlots)
            {
                bt.Bounds = new UniRectangle(fromX + (btNbr * ButtonSize), 0, ButtonSize, ButtonSize);

                btNbr++;
            }
        }

        public void Update(GameTime gameTime)
        {
            // remove selection from every cell
            for (int i = 0; i < _toolbarSlots.Count; i++)
            {
                _toolbarSlots[i].IsCellSelected = false;

                if (_playerManager.PlayerCharacter.Toolbar[i] != 0)
                {
                    _toolbarSlots[i].IsDisabledCell =
                        _playerManager.PlayerCharacter.FindSlot(s => s.Item.BluePrintId == _playerManager.PlayerCharacter.Toolbar[i]) == null;

                    if (_toolbarSlots[i].IsDisabledCell)
                        _toolbarSlots[i].Slot.ItemsCount = 1;
                    else
                        _toolbarSlots[i].Slot.ItemsCount = _playerManager.PlayerCharacter.Slots().Where(s => s.Item.BluePrintId == _playerManager.PlayerCharacter.Toolbar[i]).Sum(x => x.ItemsCount);
                }
            }

            if (_playerManager.PlayerCharacter.Equipment.RightTool == null)
            {
                return;
            }

            for (int i = 0; i < _toolbarSlots.Count; i++)
            {
                if (_playerManager.PlayerCharacter.Toolbar[i] != 0)
                {
                    _toolbarSlots[i].IsCellSelected = _playerManager.PlayerCharacter.Equipment.RightTool.BluePrintId == _playerManager.PlayerCharacter.Toolbar[i];
                }
                else
                    _toolbarSlots[i].IsCellSelected = false;
            }
        }

    }
}
