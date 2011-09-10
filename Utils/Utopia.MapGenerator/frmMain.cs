using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using S33M3Engines.Shared.Math.Noises;

namespace Utopia.MapGenerator
{
    public partial class frmMain : Form
    {
        private Map _map;

        private Polygon _selectedPolygon;
        
        public frmMain()
        {
            InitializeComponent();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {


            _map = new Map(new GenerationParameters { 
                MapSize = new Size((int)numericUpDown1.Value,(int)numericUpDown2.Value), 
                CenterElevation = centerElevationCheck.Checked,
                ElevationSeed = (int)voronoiSeedNumeric.Value, 
                GridSeed = (int)voronoiSeedNumeric.Value, 
                PolygonsCount = (int)voronoiPolyNumeric.Value, 
                RelaxCount = 3});
            _map.Generate();

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
                XmlSerializer serializer = new XmlSerializer(typeof(Map));
                using (var fs = File.OpenWrite(saveFileDialog1.FileName))
                {
                    serializer.Serialize(fs, _map);
                    fs.SetLength(fs.Position);
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
                    XmlSerializer serializer = new XmlSerializer(typeof(Map));
                    _map = (Map)serializer.Deserialize(fs);
                }
                pictureBox1.Image = _map.Render();
            }

        }


    }
}
