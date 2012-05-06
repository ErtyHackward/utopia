using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct.Plains
{
    public class Plain : ILandform
    {
        #region Private Variables
        private INoise _ground_gradient;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Plain(int seed, INoise ground_gradient)
        {
            _ground_gradient = ground_gradient;
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise lowland_shape_fractal = new FractalFbm(new Simplex(_seed), 2, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result
            INoise lowland_scale = new ScaleOffset(lowland_shape_fractal, 0.2, 0.25);
            //Remove Y value from impacting the result (Fixed to 0) = removing one dimension to the generator noise
            INoise lowland_y_scale = new ScaleDomain(lowland_scale, 1, 0);
            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise lowland_terrain = new Turbulence(_ground_gradient, 0, lowland_y_scale);

            return lowland_terrain;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
