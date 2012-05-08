using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.ResultCombiner;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class Plateau : ITerrainGenerator
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Plateau(int seed, Gradient groundGradient)
            : this(seed, groundGradient, groundGradient)
        {
        }

        public Plateau(int seed, Cache<Gradient> groundGradient)
            : this(seed, groundGradient, groundGradient.Source)
        {
        }

        private Plateau(int seed, INoise groundGradient, Gradient groundGradientTyped)
        {
            _groundGradient = groundGradient;
            _groundGradientTyped = groundGradientTyped;
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            //REM
            //The various parameters value here are scaled for a gradient being feed by 0 to 1 input value.
            //When this gradient is configured to recevied other value range then some parameters needs to be rescaled
            //That's the reason for using this _groundGradientTyped.AdjustY value
            //This way no matter the the Gradient Range, the values impacting it will be rescaled.

            //Create the Lowland base fractal with range from 0 to 1 values
            INoise plain_shape_fractal = new FractalFbm(new Simplex(_seed), 6, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result ==> Wil modify the Scope of output range value
            INoise plain_scale = new ScaleOffset(plain_shape_fractal, 0.20 * _groundGradientTyped.AdjustY, -0.2 * _groundGradientTyped.AdjustY);
            //Remove Y value from impacting the result (Fixed to 0), the value output range will not be changed, but the influence of the Y will be removed
            INoise plain_y_scale = new ScaleDomain(plain_scale, 1.0, 3.0, 1.0);

            INoise _groundChaotic = new FractalFbm(new Simplex(_seed), 5, 2);
            INoise _gradiantChaotic = new Blend(_groundGradient, _groundChaotic, -0.65);

            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise plain_terrain = new Turbulence(_gradiantChaotic, 0, plain_y_scale);

            return plain_terrain;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
