using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
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
using Utopia.Shared.Settings;

namespace Utopia.Particules
{
    public class SpriteEmitter : BaseComponent, IEmitter
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
        
        public SpriteEmitter(
                        ParticuleEngine parentParticuleEngine,                        
                        int textureSamplerId,
                        ShaderResourceView textureParticules,
                        int StateRasterId,
                        int StateBlenderId,
                        int StateDepthId)
        {
            _particules = new List<SpriteParticule>();
            _rnd = new FastRandom();

            _textureParticules = textureParticules;
            _textureSampler = RenderStatesRepo.GetSamplerState(textureSamplerId);
            _stateRasterId = StateRasterId;
            _stateBlenderId = StateBlenderId;
            _stateDepthId = StateDepthId;
            _parentParticuleEngine = parentParticuleEngine;

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

        public void Update(GameTime timeSpend)
        {
            if (_particules.Count == 0) return;
            _particules.RemoveAll(x => x.Age > x.maxAge);
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
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
            BaseParticule p;

            _particuleRenderer.Begin(true);
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                Vector3 position = p.Position.Value.AsVector3();
                ByteColor white = Color.White;
                _particuleRenderer.Processor.Draw(ref position, ref p.Size, ref white, 0);
            }
            _particuleRenderer.End(context);

        }

        public void EmitParticule(int nbr,
                                   float maximumAge,
                                   Vector3 velocity,
                                   Vector3 accelerationForce,
                                   Vector3D emittedPosition)
        {
            while (nbr > 0)
            {
                //Randomize the Velocity here
                Vector3 velocityRndAmount = velocity / 10;
                velocity.X += _rnd.NextFloat(-velocityRndAmount.X, velocityRndAmount.X);
                velocity.X += _rnd.NextFloat(-velocityRndAmount.Y, velocityRndAmount.Y);
                velocity.X += _rnd.NextFloat(-velocityRndAmount.Z, velocityRndAmount.Z);

                _particules.Add(new SpriteParticule() { Position = new FTSValue<Vector3D>(emittedPosition),
                                                        Velocity = velocity,
                                                        maxAge = maximumAge,
                                                        Size = Vector2.One,
                                                        AccelerationForce = new Vector3D(accelerationForce),
                                                        InitialPosition = emittedPosition
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

        private void RefreshExistingParticules(long elapsedTime)
        {
            //Parallel processing here !! <== TO DO !!

            SpriteParticule p;
            for (int i = 0; i < _particules.Count; i++)
            {
                //Computation of the new dimension, its a simple deterministic computation using this formula :
                // Posi(t') = 1/2 * t² * (GravityVector) + t * (VelocityVector) + Posi(0)
                p = _particules[i];
                p.Age += elapsedTime / 1000.0f; //Age in Seconds
                
                p.Position.Value = ((0.5 * p.Age * p.Age) * p.AccelerationForce)    //Taking into account a specific force applied in a constant way to the particule (Generaly Gravity) = Acceleration Force
                              + (p.Age * p.Velocity)                                 //New position based on time and move vector
                              + p.InitialPosition;                                    //Initial Position
            }
        }
        #endregion
    }

}
