using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using SharpDX;
using Utopia.Worlds.Weathers.SharedComp;

namespace Utopia.Worlds.Weather
{
    public class Wind : GameComponent, IWind
    {
        #region Private Variables
        private Random _rndWindFlowing;
        private Random _rndWindFlowChange;
        private bool _randomFlow;
        #endregion

        #region Public Variables
        public Vector3 WindFlow { get; set; }
        public Vector3 FlatWindFlow { get; set; }
        #endregion

        public Wind(bool randomFlow = false)
        {
            _randomFlow = randomFlow;
        }

        #region Public methods
        public override void Initialize()
        {
            _rndWindFlowing = new Random();
            _rndWindFlowChange = new Random(_rndWindFlowing.Next());

            WindFlow = new Vector3(GetFlowRnd(), GetFlowRnd(), GetFlowRnd());
            FlatWindFlow = new Vector3(WindFlow.X, 0, WindFlow.Z);
        }

        public override void Update(ref GameTime TimeSpend)
        {
            if (!_randomFlow) return;

            if (_rndWindFlowing.Next(0, 10000) == 0)
            {
                WindFlow = new Vector3(GetFlowRnd(), GetFlowRnd(), GetFlowRnd());
                FlatWindFlow = new Vector3(WindFlow.X, 0, WindFlow.Z);
            }
        }

        #endregion

        #region Private Methods
        private float GetFlowRnd()
        {
            return (float)(_rndWindFlowing.NextDouble() * 2) - 1;
        }
        #endregion

    }
}
