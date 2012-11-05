using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Tools
{
    public partial class ContainerEditor : Control
    {
        /// <summary>
        /// Realm configuration that contains all possible entities
        /// </summary>
        public RealmConfiguration Realm { get; set; }

        /// <summary>
        /// Gets or sets current editor content
        /// </summary>
        public SlotContainer<BlueprintSlot> Content { get; set; }

        /// <summary>
        /// Icons cache to draw at the grid
        /// </summary>
        public Dictionary<ushort, Image> Icons { get; set; }

        public ContainerEditor()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var cellSize = new Size(42, 42);

            var rect = ClientRectangle;
            rect.Inflate(-1, -1);
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rect);

            if (Content == null)
            {
                e.Graphics.DrawString("Set the content property!", Font, SystemBrushes.ControlText, ClientRectangle);
                return;
            }
            
            var gridSize = Content.GridSize;

            var gridSizePixels = new Vector2I(gridSize.X * cellSize.Width, gridSize.Y * cellSize.Height);

            // first draw the grid
            for (int x = 0; x < gridSize.X; x++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark, x, 0, x, gridSizePixels.Y);
            }
            for (int y = 0; y < gridSize.Y; y++)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark, 0, y, gridSizePixels.X , y);
            }

            // draw the icons
            if (Icons != null)
            {
                foreach (var blueprintSlot in Content)
                {
                    Image icon;
                    var position = new Point(blueprintSlot.GridPosition.X * cellSize.Width, blueprintSlot.GridPosition.Y * cellSize.Height);
                    if (!Icons.TryGetValue(blueprintSlot.BlueprintId, out icon))
                    {
                        e.Graphics.DrawString("No icon", Font, SystemBrushes.ControlText, position);
                        continue;
                    }

                    e.Graphics.DrawImage(icon, position);

                }
            }
        }
    }
}
