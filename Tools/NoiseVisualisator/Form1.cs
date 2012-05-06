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
using Utopia.Shared.World.Processors.Utopia.LandformFct.Plains;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

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
        private INoise NoiseComposition()
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
            return CreateLandFormFct();
        }

        private INoise CreateLandFormFct()
        {
            INoise ground_gradient = new Gradient(0, 0, 1, 0);
            INoise ground_gradient_cache = new Cache(ground_gradient);

            ILandform plain = new Plain(_rnd.Next(), ground_gradient_cache);

            INoise landFormFct = plain.GetLandFormFct();

            return landFormFct;
        }

        private INoise CreateLowLandNoise(INoise ground_gradient)
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise lowland_shape_fractal = new FractalFbm(new Perlin(1234), 2, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result
            INoise lowland_scale = new ScaleOffset(lowland_shape_fractal, 0.2, 0.25);
            //Remove Y value from impacting the result (Fixed to 0) = removing one dimension to the generator noise
            INoise lowland_y_scale = new ScaleDomain(lowland_scale, 1, 0);
            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise lowland_terrain = new Turbulence(ground_gradient, 0, lowland_y_scale);

            return lowland_terrain;
        }

        private INoise CreateHighLandNoise(INoise ground_gradient)
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise Highland_shape_fractal = new FractalRidgedMulti(new Perlin(15905), 2, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result
            INoise Highland_scale = new ScaleOffset(Highland_shape_fractal, 0.25, 0);
            //Remove Y value from impacting the result (Fixed to 0) = removing one dimension to the generator noise
            INoise Highland_y_scale = new ScaleDomain(Highland_scale, 1, 0);
            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise Highland_terrain = new Turbulence(ground_gradient, 0, Highland_y_scale);

            return Highland_terrain;
        }

        private INoise CreateMontainNoise(INoise ground_gradient)
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise Montain_shape_fractal = new FractalBillow(new Perlin(5620), 4, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result
            INoise Montain_scale = new ScaleOffset(Montain_shape_fractal, 0.75, 0.0);
            //Remove Y value from impacting the result (Fixed to 0) = removing one dimension to the generator noise
            INoise Montain_y_scale = new ScaleDomain(Montain_scale, 1, 0.1);
            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise Montain_terrain = new Turbulence(ground_gradient, 0, Montain_y_scale);

            return Montain_terrain;
        }


        private void btStart_Click(object sender, EventArgs e)
        {
            OffsetX = 0;
            GameRender render = new GameRender(new System.Drawing.Size(1024, 768), "3D Noise visualisator !", NoiseComposition());
            render.Run();
            render.Dispose();
        }

        private void bt2DRender_Click(object sender, EventArgs e)
        {
            INoise noise = NoiseComposition();
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
                                                            0, 3, w, 
                                                            0, 1, h);

            lblGenerationTime.Text = ((Stopwatch.GetTimestamp() - from) / (double)Stopwatch.Frequency * 1000.0).ToString();


            int i = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    xl = MathHelper.FullLerp(0, 3, 0, w, x + OffsetX);
                    yl = MathHelper.FullLerp(0, 1, 0, h, y);

                    var val = noiseData[i];

                    var col = MathHelper.FullLerp(0, 255, outPutRange.Min, outPutRange.Max, val, true);

                    bmp.SetPixel(x, h - y - 1, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    i++;
                }
            }
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
