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

        private int Octave
        {
            get
            {
                return int.Parse(txtOctave.Text); 
            }
        }

        private double Freq
        {
            get
            {
                return double.Parse(txtfreq.Text);
            }
        }

        private int Seed
        {
            get
            {
                return int.Parse(txtSeed.Text);
            }
        }

        Random _rnd;
        private INoise NoiseComposition(bool is2DRenderRequest)
        {
            _rnd = new Random("test".GetHashCode());

            //INoise ground_gradient = new Gradient(0, 0, 1, 0);
            //INoise ground_gradient_cache = new Cache(ground_gradient);

            //INoise lowLandNoise = CreateLowLandNoise(ground_gradient_cache);
            //INoise HighLandNoise = CreateHighLandNoise(ground_gradient_cache);
            //INoise MontainNoise = CreateMontainNoise(ground_gradient_cache);

            //INoise terrain_type_fractal = new FractalFbm(new Perlin(5632), 3, 0.5, enuBaseNoiseRange.ZeroToOne);
            //INoise terrain_type_cache = new Cache(terrain_type_fractal);

            //INoise highland_mountain_select = new Select(HighLandNoise, MontainNoise, terrain_type_cache, 0.75, 0.15);
            //INoise highland_lowland_select = new Select(lowLandNoise, highland_mountain_select, terrain_type_cache, 0.45, 0.15);

            //INoise ground_solid = new Select(0, 1, lowLandNoise, 0.5);

            return CreateLandFormFct(is2DRenderRequest);
        }

        private INoise CreateLandFormFct(bool is2DRenderRequest)
        {
            Gradient ground_gradient;
            if (is2DRenderRequest)
            {
                ground_gradient = new Gradient(0, 0, 1, 0);
            }
            else
            {
                ground_gradient = new Gradient(0, 0, 0.42, 0);
            }

            Cache<Gradient> ground_gradient_cache = new Cache<Gradient>(ground_gradient);

            //Get Basic landscape forms
            ITerrainGenerator plain = new Plain(_rnd.Next(), ground_gradient_cache);
            ITerrainGenerator midland = new Midland(_rnd.Next(), ground_gradient_cache);
            ITerrainGenerator montain = new Montain(_rnd.Next(), ground_gradient_cache);

            //Will be used as map for blending terrain type
            ITerrainGenerator terrainType = new TerrainType(_rnd.Next());

            INoise plainFct = plain.GetLandFormFct();
            INoise midlandFct = midland.GetLandFormFct();
            INoise montainFct = montain.GetLandFormFct();
            INoise terrainTypeFct = terrainType.GetLandFormFct();

            //Console.WriteLine(NoiseAnalyse.Analyse((INoise2)terrainTypeFct, 1000000));

            //0.0 => 0.3 Montains
            //0.3 => 0.6 MidLand
            //0.6 => 1 Plain
            INoise mountain_midland_select = new Select(montainFct, midlandFct, terrainTypeFct, 0.45, 0.15);
            INoise midland_plain_select = new Select(mountain_midland_select, plainFct, terrainTypeFct, 0.65, 0.10);

            return midland_plain_select;
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            OffsetX = 0;
            GameRender render = new GameRender(new System.Drawing.Size(1024, 768), "3D Noise visualisator !", NoiseComposition(false));
            render.Run();
            render.Dispose();
        }

        private void bt2DRender_Click(object sender, EventArgs e)
        {
            INoise noise = NoiseComposition(true);
            if (withThresHold.Checked)
            {
                noise = new Select(0, 1, noise, 0.5); //Apply the same
            }
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

            double[] noiseData = NoiseSampler.NoiseSampling(workingNoise, new Vector2I(w,h),
                                                            0 + OffsetX, 3 + OffsetX, w,
                                                            0, 1, h);

            lblGenerationTime.Text = ((Stopwatch.GetTimestamp() - from) / (double)Stopwatch.Frequency * 1000.0).ToString();

            double MinNoise = double.MaxValue;
            double MaxNoise = double.MinValue;

            int i = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    xl = MathHelper.FullLerp(0, 3, 0, w, x + OffsetX);
                    yl = MathHelper.FullLerp(0, 1, 0, h, y);

                    var val = noiseData[i];

                    if (val < MinNoise) MinNoise = val;
                    if (val > MaxNoise) MaxNoise = val;

                    var col = MathHelper.FullLerp(0, 255, outPutRange.Min, outPutRange.Max, val, true);

                    bmp.SetPixel(x, h - y - 1, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    i++;
                }
            }

            Console.WriteLine("Min Value : " + MinNoise.ToString() + " Max Value : " + MaxNoise.ToString());

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
