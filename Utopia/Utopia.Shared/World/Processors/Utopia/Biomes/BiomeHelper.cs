using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class BiomeHelper
    {
        #region Private Variables
        private WorldConfiguration _config;
        #endregion

        #region Public Properties
        #endregion

        public BiomeHelper(WorldConfiguration config)
        {
            _config = config;
        }

        #region Public Methods
        public byte GetBiome(double landFormType, double temperature, double moisture)
        {
            enuLandFormType landformtype = (enuLandFormType)landFormType;

            for (byte biomeId = 0; biomeId <= _config.Biomes.Count - 1; biomeId++)
            {
                Biome biome = _config.Biomes[biomeId];
                //Does this biome support this land form type ?
                if (biome.LandFormFilters.Contains(landformtype))
                {
                    //Check the temp range
                    if (temperature >= biome.TemperatureFilter.Min &&
                        temperature <= biome.TemperatureFilter.Max)
                    {
                        if (moisture >= biome.MoistureFilter.Min &&
                            moisture <= biome.MoistureFilter.Max)
                        {
                            return biomeId;
                        }
                    }
                }
            }

            //By default return the first Biome from the list
            return (byte)0;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
