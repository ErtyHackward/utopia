using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3CoreComponents.Particules.ParticulesCol;
using S33M3CoreComponents.Sprites3D;
using S33M3CoreComponents.Sprites3D.Processors;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33m3Engines.Effects;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Particules.Emitters
{
    public class Emitter : BaseComponent, IEmitter
    {
        public enum ParticuleGenerationMode
        {
            Manual,
            Automatic,
            Random
        }

        #region Private Variables
        private Random _rnd;
        private List<BaseParticule> _particules;
        private Vector3D _initialPosition;
        private Vector3 _initialVelocity;
        private Vector2 _initialSize;
        private float _maximumAge;
        private ParticuleGenerationMode _generationMode;

        private bool _isStopped;
        private bool _withLandscapeCollision = false;
        private int _automaticGenerationRate = 1;
        private float _rndGenerationChance = 0.01f;
        private Vector3 _velocityRndPower;
        private Vector3D _forceOnEmittedParticules;
        private Sprite3DRenderer<Sprite3DBillBoardProc> _particuleRenderer;
        private ShaderResourceView _textureParticules;
        private SamplerState _textureSampler;

        private int _stateRasterId;
        private int _stateBlenderId;
        private int _stateDepthId;
        #endregion

        #region Public Properties

        public double MaxRenderingDistance { get; set; }

        public ParticuleEngine ParentParticuleEngine { get; set; }

        public List<BaseParticule> Particules
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
        
        public Emitter(Vector3D initialPosition,
                        Vector3 initialVelocity,
                        Vector2 initialSize,
                        float maximumAge,
                        ParticuleGenerationMode generationMode,
                        Vector3 velocityRndPower,
                        Vector3D forceOnEmittedParticules,
                        SamplerState textureSampler,
                        ShaderResourceView textureParticules,
                        int StateRasterId,
                        int StateBlenderId,
                        int StateDepthId)
        {
            _particules = new List<BaseParticule>();
            _rnd = new Random();

            _forceOnEmittedParticules = forceOnEmittedParticules;
            _velocityRndPower = velocityRndPower;
            _initialPosition = initialPosition;
            _initialVelocity = initialVelocity;
            _initialSize = initialSize;
            _maximumAge = maximumAge;
            _generationMode = generationMode;
            _textureParticules = textureParticules;
            _textureSampler = textureSampler;

            _stateRasterId = StateRasterId;
            _stateBlenderId = StateBlenderId;
            _stateDepthId = StateDepthId;
        }

        
        #region Public Methods

        public void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer)
        {
            //Create the processor that will be used by the Sprite3DRenderer
            Sprite3DBillBoardProc processor = ToDispose(new Sprite3DBillBoardProc(_textureParticules, _textureSampler, ToDispose(new DefaultIncludeHandler()), sharedFrameBuffer));

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
            _particules.RemoveAll(x => x.Age > _maximumAge);
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_particules.Count == 0) return;
            RefreshExistingParticules(elapsedTime);

            List<BaseParticule> sortedList = _particules.OrderByDescending(x => Vector3D.DistanceSquared(x.Position.Value, ParentParticuleEngine.CameraPosition)).ToList();
            _particules = sortedList;
        }

        public void Draw(DeviceContext context, int index)
        {
            if (_particules.Count == 0) return;
            //Accumulate particules here for this emitters, and render them
            BaseParticule p;

            _particuleRenderer.Begin(context, true);
            for (int i = 0; i < _particules.Count; i++)
            {
                p = _particules[i];
                Vector3 position = p.Position.Value.AsVector3();
                ByteColor white = Color.White;
                _particuleRenderer.Processor.Draw(ref position, ref p.Size, ref white, 0);
            }
            _particuleRenderer.End(context);

        }

        public void EmmitParticule(int nbr = 1)
        {
            GenerateNewParticule(nbr);
        }

        public void Stop()
        {
            _particules.Clear();
            _isStopped = true;
        }
        #endregion

        #region Private Methods
        private void GenerateNewParticule(int nbr = 1)
        {
            while (nbr > 0)
            {
                //Randomize the Velocity here
                Vector3 finalVelocity = _initialVelocity;
                finalVelocity.X += (((float)_rnd.NextDouble() * 2) - 1) * _velocityRndPower.X;
                finalVelocity.Y += (((float)_rnd.NextDouble() * 2) - 1) * _velocityRndPower.Y;
                finalVelocity.Z += (((float)_rnd.NextDouble() * 2) - 1) * _velocityRndPower.Z;

                _particules.Add(new BaseParticule() { Position = new FTSValue<Vector3D>(_initialPosition), Velocity = finalVelocity, Age = 0, Size = _initialSize });
                nbr--;
            }
        }

        private void RefreshExistingParticules(float elapsedTime)
        {
            //Parallel processing here !! <== TO DO !!

            BaseParticule p;
            for (int i = 0; i < _particules.Count; i++)
            {
                //Computation of the new dimension, its a simple deterministic computation using this formula :
                // Posi(t') = 1/2 * t² * (GravityVector) + t * (VelocityVector) + Posi(0)
                p = _particules[i];
                p.Age += (float)elapsedTime / 1000.0f; //Age in Seconds
                
                p.Position.Value = ((0.5 * p.Age * p.Age) * _forceOnEmittedParticules)    //Taking into account a specific force applied in a constant way to the particule (Generaly Gravity) = Acceleration Force
                              + (p.Age * p.Velocity)                                 //New position based on time and move vector
                              + _initialPosition;                                    //Initial Position
            }
        }
        #endregion
    }

}
