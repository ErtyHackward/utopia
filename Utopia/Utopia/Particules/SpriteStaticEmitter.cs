using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Particules;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3CoreComponents.Particules.ParticulesCol;
using S33M3CoreComponents.Sprites3D;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33m3Engines.Effects;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Resources.Sprites;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;
using Utopia.Worlds.Weather;

namespace Utopia.Particules
{
    public class SpriteStaticEmitter : BaseComponent, IEmitter
    {
        #region Private Variables
        private FastRandom _rnd;
        private List<SpriteParticule> _particules;

        private bool _isStopped;
        private bool _withLandscapeCollision = false;
        private int _automaticGenerationRate = 1;
        private float _rndGenerationChance = 0.01f;
        private Sprite3DRenderer<Sprite3DBillBoardProc> _particuleRenderer;
        private ShaderResourceView _textureParticules;
        private SamplerState _textureSampler;
        private ParticuleEngine _parentParticuleEngine;

        private int _stateRasterId;
        private int _stateBlenderId;
        private int _stateDepthId;
        private IWeather _weather;
        #endregion

        #region Public Properties

        public double MaxRenderingDistance { get; set; }

        public ParticuleEngine ParentParticuleEngine { get; set; }

        public List<SpriteParticule> Particules
        {
            get { return _particules; }
        }

        public bool WithLandscapeCollision
        {
            get { return _withLandscapeCollision; }
            set { _withLandscapeCollision = value; }
        }

        public float RndGenerationChance
        {
            get { return _rndGenerationChance; }
            set { _rndGenerationChance = value; }
        }

        public int AutomaticGenerationRate
        {
            get { return _automaticGenerationRate; }
            set { _automaticGenerationRate = value; }
        }

        public bool isStopped
        {
            get { return _isStopped; }
        }
        #endregion
        
        public SpriteStaticEmitter(                   
                        ParticuleEngine parentParticuleEngine,                        
                        int textureSamplerId,
                        ShaderResourceView textureParticules,
                        int StateRasterId,
                        int StateBlenderId,
                        int StateDepthId,
                        IWeather weather)
        {
            _particules = new List<SpriteParticule>();
            _rnd = new FastRandom();

            _textureParticules = textureParticules;
            _textureSampler = RenderStatesRepo.GetSamplerState(textureSamplerId);
            _stateRasterId = StateRasterId;
            _stateBlenderId = StateBlenderId;
            _stateDepthId = StateDepthId;
            _parentParticuleEngine = parentParticuleEngine;
            _weather = weather;
        }
        #region Public Methods

        public void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer)
        {
            //Create the processor that will be used by the Sprite3DRenderer
            Sprite3DBillBoardProc processor = ToDispose(new Sprite3DBillBoardProc(_textureParticules, _textureSampler, ToDispose(new DefaultIncludeHandler()), sharedFrameBuffer, ClientSettings.EffectPack + @"Sprites/PointSprite3DBillBoard.hlsl"));

            //Create a sprite3Drenderer that will use the previously created processor to accumulate text data for drawing.
            _particuleRenderer = ToDispose(new Sprite3DRenderer<Sprite3DBillBoardProc>(processor,
                                                                                        _stateRasterId,
                                                                                        _stateBlenderId,
                                                                                        _stateDepthId,
                                                                                        context));
        }

        public void FTSUpdate(GameTime timeSpend)
        {
            if (_particules.Count == 0) return;
            _particules.RemoveAll(x => x.Age > x.maxAge);
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_particules.Count == 0) return;
            RefreshExistingParticules(elapsedTime);

