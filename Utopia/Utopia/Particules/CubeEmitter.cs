using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Particules;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.World;
using Color = SharpDX.Color;

namespace Utopia.Particules
{
    public class CubeEmitter : BaseComponent, IEmitter
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private bool _isStopped;
        private List<Particule> _particules;

        private string _cubeTexturePath;
        private string _fileNamePatern;
        private Dictionary<int, Color[]> _cubeColorSampled;
        private VisualWorldParameters _visualWorldParameters;
        #endregion

        #region Public Properties
        public ParticuleEngine ParentParticuleEngine { get; set; }

        public bool isStopped
        {
            get { return _isStopped; }
        }

        public List<Particule> Particules
        {
            get { return _particules; }
        }

        public bool WithLandscapeCollision { get; set; }
        #endregion

        public CubeEmitter(string cubeTexturePath, 
                           string fileNamePatern, 
                           VisualWorldParameters visualWorldParameters)
        {
            _cubeColorSampled = new Dictionary<int, Color[]>();
            _fileNamePatern = fileNamePatern;
            _cubeTexturePath = cubeTexturePath;
            _visualWorldParameters = visualWorldParameters;
        }

        #region Public Methods
        public void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer)
        {
            CreateColorsSetPerCubeTexture();
        }

        public void EmitParticule(int nbr, byte cubeId, Vector3I CubeLocation)
        {
            logger.Debug("Particules emitted {0}, CubeId {1}, CubeLocation {2}", nbr, cubeId, CubeLocation);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Update(S33M3DXEngine.Main.GameTime timeSpend)
        {
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
        }

        #endregion

        #region Private Methods
        private void CreateColorsSetPerCubeTexture()
        {
            Random rnd = new Random();

            Dictionary<int, Color[]> perBitmapColorSampling = new Dictionary<int, Color[]>(); ;

            //Sample each cubeTextures bmp
            foreach (var file in Directory.GetFiles(_cubeTexturePath, _fileNamePatern))
            {
                //Get Texture ID.
                string fileName = Path.GetFileName(file);
                int id = int.Parse(fileName.Substring(2, fileName.IndexOf('_') - 2));

                //Load files
                using (var image = (Bitmap)Bitmap.FromFile(file))
                {
                    //Bitmap sampling here, 4 point per texture
                    Color[] colorArray = new Color[4];
                    for (int i = 0; i < 4; i++)
                    {
                        var color = image.GetPixel(rnd.Next(image.Width), rnd.Next(image.Height));
                        colorArray[i] = new Color(color.R, color.G, color.B, color.A);
                    }
                    perBitmapColorSampling.Add(id, colorArray);
                }
            }
            
            //for each define cubes profiles, merge 6 faces color sampled to give a collections a sampled color per Cube (24 colors)
            foreach (var cubeprofile in _visualWorldParameters.WorldParameters.Configuration.GetAllCubesProfiles())
            {
                List<Color> colorArray = new List<Color>();
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Back]);
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Front]);
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Left]);
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Right]);
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Top]);
                colorArray.AddRange(perBitmapColorSampling[cubeprofile.Tex_Bottom]);
                _cubeColorSampled.Add(cubeprofile.Id, colorArray.ToArray());
            }

        }
        #endregion
    }
}
