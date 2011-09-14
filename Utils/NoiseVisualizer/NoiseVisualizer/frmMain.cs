using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3Engines.Shared.Math.Noises;
using S33M3Engines.Shared.Math;

namespace NoiseVisualizer
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Random r = new Random((int)numericUpDown3.Value); //Assign Seed to random generator
            var noise = new SimplexNoise(r);
            noise.SetParameters((double)numericUpDown5.Value, (SimplexNoise.InflectionMode)Enum.Parse(typeof(SimplexNoise.InflectionMode), (string)(listBox1.SelectedItem)), SimplexNoise.ResultScale.ZeroToOne);

            Point center = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            var maxLen = Length(center, new Point(pictureBox1.Width / 2, 0)) * 1.4;

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var val = noise.GetNoise2DValue(x, y, (int)numericUpDown4.Value, (double)numericUpDown6.Value);
                    Color color;
                    double colorcomp;
                    if (checkBox2.Checked)
                    {
                        if (checkBox1.Checked)
                        {
                            if (MathHelper.FullLerp(0, 1, val, true) > (double)numericUpDown1.Value) color = Color.White;
                            else color = Color.Black;
                        }
                        else
                        {
                            if (MathHelper.FullLerp(0, 1, val, true) < (double)numericUpDown1.Value) color = Color.White;
                            else color = Color.Black;
                        }
                    }
                    else
                    {
                        colorcomp = MathHelper.FullLerp(0, 255, val, true); // Make the noise result between 0 and 255.
                        color = Color.FromArgb((byte)colorcomp, (byte)colorcomp, (byte)colorcomp);
                    }

                    bmp.SetPixel(x, y, color);
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
    }
}
