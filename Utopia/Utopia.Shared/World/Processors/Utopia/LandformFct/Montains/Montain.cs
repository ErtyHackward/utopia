using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class Montain : ITerrainGenerator 
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Montain(int seed, Gradient groundGradient)
            :this (seed, groundGradient, groundGradient)
        {
        }

        public Montain(int seed, Cache<Gradient> groundGradient)
            :this(seed, groundGradient, groundGradient.Source)
        {
        }

        private Montain(int seed, INoise groundGradient, Gradient groundGradientTyped)
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
            INoise montain_shape_fractal = new FractalBillow(new Simplex(_seed), 4, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result ==> Wil modify the Scope of output range value
            INoise montain_scale = new ScaleOffset(montain_shape_fractal, 0.6 * _groundGradientTyped.AdjustY, -0.30 * _groundGradientTyped.AdjustY); 
            //Remove Y value from impacting the result (Fixed to 0), the value output range will not be changed, but the influence of the Y will be removed
            INoise montain_y_scale = new ScaleDomain(montain_scale, 1.0, 0.0, 1.0);

            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise montain_terrain = new Turbulence(_groundGradient, 0, montain_y_scale);

            return montain_terrain;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
