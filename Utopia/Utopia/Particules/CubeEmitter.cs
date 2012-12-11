using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Particules;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3CoreComponents.Particules.ParticulesCol;
using S33M3CoreComponents.Sprites3D;
using S33M3CoreComponents.Sprites3D.Processors;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33m3Engines.Effects;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Color = SharpDX.Color;

namespace Utopia.Particules
{
    public class CubeEmitter : BaseComponent, IEmitter
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private bool _isStopped;
        private float _maximumAge;
        private List<ColoredParticule> _particules;
        private static Vector3D GravityForce = new Vector3D(0.0, -9.81, 0.0);

        private Random _rnd;
        private string _cubeTexturePath;
        private string _fileNamePatern;
        private Dictionary<int, Color[]> _cubeColorSampled;
        private VisualWorldParameters _visualWorldParameters;
        private IWorldChunks _worldChunk;

        private Sprite3DRenderer<Sprite3DColorBillBoardProc> _particuleRenderer;
        #endregion

        #region Public Properties
        public ParticuleEngine ParentParticuleEngine { get; set; }

        public bool isStopped
        {
            get { return _isStopped; }
        }

        public List<ColoredParticule> Particules
        {
            get { return _particules; }
        }

        public bool WithLandscapeCollision { get; set; }
        #endregion

        public CubeEmitter(string cubeTexturePath, 
                           string fileNamePatern, 
                           float maximumAge,
                           VisualWorldParameters visualWorldParameters,
                           IWorldChunks worldChunk)
        {
            _cubeColorSampled = new Dictionary<int, Color[]>();
            _fileNamePatern = fileNamePatern;
            _cubeTexturePath = cubeTexturePath;
            _visualWorldParameters = visualWorldParameters;
            _worldChunk = worldChunk;
            _isStopped = false;
            _maximumAge = maximumAge;
            _particules = new List<ColoredParticule>();

            _rnd = new Random();
        }

        #region Public Methods
        public void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer)
        {
            CreateColorsSetPerCubeTexture();

            //Create the processor that will be used by the Sprite3DRenderer
            Sprite3DColorBillBoardProc processor = ToDispose(new Sprite3DColorBillBoardProc(ToDispose(new DefaultIncludeHandler()), sharedFrameBuffer));

            //Create a sprite3Drenderer that will use the previously created processor to accumulate text data for drawing.
            _particuleRenderer = ToDispose(new Sprite3DRenderer<Sprite3DColorBillBoardProc>(processor,
                                                                                            DXStates.Rasters.Default,
                                                                                            DXStates.Blenders.Disabled,
                                                                                            DXStates.DepthStencils.DepthReadEnabled,
                                                                                            context));
        }

        public void EmitParticule(int nbr, byte cubeId, Vector3I CubeLocation)
        {
            //Get Cube color palette
            Color[] palette = _cubeColorSampled[cubeId];

            while (nbr > 0)
            {
                //Randomize the Velocity
                Vector3 finalVelocity = new Vector3(0, 1 ,0);
                finalVelocity.X += (((float)_rnd.NextDouble() * 2) - 1) * 2;
                finalVelocity.Y += (((float)_rnd.NextDouble() * 2) - 1) * 5;
                finalVelocity.Z += (((float)_rnd.NextDouble() * 2) - 1) * 2;

                Vector3D CubeCenteredPosition = new Vector3D(CubeLocation.X + 0.5, CubeLocation.Y + 0.5, CubeLocation.Z + 0.5);

                _particules.Add(new ColoredParticule()
                {
                    Age = 0,
                    InitialPosition = CubeCenteredPosition,
                    ParticuleColor = palette[_rnd.Next(24)],
                    Position = CubeCenteredPosition,
                    Size = new Vector2(0.2f,0.2f),
                    Velocity = finalVelocity
                });
                
                nbr--;
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime timeSpend)
        {
            if (_particules.Count == 0) return;
            _particules.RemoveAll(x => x.Age > _maximumAge);

            RefreshExistingParticules(timeSpend.ElapsedGameTimeInS_LD);
            //Do Collision against landscape test using _worldChunk
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_particules.Count == 0) return;
            //Accumulate particules here for this emitters, and render them
            ColoredParticule p;

            _particuleRenderer.Begin(true);
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                Vector3 position = p.Position.AsVector3();
                ByteColor color = p.ParticuleColor;
                _particuleRenderer.Processor.Draw(ref position, ref p.Size, ref color);
            }
            _particuleRenderer.End(context);
        }

        #endregion

        #region Private Methods
        private void CreateColorsSetPerCubeTexture()
        {
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
                        var color = image.GetPixel(_rnd.Next(image.Width), _rnd.Next(image.Height));
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

        private void RefreshExistingParticules(float elapsedTime)
        {
            ColoredParticule p;
            for (int i = 0; i < _particules.Count; i++)
            {
                //Computation of the new dimension, its a simple deterministic computation using this formula :
                // Posi(t') = 1/2 * t² * (GravityVector) + t * (VelocityVector) + Posi(0)
                p = _particules[i];
                p.Age += elapsedTime; //Age in Seconds

                p.Position = ((0.5 * p.Age * p.Age) * CubeEmitter.GravityForce)    //Acceleration force
                               + (p.Age * p.Velocity)                              //Constant force
                               + p.InitialPosition;                                //Initial position of the particule
            }
        }
        #endregion
    }
}
