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
        public class Range
        {
            public double Size { get; set; }
            public Color Color { get; set; }
            public string Name { get; set; }
        }

        public enum ThumbState
        {
            Normal,
            MouseOver,
            MouseDown
        }

        public class RangeThumb
        {
            public double PositionX { get; set; }
            public Range LeftLinkedRange { get; set; }
            public Range RightLinkedRange { get; set; }
            public ThumbState State { get; set; }
            public int Position;
        }

        private BindingList<RangeBar.Range> _ranges = new BindingList<RangeBar.Range>();
        private List<RangeBar.RangeThumb> _rangeThumbs = new List<RangeBar.RangeThumb>();

        [Category("Data"),  Description("Specifies the alignment of text.")]
        public BindingList<RangeBar.Range> Ranges
        {
            get { return _ranges; }
        }

        public RangeBar()
        {
            InitializeComponent();
            _ranges.ListChanged += ranges_ListChanged;
            this.MouseMove += RangeBar_MouseMove;
            this.MouseDown += RangeBar_MouseDown;
            this.MouseUp += RangeBar_MouseUp;

            _ranges.Add(new Range() { Name = "Flat", Color = Color.Red, Size = 0.1 });
            _ranges.Add(new Range() { Name = "Plain", Color = Color.Yellow, Size = 0.2 });
            _ranges.Add(new Range() { Name = "Hill", Color = Color.Green, Size = 0.5 });
            _ranges.Add(new Range() { Name = "Montain", Color = Color.Blue, Size = 0.2 });

        }

        void RangeBar_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (var thumb in _rangeThumbs.Where(x => x.State == ThumbState.MouseDown))
            {
                int lineYPosi = this.Height / 2;
                Rectangle thumbArea = new Rectangle((int)(thumb.PositionX * this.Width) - 10, lineYPosi - 18, 20, 20);
                if (thumbArea.Contains(e.Location))
                {
                    thumb.State = ThumbState.MouseOver;
                    this.Refresh();
                }
                else
                {
                    thumb.State = ThumbState.Normal;
                    this.Refresh();
                }
            }
        }

        void RangeBar_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var thumb in _rangeThumbs.Where(x => x.State == ThumbState.MouseOver))
            {
                thumb.State = ThumbState.MouseDown;
                this.Refresh();
            }
        }

        void RangeBar_MouseMove(object sender, MouseEventArgs e)
        {
            int lineYPosi = this.Height / 2;


            var thumbMouseDown = _rangeThumbs.FirstOrDefault(x => x.State == ThumbState.MouseDown);
            if (thumbMouseDown != null)
            {
                //Compute the new Thumb Position
                double newX = (double)(e.X / (double)this.Width);
                bool validatednewX = true;
                //Validate newX value;
                //1) Cannot go over below next thumb
                if (validatednewX && thumbMouseDown.Position > 0) //Check with left thumb
                {
                    if (_rangeThumbs[thumbMouseDown.Position - 1].PositionX > newX) validatednewX = false;
                }

                if (validatednewX && thumbMouseDown.Position < _rangeThumbs.Count - 1) //Check Right thumb
                {
                    if (_rangeThumbs[thumbMouseDown.Position + 1].PositionX < newX) validatednewX = false;
                }
                //2) must be in range [0;1]
                if (validatednewX)
                {
                    if (newX < 0 || newX > 1) validatednewX = false;
                }

                if(validatednewX)
                {
                    double differenceMove = thumbMouseDown.PositionX - newX;
                    thumbMouseDown.PositionX = newX;

                    //Recompute range
                    thumbMouseDown.LeftLinkedRange.Size -= differenceMove;
                    thumbMouseDown.RightLinkedRange.Size += differenceMove;

                    this.Refresh();
                }
            }
            else
            {
                foreach (var thumb in _rangeThumbs)
                {

                    Rectangle thumbArea = new Rectangle((int)(thumb.PositionX * this.Width) - 10, lineYPosi - 18, 20, 20);
                    if (thumbArea.Contains(e.Location))
                    {
                        if (thumb.State != ThumbState.MouseOver)
                        {
                            thumb.State = ThumbState.MouseOver;
                            this.Refresh();
                        }
                    }
                    else
                    {
                        if (thumb.State != ThumbState.Normal)
                        {
                            thumb.State = ThumbState.Normal;
                            this.Refresh();
                        }
                    }
                }
            }
        }

        private void ranges_ListChanged(object sender, ListChangedEventArgs e)
        {
            _rangeThumbs.Clear();

            if (Ranges.Count <= 1) return;

            Range leftRange = null;
            Range rightRange = null;

            double RangeToX = 0;
            int ArrayId = 0;
            foreach (var range in Ranges)
            {
                rightRange = range;
                if (leftRange == null)
                {
                    leftRange = rightRange;
                    RangeToX += leftRange.Size;
                    continue;
                }

                _rangeThumbs.Add(new RangeThumb()
                {
                    LeftLinkedRange = leftRange,
                    RightLinkedRange = rightRange,
                    PositionX = RangeToX,
                    State = ThumbState.Normal,
                    Position = ArrayId
                });

                leftRange = rightRange;
                RangeToX += leftRange.Size;
                ArrayId++;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Calling the base class OnPaint
            base.OnPaint(e);

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
            foreach (var range in _ranges)
            {
                RangeToX = (int)(RangeFromX + ((range.Size * this.Width)));

                //Draw Colored Rectangle
                Rectangle ColoredSurface = new Rectangle(RangeFromX, lineYPosi - 10, RangeToX - RangeFromX, 10);
                Brush bColor = new SolidBrush(range.Color);
                e.Graphics.FillRectangle(bColor, ColoredSurface);
                bColor.Dispose();

                FontFamily ff = new FontFamily("Times New Roman");
                Font fnt = new Font(ff, 12, GraphicsUnit.Pixel);
                Brush fontBursh = new SolidBrush(Color.Black);
                //Draw Name below color Carret
                e.Graphics.DrawString(range.Name, fnt, fontBursh, ColoredSurface.X + ((RangeToX - RangeFromX) / 2) - (e.Graphics.MeasureString(range.Name, fnt).Width / 2), ColoredSurface.Y + 15);

                //Draw % above color Carret
                string Pourc = (range.Size * 100).ToString("0") + "%";
                e.Graphics.DrawString(Pourc, fnt, fontBursh, ColoredSurface.X + ((RangeToX - RangeFromX) / 2) - (e.Graphics.MeasureString(Pourc, fnt).Width / 2), ColoredSurface.Y - 25);

                fontBursh.Dispose();
                fnt.Dispose();

                RangeFromX += RangeToX - RangeFromX;
            }

            foreach (var thumb in _rangeThumbs)
            {
                VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.TrackBar.ThumbBottom.Normal);

                switch (thumb.State)
                {
                    case ThumbState.Normal:
                        if (VisualStyleRenderer.IsElementDefined(VisualStyleElement.TrackBar.ThumbBottom.Normal))
                        {
                            renderer = new VisualStyleRenderer(VisualStyleElement.TrackBar.ThumbBottom.Normal);
                        }
                        break;
                    case ThumbState.MouseOver:
                        if (VisualStyleRenderer.IsElementDefined(VisualStyleElement.TrackBar.ThumbBottom.Hot))
                        {
                            renderer = new VisualStyleRenderer(VisualStyleElement.TrackBar.ThumbBottom.Hot);
                        }
                        break;
                    case ThumbState.MouseDown:
                        if (VisualStyleRenderer.IsElementDefined(VisualStyleElement.TrackBar.ThumbBottom.Pressed))
                        {
                            renderer = new VisualStyleRenderer(VisualStyleElement.TrackBar.ThumbBottom.Pressed);
                        }
                        break;
                    default:
                        break;
                }

                Rectangle surface = new Rectangle((int)(thumb.PositionX * this.Width) - 8, lineYPosi - 18, 20, 20);
                renderer.DrawBackground(e.Graphics, surface);

                Rectangle surface2 = new Rectangle((int)(thumb.PositionX * this.Width) - 10, lineYPosi - 18, 20, 20);
                e.Graphics.DrawLine(linePen, surface2.Location.X + 10 - 10, surface2.Location.Y, surface2.Location.X + 10 + 10, surface2.Location.Y);
                //e.Graphics.DrawLine(linePen, surface2.Location.X + 10, surface2.Location.Y, surface2.Location.X + 10, surface2.Location.Y + 18);
            }

            lineBrush.Dispose();
            linePen.Dispose();
        }



    }
}
