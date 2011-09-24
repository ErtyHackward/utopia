using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using S33M3Engines.Shared.Math.Noises;
using Utopia.MapGenerator.Properties;
using Utopia.Shared.World.PlanGenerator;

namespace Utopia.MapGenerator
{
    public partial class frmMain : Form
    {
        private WorldPlan _map;

        private Polygon _selectedPolygon;
        
        public frmMain()
        {
            InitializeComponent();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {


            _map = new WorldPlan(new GenerationParameters { 
                MapSize = new Size((int)numericUpDown1.Value,(int)numericUpDown2.Value), 
                CenterElevation = centerElevationCheck.Checked,
                ElevationSeed = (int)voronoiSeedNumeric.Value, 
                GridSeed = (int)voronoiSeedNumeric.Value, 
                PolygonsCount = (int)voronoiPolyNumeric.Value, 
                RelaxCount = 3});

            _map.RenderMapTemplate = Resources.seamap;
            _map.RenderContinentTemplate = Resources.brush;
            _map.RenderWavePatterns = new [] { Resources.wavePattern, Resources.wavePattern1, Resources.wavePattern2};
            _map.RenderForest = Resources.forest;
            _map.RenderTropicalForest = Resources.tropicForest;

            var sw = Stopwatch.StartNew();
            _map.Generate();
            sw.Stop();
            genTimeLabel.Text = sw.Elapsed.TotalMilliseconds.ToString()+ " ms";

            pictureBox1.Image = _map.Render();
        }
        

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            //_selectedPolygon = _map.GetAtPoint(e.Location);
            //label6.Text = string.Format("Polygon {0} Elevation: {1} Moisture: {2} Biome: {3}", _selectedPolygon.Center.ToString(), _selectedPolygon.Elevation, _selectedPolygon.Moisture, _selectedPolygon.Biome);
            
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Select where to save the map";
            saveFileDialog1.Filter = "*.png|*.png";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Select where to save the map";
            saveFileDialog1.Filter = "*.umap|*.umap";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WorldPlan));
                using (var fs = File.OpenWrite(saveFileDialog1.FileName))
                {
                    fs.SetLength(0);
                    using (GZipStream zip = new GZipStream(fs, CompressionMode.Compress))
                    {
                        serializer.Serialize(zip, _map);
                    }
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "*.umap|*.umap";
            openFileDialog1.Title = "Select map to load";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (var fs = File.OpenRead(openFileDialog1.FileName))
                {
                    using (GZipStream zip = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof (WorldPlan));
                        _map = (WorldPlan) serializer.Deserialize(zip);
                    }
                }
                pictureBox1.Image = _map.Render();
            }

        }


    }
}
