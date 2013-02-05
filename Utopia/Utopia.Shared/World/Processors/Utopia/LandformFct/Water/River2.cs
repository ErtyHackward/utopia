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
using S33M3CoreComponents.Noise.NoiseResultCombiner;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class River2 : ITerrainGenerator
    {
        #region Private Variables
        private INoise _groundGradient;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public River2(int seed, INoise groundGradient)
        {
            _groundGradient = groundGradient;
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
            INoise river_shape_fractal = new FractalRidgedMulti(new Simplex(_seed), 1, 2, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result ==> Wil modify the Scope of output range value
            INoise river_shape_scale = new ScaleOffset(river_shape_fractal, 0.30, 0.01);
            //Remove Y value from impacting the result (Fixed to 0), the value output range will not be changed, but the influence of the Y will be removed

            //Force the Fractal to be used as 2D Noise, I don't need to 3th dimension
            INoise river_y_scale = new NoiseAccess(river_shape_fractal, NoiseAccess.enuDimUsage.Noise2D, true);

            INoise turb = new ScaleOffset(river_y_scale, 0.03, 0);
            INoise river_selected = new Select(0, turb, river_y_scale, 0.7);  //Last param define the width of the river

            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value
            INoise _groundGradient_biased = new Bias(_groundGradient, 0.45);
            INoise river = new Turbulence(_groundGradient_biased, 0, river_selected);

            return river;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
