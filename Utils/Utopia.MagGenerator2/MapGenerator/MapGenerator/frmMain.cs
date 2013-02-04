using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using S33M3CoreComponents.Maths.Noises;

namespace MapGenerator
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Random r = new Random((int)numericUpDown3.Value);
            var noise = new SimplexNoise(r);
            noise.SetParameters(0.002,SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

            Point center = new Point(pictureBox1.Width/2, pictureBox1.Height/2);
            var maxLen = Length(center, new Point(pictureBox1.Width / 2,0))* 1.4 ;

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var val = noise.GetNoise2DValue(x, y, 4, 0.8);

                    var col = 255 / val.MaxValue * val.Value - (255 / 2);

                    var minVal =255 - 255 * (Length(new Point(x, y), center) / maxLen);

                    col = col + minVal;
                    if (col > 255) col = 255;
                    if (col < 0) col = 0;

                    if (col > 127)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)col));
                    }
                    else
                    {
                        bmp.SetPixel(x,y, Color.Black);
                    }

                    //if (col > 127)
                    //{
                    //    //earth
                    //    bmp.SetPixel(x, y, Color.FromArgb((byte)(100 - col), (byte)(80 - col), (byte)(50 - col)));
                    //}
                    //else bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)255));

                }
            }

            pictureBox1.Image = bmp;


        }

        private double Length(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // starting rolling down
            
            PropagateWater(e.Location, 1000);
            pictureBox1.Refresh();
        }

        private void PropagateWater(Point p, int power)
        {
            var bmp = (pictureBox1.Image as Bitmap);
            Color c = bmp.GetPixel(p.X, p.Y);
            if (c.B != 0 && power > 0)
            {
                bmp.SetPixel(p.X, p.Y, Color.Blue);

                var lowest = bmp.GetPixel(p.X - 1, p.Y - 1).B;
                var point = new Point(p.X - 1, p.Y - 1);
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        var col = bmp.GetPixel(p.X + x, p.Y + y).B;
                        if (col < lowest)
                        {
                            lowest = col;
                            point = new Point(p.X + x, p.Y + y);
                        }
                    }
                }

                //pictureBox1.Refresh();
                //Application.DoEvents();
                //Thread.Sleep(100);
                PropagateWater(point, power-1);
            }
        }

    }
}
