using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Tools
{
    public partial class ContainerEditor : Control
    {
        private readonly ContextMenuStrip _contextMenu;

        private SlotContainer<BlueprintSlot> _content;
        private Point _gridOffset;
        private readonly Size _cellSize = new Size(42, 42);
        private Vector2I _currentCell;

        /// <summary>
        /// Realm configuration that contains all possible entities
        /// </summary>
        public WorldConfiguration Configuration { get; set; }
        
        /// <summary>
        /// Gets or sets current editor content
        /// </summary>
        public SlotContainer<BlueprintSlot> Content
        {
            get { return _content; }
            set { 
                _content = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Icons cache to draw at the grid
        /// </summary>
        public Dictionary<string, Image> Icons { get; set; }

        public event EventHandler<ItemNeededEventArgs> ItemNeeded;

        private void OnItemNeeded(ItemNeededEventArgs e)
        {
            var handler = ItemNeeded;
            if (handler != null) handler(this, e);
        }

        public ContainerEditor()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Add...").Click += ContainerEditorAdd;
            _contextMenu.Items.Add("Delete").Click += ContainerEditorRemove;

        }

        private void ContainerEditorAdd(object sender, EventArgs e)
        {
            var ea = new ItemNeededEventArgs();

            OnItemNeeded(ea);

            if (ea.BlueprintId != 0)
            {
                var blueprintSlot = new BlueprintSlot { GridPosition = _currentCell, BlueprintId = ea.BlueprintId, ItemsCount = ea.Count };

                Content.WriteSlot(blueprintSlot);
                Invalidate();
            }

        }

        private void ContainerEditorRemove(object sender, EventArgs e)
        {
            Content.ClearSlot(_currentCell);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Right)
                return;

            if (e.X < _gridOffset.X || e.Y < _gridOffset.Y)
                return;

            _currentCell = new Vector2I((e.X - _gridOffset.X) / _cellSize.Width, (e.Y - _gridOffset.Y) / _cellSize.Height);

            var range = new Range2I(new Vector2I(), Content.GridSize);

            if (range.Contains(_currentCell))
            {
                var slot = Content.PeekSlot(_currentCell);

                _contextMenu.Items[1].Enabled = slot != null;

                _contextMenu.Show(this, e.Location);
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            rect.Height--;
            rect.Width--;
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rect);

            if (Content == null)
            {
                e.Graphics.DrawString("Set the content property!", Font, SystemBrushes.ControlText, ClientRectangle);
                return;
            }
            
            var gridSize = Content.GridSize;

            var gridSizePixels = new Size(gridSize.X * _cellSize.Width, gridSize.Y * _cellSize.Height);

            _gridOffset = new Point(ClientRectangle.Size - gridSizePixels);
            _gridOffset.X /= 2;
            _gridOffset.Y /= 2;
            

            // first draw the grid
            for (int x = 0; x < gridSize.X; x++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark, x * _cellSize.Width + _gridOffset.X, _gridOffset.Y, _gridOffset.X + x * _cellSize.Width, _gridOffset.Y + gridSizePixels.Height);
            }
            for (int y = 0; y < gridSize.Y; y++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark, _gridOffset.X, y * _cellSize.Height + _gridOffset.Y, gridSizePixels.Width + _gridOffset.X , _gridOffset.Y + y * _cellSize.Height);
            }

            e.Graphics.DrawRectangle(SystemPens.ControlDark, _gridOffset.X, _gridOffset.Y, gridSizePixels.Width, gridSizePixels.Height);

            if (Configuration == null)
                return;

            // draw the icons
            if (Icons != null)
            {
                foreach (var blueprintSlot in Content)
                {
                    var position = new Point(blueprintSlot.GridPosition.X * _cellSize.Width, blueprintSlot.GridPosition.Y * _cellSize.Height);
                    position.Offset(_gridOffset);

                    Image icon = null;

                    if (blueprintSlot.BlueprintId < 256)
                    {
                        var profile = Configuration.CubeProfiles[blueprintSlot.BlueprintId];
                        if (profile != null)
                            Icons.TryGetValue("CubeResource_" + profile.Name, out icon);
                    }
                    else
                    {
                        Entity blueprintEntity;
                        if (Configuration.BluePrints.TryGetValue(blueprintSlot.BlueprintId, out blueprintEntity))
                        {
                            var item = blueprintEntity as IVoxelEntity;
                            if (item != null)
                                Icons.TryGetValue(item.ModelName, out icon);
                        }
                    }


                    if (icon == null)
                    {
                        e.Graphics.DrawString("No icon", Font, SystemBrushes.ControlText, position);
                    }
                    else
                    {
                        var drect = new Rectangle(position, _cellSize);

                        e.Graphics.DrawImage(icon, drect);

                        // draw count
                        var cntSize = e.Graphics.MeasureString(blueprintSlot.ItemsCount.ToString(), Font);

                        drect.X += drect.Width - (int) cntSize.Width;
                        drect.Y += drect.Height - (int) cntSize.Height;
                        drect.Size = cntSize.ToSize() + new Size(1, 1);
                        drect.Offset(-1, -1);
                        e.Graphics.FillRectangle(SystemBrushes.Control, drect);
                        e.Graphics.DrawString(blueprintSlot.ItemsCount.ToString(), Font, SystemBrushes.ControlText,
                                              drect);
                    }
                }
            }
        }
    }

    public class ItemNeededEventArgs : EventArgs
    {
        public ushort BlueprintId { get; set; }
        public int Count { get; set; }
    }
}