            List<SpriteParticule> sortedList = _particules.OrderByDescending(x => Vector3D.DistanceSquared(x.Position.Value, ParentParticuleEngine.CameraPosition)).ToList();
            _particules = sortedList;
        }

        public void Draw(DeviceContext context, int index)
        {
            if (_particules.Count == 0) return;
            //Accumulate particules here for this emitters, and render them
            SpriteParticule p;

            _particuleRenderer.Begin(context, true);
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                Vector3 position = p.Position.Value.AsVector3();
                _particuleRenderer.Processor.Draw(ref position, ref p.Size, ref p.ColorModifier, p.ParticuleId);
            }
            _particuleRenderer.End(context);

        }

        public void EmitParticule(StaticEntityParticule particuleMetaData,
                                  Vector3D EmittedPosition)
        {
            int nbr = particuleMetaData.EmittedParticulesAmount;

            Vector3D startUpPosition = EmittedPosition + particuleMetaData.PositionOffset;

            while (nbr > 0)
            {
                //Randomize the Velocity
                Vector3 velocity = particuleMetaData.EmitVelocity;

                velocity.X = particuleMetaData.EmitVelocity.X + _rnd.NextFloat(particuleMetaData.EmitVelocityRandomness.X);
                velocity.Y = particuleMetaData.EmitVelocity.Y + _rnd.NextFloat(particuleMetaData.EmitVelocityRandomness.Y);
                velocity.Z = particuleMetaData.EmitVelocity.Z + _rnd.NextFloat(particuleMetaData.EmitVelocityRandomness.Z);

                float lifetime = particuleMetaData.ParticuleLifeTime + _rnd.NextFloat(particuleMetaData.ParticuleLifeTimeRandomness);

                Vector3D finalPosition;
                finalPosition.X = startUpPosition.X + _rnd.NextFloat(particuleMetaData.PositionRandomness.X);
                finalPosition.Y = startUpPosition.Y + _rnd.NextFloat(particuleMetaData.PositionRandomness.X);
                finalPosition.Z = startUpPosition.Z + _rnd.NextFloat(particuleMetaData.PositionRandomness.X);

                Vector3 accelerationForce = particuleMetaData.AccelerationForces;
                if (particuleMetaData.ApplyWindForce)
                {
                    accelerationForce += _weather.Wind.WindFlowFlat * 0.05f; 
                }

                _particules.Add(new SpriteParticule()
                {
                    AccelerationForce = new Vector3D(accelerationForce),
                    ColorModifier = new ByteColor(particuleMetaData.ParticuleColor.R, particuleMetaData.ParticuleColor.G, particuleMetaData.ParticuleColor.B, (byte)0),
                    InitialPosition = finalPosition,
                    maxAge = lifetime,
                    ParticuleId = particuleMetaData.ParticuleId,
                    Position = new FTSValue<Vector3D>(finalPosition),
                    Size = particuleMetaData.Size,
                    SizeGrowSpeed = particuleMetaData.SizeGrowSpeed,
                    Velocity = velocity,
                    alphaFadingPowBase = particuleMetaData.AlphaFadingPowBase
                });                 
                nbr--;
            }
        }

        public void Stop()
        {
            _particules.Clear();
            _isStopped = true;
        }
        #endregion

        #region Private Methods

        private void RefreshExistingParticules(float elapsedTime)
        {
            //Parallel processing here !! <== TO DO !!

            SpriteParticule p;
            for (int i = 0; i < _particules.Count; i++)
            {
                //Computation of the new dimension, its a simple deterministic computation using this formula :
                // Posi(t') = 1/2 * t² * (GravityVector) + t * (VelocityVector) + Posi(0)
                p = _particules[i];
                p.Age += elapsedTime; //Age in Seconds

                if (p.SizeGrowSpeed != 0) //Size
                {
                    float growsize = elapsedTime * p.SizeGrowSpeed;
                    p.Size.X += growsize;
                    p.Size.Y += growsize;
                }

                if (p.alphaFadingPowBase != 0)
                {
                    double alpha = Math.Pow(MathHelper.FullLerp(0.0f, 1.0f, 0, p.maxAge, p.Age, true), p.alphaFadingPowBase);
                    double alphaInverted = Math.Pow(MathHelper.FullLerp(1.0f, 0.0f, 0, p.maxAge, p.Age), p.alphaFadingPowBase);

                    alpha = Math.Max(alpha, alphaInverted);

                    p.ColorModifier.A = (byte)(alpha * 255);
                }

                p.Position.Value = ((0.5 * p.Age * p.Age) * p.AccelerationForce)    //Taking into account a specific force applied in a constant way to the particule (Generaly Gravity) = Acceleration Force
                              + (p.Age * p.Velocity)                                 //New position based on time and move vector
                              + p.InitialPosition;                                    //Initial Position
            }
        }
        #endregion
    }

}
