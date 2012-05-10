using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Samples;
using S33M3_Resources.Structs;
using SharpDX.Windows;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.Sampler;
using S33M3Resources.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using Utopia.Shared.World.Processors.Utopia;

namespace NoiseVisualisator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Random _rnd;
        private INoise NoiseComposition(bool is2DRenderRequest)
        {

            long from = Stopwatch.GetTimestamp();
            UtopiaProcessor processor = new UtopiaProcessor(new Utopia.Shared.World.WorldParameters() { SeedName = "test", SeaLevel =64, WorldName = "test" });
            label4.Text = ((Stopwatch.GetTimestamp() - from) / (double)Stopwatch.Frequency * 1000.0).ToString();

            Gradient ground_gradient;
            if (is2DRenderRequest)
            {
                ground_gradient = new Gradient(0, 0, 1, 0);
            }
            else
            {
                ground_gradient = new Gradient(0, 0, 0.42, 0);
            }

            var test = new UncommonCubeDistri(123456, ground_gradient); //, ground_gradient);

            return test.GetLandFormFct();

            return processor.CreateLandFormFct(ground_gradient);
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            OffsetX = 0;

            double thresholdFrom = double.Parse(thresholdValue.Text.Replace('.', ','));
            double thresholdTo = double.Parse(txtBelow.Text.Replace('.', ','));
            GameRender render = new GameRender(new System.Drawing.Size(1024, 768), "3D Noise visualisator !", NoiseComposition(false), thresholdFrom, thresholdTo, withBelow.CheckState == CheckState.Checked);
            render.Run();
            render.Dispose();
        }

        private void bt2DRender_Click(object sender, EventArgs e)
        {
            INoise noise = NoiseComposition(true);
            InitNoise(noise, new Range(0, 1));
        }

        int OffsetX;
        INoise2 workingNoise;
        private void InitNoise(INoise2 noise, Range outPutRange)
        {
            if (noise != null) workingNoise = noise;
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            float xl, yl;

            int w = pictureBox1.Width;
            int h = pictureBox1.Height;

            long from = Stopwatch.GetTimestamp();

            double[,] noiseData = NoiseSampler.NoiseSampling(new Vector2I(w ,h ),
                                                            0 + OffsetX, 3 + OffsetX, w,
                                                            0, 1, h, workingNoise);
            lblGenerationTime.Text = ((Stopwatch.GetTimestamp() - from) / (double)Stopwatch.Frequency * 1000.0).ToString();

            double min = double.MaxValue;
            double max = double.MinValue;

            int i = 0;
            double thresholdFrom = double.Parse(thresholdValue.Text.Replace('.', ','));
            double thresholdTo = double.Parse(txtBelow.Text.Replace('.', ','));

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    xl = MathHelper.FullLerp(0, 3, 0, w, x + OffsetX);
                    yl = MathHelper.FullLerp(0, 1, 0, h, y);

                    var val = noiseData[i, 0];

                    if (val < min) min = val;
                    if (val > max) max = val;

                    if (withThresHold.Checked)
                    {
                        if (withBelow.CheckState == CheckState.Checked)
                        {
                            if (val > thresholdFrom && val < thresholdTo) val = 1.0; else val = 0.0;
                        }
                        else
                        {
                            if (val > thresholdFrom) val = 1.0; else val = 0.0;
                        }
                    }

                    var col = MathHelper.FullLerp(0, 255, outPutRange.Min, outPutRange.Max, val, true);

                    bmp.SetPixel(x, h - y - 1, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    i++;
                }
            }

            Console.WriteLine("Min : " + min + " max : " + max);

            pictureBox1.Image = bmp;
        }

        private void forward_Click(object sender, EventArgs e)
        {
            OffsetX += 1000;
            InitNoise(null, new Range(0, 1));
        }

        private void backward_Click(object sender, EventArgs e)
        {
            OffsetX -= 1000;
            InitNoise(null, new Range(0, 1));
        }
    }
}
