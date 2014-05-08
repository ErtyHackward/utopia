using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Color = SharpDX.Color;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs.Landscape;
using System.Threading.Tasks;
using Utopia.Resources.Sprites;
using Utopia.Shared.Settings;
using Utopia.Worlds.Weather;

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

        private Bitmap[] _colorsBiome;

        private FastRandom _rnd;
        private string _cubeTexturePath;
        private string _fileNamePatern;
        private string _biomeColorFilePath;

        private Dictionary<int, Color[]> _cubeColorSampled;
        private VisualWorldParameters _visualWorldParameters;
        private IWorldChunks _worldChunk;
        private readonly ILandscapeManager _landscapeManager;
        private BoundingBox _cubeBB;

        private double _maxRenderingDistanceSquared;
        private double _maxRenderingDistance;
        private IWeather _weather;
        private Sprite3DRenderer<CubeColorProc> _particuleRenderer;

        #endregion

        #region Public Properties
        public double MaxRenderingDistance
        {
            get { return _maxRenderingDistance; }
            set { _maxRenderingDistance = value; _maxRenderingDistanceSquared = value * value; }
        }

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
                           string biomeColorFilePath,
                           float maximumAge,
                           float size,
                           VisualWorldParameters visualWorldParameters,
                           IWorldChunks worldChunk,
                           ILandscapeManager landscapeManager,
                           double maxRenderingDistance,
                           IWeather weather)
        {
            if (landscapeManager == null) throw new ArgumentNullException("landscapeManager");

            _cubeColorSampled = new Dictionary<int, Color[]>();
            _fileNamePatern = fileNamePatern;
            _cubeTexturePath = cubeTexturePath;
            _visualWorldParameters = visualWorldParameters;
            _biomeColorFilePath = biomeColorFilePath;
            _weather = weather;
            MaxRenderingDistance = maxRenderingDistance;
            _worldChunk = worldChunk;
            _landscapeManager = landscapeManager;
            _isStopped = false;
            _maximumAge = maximumAge;
            _particules = new List<ColoredParticule>();
            
            _rnd = new FastRandom();

            _cubeBB = new BoundingBox(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(size / 2.0f, size / 2.0f, size / 2.0f));
        }

        #region Public Methods
        public void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer)
        {
            CreateColorsSetPerCubeTexture();
            LoadBiomeColorsTexture();

            //Create the processor that will be used by the Sprite3DRenderer
            CubeColorProc processor = ToDispose(new CubeColorProc(ToDispose(new DefaultIncludeHandler()), sharedFrameBuffer));
            
            //Create a sprite3Drenderer that will use the previously created processor to accumulate text data for drawing.
            _particuleRenderer = ToDispose(new Sprite3DRenderer<CubeColorProc>(processor,
                                                                                            DXStates.Rasters.Default,
                                                                                            DXStates.Blenders.Disabled,
                                                                                            DXStates.DepthStencils.DepthReadWriteEnabled,
                                                                                            context));
        }

        public void EmitParticuleForCubeDestruction(int nbr, TerraCube cube, Vector3I CubeLocation, ref Vector3D cameraLocation)
        {
            //Check distance to emit
            if (MaxRenderingDistance > 0 && Vector3D.DistanceSquared(cameraLocation, new Vector3D(CubeLocation)) > _maxRenderingDistanceSquared) return;

            //GetCube Profile
            VisualChunk chunk = null;
            var profile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.Id];
            //Get Chunk in case if the block is subject to BiomeColoring
            chunk = _worldChunk.GetChunk(CubeLocation.X, CubeLocation.Z);

            //Foreach Surrending Cube
            ByteColor blockAvgColorReceived = new ByteColor();
            foreach (var surrendingCube in chunk.BlockData.ChunkCubes.GetSurroundingBlocksIndex(ref CubeLocation))
            {
                var cubeColor = chunk.BlockData.ChunkCubes.Cubes[surrendingCube.Index].EmissiveColor;
                blockAvgColorReceived.A = Math.Max(blockAvgColorReceived.A, cubeColor.A);
                blockAvgColorReceived.R = Math.Max(blockAvgColorReceived.R, cubeColor.R);
                blockAvgColorReceived.G = Math.Max(blockAvgColorReceived.G, cubeColor.G);
                blockAvgColorReceived.B = Math.Max(blockAvgColorReceived.B, cubeColor.B);
            }

            //Get Cube color palette
            Color[] palette = _cubeColorSampled[cube.Id];

            while (nbr > 0)
            {
                //Randomize the Velocity
                Vector3 finalVelocity = new Vector3(0, 1 ,0);
                finalVelocity.X += (float)_rnd.NextDouble(-1.0, 1.0) * 1.5f;
                finalVelocity.Y += (float)_rnd.NextDouble() * 3f;
                finalVelocity.Z += (float)_rnd.NextDouble(-1.0, 1.0) * 1.5f;

                Vector3D CubeCenteredPosition = new Vector3D(CubeLocation.X + _rnd.NextDouble(0.2, 0.8), CubeLocation.Y + _rnd.NextDouble(0.2, 0.8), CubeLocation.Z + _rnd.NextDouble(0.2, 0.8));

                //Get Color
                var color = palette[_rnd.Next(24)];
                if (color.A < 255)
                {
                    ApplyBiomeColor(ref color, profile.BiomeColorArrayTexture, chunk.BlockData.GetColumnInfo(CubeLocation.X - chunk.ChunkPositionBlockUnit.X, CubeLocation.Z - chunk.ChunkPositionBlockUnit.Y));
                }

                _particules.Add(new ColoredParticule()
                {
                    Age = 0 + (float)_rnd.NextDouble(0,2.0),
                    computationAge = 0,
                    InitialPosition = CubeCenteredPosition,
                    ParticuleColor = color,
                    Position = new FTSValue<Vector3D>(CubeCenteredPosition),
                    Size = new Vector2(0.1f,0.1f),
                    Velocity = finalVelocity,
                    ColorReceived = blockAvgColorReceived,
                    SpinningRotation = Quaternion.RotationAxis(Vector3.UnitY, _rnd.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8)) *
                                       Quaternion.RotationAxis(Vector3.UnitX, _rnd.NextFloat(-MathHelper.Pi / 16, MathHelper.Pi / 16)) *
                                       Quaternion.RotationAxis(Vector3.UnitZ, _rnd.NextFloat(-MathHelper.Pi / 16, MathHelper.Pi / 16)),
                    RotationAngles = new FTSValue<Quaternion>(Quaternion.Identity)
                });

                nbr--;
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void FTSUpdate(GameTime timeSpend)
        {
            if (_particules.Count == 0) return;
            _particules.RemoveAll(x => x.Age > _maximumAge);

            RefreshExistingParticules(timeSpend.ElapsedGameTimeInS_LD);
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            ColoredParticule p;
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                if (!p.isFrozen)
                {
                    Vector3D.Lerp(ref p.Position.ValuePrev, ref p.Position.Value, interpolationHd, out p.Position.ValueInterp);
                    Quaternion.Slerp(ref p.RotationAngles.ValuePrev, ref p.RotationAngles.Value, interpolationLd, out p.RotationAngles.ValueInterp);
                }
            }
        }

        public void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_particules.Count == 0) return;
            //Accumulate particules here for this emitters, and render them
            ColoredParticule p;

            _particuleRenderer.Begin(context, true);
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                Vector3 position = p.Position.ValueInterp.AsVector3();
                ByteColor color = p.ParticuleColor;

                //Add a new Particule
                Matrix result;
                Matrix scaling = Matrix.Scaling(p.Size.X, p.Size.Y, p.Size.Y);
                Matrix rotation = Matrix.RotationQuaternion(p.RotationAngles.ValueInterp); //Identity rotation
                Matrix translation = Matrix.Translation(position);

                result = scaling * rotation * translation;

                _particuleRenderer.Processor.Draw(ref color, ref p.ColorReceived, ref result);
            }
            _particuleRenderer.End(context);
        }

        #endregion

        #region Private Methods
        private void LoadBiomeColorsTexture()
        {

            List<Bitmap> biomeMaps = new List<Bitmap>();
            foreach (var biomeColorImg in Directory.GetFiles(_biomeColorFilePath, "*.png"))
            {
                biomeMaps.Add((Bitmap)ToDispose(Bitmap.FromFile(biomeColorImg)));
            }

            _colorsBiome = biomeMaps.ToArray();
        }

        private void ApplyBiomeColor(ref Color baseColor, byte biomeColorId, ChunkColumnInfo ci)
        {
            //Apply weather offset
            var m = ((ci.Moisture / 256f) * 0.6f) + 0.2f;
            var moistureAmount = MathHelper.Clamp(m + _weather.MoistureOffset, 0f, 1f);
            var t = ((ci.Temperature / 256f) * 0.6f) + 0.2f;
            var tempAmount = MathHelper.Clamp(t + _weather.TemperatureOffset, 0f, 1f);

            //X = Moisture
            int moisture = (int)MathHelper.FullLerp(0, _colorsBiome[biomeColorId].Width - 1, 0.0, 1.0, moistureAmount);
            //Y = Temperature
            int temp = (int)MathHelper.FullLerp(0, _colorsBiome[biomeColorId].Height - 1, 0.0, 1.0, tempAmount);

            var sampledColor = _colorsBiome[biomeColorId].GetPixel(moisture, temp);

            int red = (int)MathHelper.FullLerp(0.0, (double)sampledColor.R, 0.0, 255.0, (double)baseColor.R, true);
            int green = (int)MathHelper.FullLerp(0.0, (double)sampledColor.G, 0.0, 255.0, (double)baseColor.G, true);
            int blue = (int)MathHelper.FullLerp(0.0, (double)sampledColor.B, 0.0, 255.0, (double)baseColor.B, true);

            baseColor.R = (byte)red;
            baseColor.G = (byte)green;
            baseColor.B = (byte)blue;
        }

        private void CreateColorsSetPerCubeTexture()
        {
            Dictionary<string, Color[]> perBitmapColorSampling = new Dictionary<string, Color[]>(); ;

            //Sample each cubeTextures bmp
            foreach (var file in Directory.GetFiles( _cubeTexturePath, _fileNamePatern))
            {
                //Get Texture ID.
                string fileName = Path.GetFileNameWithoutExtension(file);

                //Load files
                using (var image = (Bitmap)Bitmap.FromFile(file))
                {
                    //Bitmap sampling here, 4 point per texture
                    Color[] colorArray = new Color[4];
                    for (int i = 0; i < 4; i++)
                    {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 0, 0, 0);
                        while (color.A == 0)
                        {
                            color = image.GetPixel(_rnd.Next(image.Width), _rnd.Next(image.Height));
                            colorArray[i] = new Color(color.R, color.G, color.B, color.A);
                        }
                    }
                    perBitmapColorSampling.Add(fileName, colorArray);
                }
            }
            
            //for each define cubes profiles, merge 6 faces color sampled to give a collections a sampled color per Cube (24 colors)
            foreach (var blockProfile in _visualWorldParameters.WorldParameters.Configuration.GetAllCubesProfiles())
            {
                if (blockProfile.Textures == null || blockProfile.Textures.Count(x => x.Texture.Name == null) > 0) continue;
                List<Color> colorArray = new List<Color>();
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Back.Texture.Name]);
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Front.Texture.Name]);
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Left.Texture.Name]);
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Right.Texture.Name]);
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Top.Texture.Name]);
                colorArray.AddRange(perBitmapColorSampling[blockProfile.Tex_Bottom.Texture.Name]);
                _cubeColorSampled.Add(blockProfile.Id, colorArray.ToArray());
            }

        }

        private void RefreshExistingParticules(float elapsedTime)
        {
            Parallel.ForEach(_particules, p =>
                {
                    //Computation of the new dimension, its a simple deterministic computation using this formula :
                    // Posi(t') = 1/2 * t² * (GravityVector) + t * (VelocityVector) + Posi(0)
                    p.Age += elapsedTime; //Age in Seconds
                    p.computationAge += elapsedTime;

                    if (!p.isFrozen)
                    {
                        p.Position.BackUpValue();
                        p.Position.Value = ((0.5 * p.computationAge * p.computationAge) * CubeEmitter.GravityForce)    //Acceleration force
                                       + (p.computationAge * p.Velocity)                              //Constant force
                                       + p.InitialPosition;                                //Initial position of the particule

                        p.RotationAngles.BackUpValue();
                        p.RotationAngles.Value *= p.SpinningRotation;
                    }

                    CollisionCheck(p);
                });
        }

        private void CollisionCheck(ColoredParticule p)
        {
            if (_landscapeManager.IsCollidingWithTerrain(ref _cubeBB, ref p.Position.Value) > 0)
            {
                if (!p.isFrozen)
                {
                    if (p.wasColliding)
                    {
                        p.isFrozen = true;
                        return;
                    }
                    //Colliding with landscape !
                    p.computationAge = 0;
                    p.Velocity = (Vector3D.Normalize((p.Position.ValuePrev - p.Position.Value)) * _rnd.NextDouble(0.5, 0.9)).AsVector3();
                    p.InitialPosition = p.Position.ValuePrev;
                    p.Position.Value = p.Position.ValuePrev;
                    p.wasColliding = true;
                }
            }
            else
            {
                //If frozen and not colliding !
                if (p.isFrozen)
                {
                    //Colliding with landscape !
                    p.computationAge = 0;
                    p.InitialPosition = p.Position.Value;
                    p.Position.ValuePrev = p.Position.Value;
                    p.isFrozen = false;
                    p.wasColliding = false;
                }
            }

        }
        #endregion
    }
}
