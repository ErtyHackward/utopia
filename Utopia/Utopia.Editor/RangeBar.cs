using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace Utopia.Editor
{
    public partial class RangeBar : UserControl
    {
        public struct Range
        {
            public double Size;
            public Color Color;
            public string Name;
        }

        public List<RangeBar.Range> Ranges = new List<Range>();

        public RangeBar()
        {
            InitializeComponent();

            Ranges.Add(new Range() { Name = "TEST", Color = Color.Red, Size = 0.1 });
            Ranges.Add(new Range() { Name = "TEST2", Color = Color.Yellow, Size = 0.2 });
            Ranges.Add(new Range() { Name = "TEST2", Color = Color.Green, Size = 0.7 });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Calling the base class OnPaint
            base.OnPaint(e);
            // Create two semi-transparent colors
            //Color c1 = Color.Blue;
            //Color c2 = Color.Red;
            //Brush b = new LinearGradientBrush(ClientRectangle, c1, c2, 10);
            //e.Graphics.FillRectangle(b, ClientRectangle);
            //b.Dispose();

            //Draw a line
            DrawLine(e);
        }

        //Graphical component drawing
        private void DrawLine(PaintEventArgs e)
        {
            int lineYPosi = this.Height / 2;

            Brush lineBrush = new SolidBrush(Color.Black);
            Pen linePen = new Pen(lineBrush);
            e.Graphics.DrawLine(linePen, 0, lineYPosi, this.Width, this.Height / 2);

            e.Graphics.DrawLine(linePen, 0, lineYPosi, 0, (lineYPosi) - 20);
            e.Graphics.DrawLine(linePen, this.Width - 1, lineYPosi, this.Width - 1, (lineYPosi) - 20);

            int RangeFromX = 1;
            int RangeToX = 0;
            foreach (var range in Ranges)
            {
                RangeToX = (int)(RangeFromX + ((range.Size * this.Width)));

                //Draw Colored Rectangle
                Rectangle ColoredSurface = new Rectangle(RangeFromX, lineYPosi - 10, RangeToX, 10);
                Brush bColor = new SolidBrush(range.Color);
                e.Graphics.FillRectangle(bColor, ColoredSurface);
                bColor.Dispose();
                RangeFromX = RangeToX;
            }

            RangeFromX = 1;
            RangeToX = 0;
            int nbrCarretToDraw = Ranges.Count - 1;
            foreach (var range in Ranges)
            {
                RangeToX = (int)(RangeFromX + ((range.Size * this.Width)));

                //Draw Carret
                if (nbrCarretToDraw > 0 && VisualStyleRenderer.IsElementDefined(VisualStyleElement.TrackBar.ThumbBottom.Normal))
                {
                    VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.TrackBar.ThumbBottom.Normal);
                    Rectangle surface = new Rectangle(RangeToX - 10, lineYPosi - 18, 20, 20);
                    renderer.DrawBackground(e.Graphics, surface);
                }

                nbrCarretToDraw--;
                RangeFromX = RangeToX;
            }


            lineBrush.Dispose();
            linePen.Dispose();
        }

    }
}
