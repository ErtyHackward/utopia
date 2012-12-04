using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Particules
{
    public class Emitter : IEmitter
    {
        public enum ParticuleGenerationMode
        {
            Manual,
            Automatic,
            Random
        }

        #region Private Variables
        private List<Particule> _particules;
        private Vector3D _initialPosition;
        private Vector3 _initialVelocity;
        private Vector2 _initialSize;
        private float _maximumAge;
        private ParticuleGenerationMode _generationMode;

        private bool _withLandscapeCollision = false;
        private int _automaticGenerationRate = 1;
        private float _rndGenerationChance = 0.01f;
        #endregion

        #region Public Properties
        public List<Particule> Particules
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
        #endregion
        
        public Emitter(Vector3D initialPosition,
                        Vector3 initialVelocity,
                        Vector2 initialSize,
                        float maximumAge,
                        ParticuleGenerationMode generationMode,
                        int initialParticules = 0)
        {
            _particules = new List<Particule>();

            _initialPosition = initialPosition;
            _initialVelocity = initialVelocity;
            _initialSize = initialSize;
            _maximumAge = maximumAge;
            _generationMode = generationMode;

            GenerateNewParticule(initialParticules);
        }

        public void Dispose()
        {
        }

        #region Public Methods

        public void Update(GameTime timeSpend)
        {
            RefreshExistingParticules();
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public void Draw(DeviceContext context, int index)
        {
        }

        public void EmmitParticule(int nbr = 1)
        {
            GenerateNewParticule(nbr);
        }

        public void Stop()
        {
            _particules.Clear();
        }
        #endregion

        #region Private Methods
        private void GenerateNewParticule(int nbr = 1)
        {
            while (nbr > 0)
            {
                _particules.Add(new Particule());
                nbr--;
            }
        }

        private void RefreshExistingParticules()
        {
            for (int i = 0; i < _particules.Count; i++)
            {

            }
        }
        #endregion

    }
}
