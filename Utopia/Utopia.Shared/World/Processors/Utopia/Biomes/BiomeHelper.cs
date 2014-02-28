using S33M3Resources.Structs;
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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private UtopiaWorldConfiguration _config;
        private Dictionary<enuLandFormType, List<List<Biome>>> _biomesConfig;
        #endregion

        #region Public Properties
        #endregion

        public BiomeHelper(UtopiaWorldConfiguration config)
        {
            _config = config;
            Initialize();
        }

        #region Public Methods
        //public byte GetBiome(double landFormType, double temperature, double moisture, double zone)
        //{
        //    enuLandFormType landformtype = (enuLandFormType)landFormType;

        //    for (byte biomeId = 0; biomeId <= _config.ProcessorParam.Biomes.Count - 1; biomeId++)
        //    {
        //        Biome biome = _config.ProcessorParam.Biomes[biomeId];
        //        //Does this biome support this land form type ?
        //        if (biome.LandFormFilters.Contains(landformtype))
        //        {
        //            //Check the temp range
        //            if (temperature >= biome.TemperatureFilter.Min &&
        //                temperature <= biome.TemperatureFilter.Max)
        //            {
        //                if (moisture >= biome.MoistureFilter.Min &&
        //                    moisture <= biome.MoistureFilter.Max)
        //                {
        //                    return biomeId;
        //                }
        //            }
        //        }
        //    }

        //    //By default return the first Biome from the list
        //    return (byte)0;
        //}

        public byte GetBiome(double landFormType, double temperature, double moisture, double zone)
        {
            enuLandFormType landformtype = (enuLandFormType)landFormType;

            List<List<Biome>> biomeList = _biomesConfig[landformtype];

            foreach (var biomesWithSameWeatherHash in biomeList)
            {
                Biome biome = biomesWithSameWeatherHash[0];
                //Check the temp range
                if (biome.isWeatherBiomes)
                {
                    if (temperature >= biome.TemperatureFilter.Min &&
                        temperature <= biome.TemperatureFilter.Max &&
                        moisture >= biome.MoistureFilter.Min &&
                        moisture <= biome.MoistureFilter.Max)
                    {
                        if (biomesWithSameWeatherHash.Count == 1) return biome.Id;
                        //Compute cell
                        byte zoneLayer = (byte)(zone * (biomesWithSameWeatherHash.Count - 1));
                        return biomesWithSameWeatherHash[zoneLayer].Id;
                    }
                }
                else
                {
                    if (biomesWithSameWeatherHash.Count == 1) return biome.Id;
                    //Compute cell
                    byte zoneLayer = (byte)(zone * (biomesWithSameWeatherHash.Count - 1));
                    return biomesWithSameWeatherHash[zoneLayer].Id;
                }
            }

            //By default return the first Biome from the list
            return (byte)0;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            //Assign configuration list position as ID for biome
            byte id = 0;
            foreach (var biome in _config.ProcessorParam.Biomes)
            {
                biome.Id = id;
                id++;
            }

            //Create a biome list per landformtype;
            Dictionary<enuLandFormType, List<Biome>> biomesPerType = new Dictionary<enuLandFormType, List<Biome>>();
            //For each biomes created in the editor
            foreach(var biome in _config.ProcessorParam.Biomes)
            {
                //Take all landscape type where it can spawn
                foreach (var type in biome.LandFormFilters)
                {
                    List<Biome> biomes;

                    if (!biomesPerType.TryGetValue(type, out biomes))
                    {
                        biomes = new List<Biome>();
                        biomesPerType[type] = biomes;
                    }

                    biomes.Add(biome);
                }
            }


            _biomesConfig = new Dictionary<enuLandFormType, List<List<Biome>>>();
            foreach (var kvp in biomesPerType)
            {
                List<Biome> biomesWithSameWeatherHash;
                //Take all the various weatherHash from the biomes within this enuLandFormType
                foreach (int weatherHash in kvp.Value.Select(x => x.WeatherHash).Distinct().OrderByDescending(x => x))
                {
                    biomesWithSameWeatherHash = new List<Biome>(kvp.Value.Where(x => x.WeatherHash == weatherHash));

                    //Add this List to the Main dictionnary
                    List<List<Biome>> allBiomesFromLandscapeType;
                    if (!_biomesConfig.TryGetValue(kvp.Key, out allBiomesFromLandscapeType))
                    {
                        allBiomesFromLandscapeType = new List<List<Biome>>();
                        _biomesConfig[kvp.Key] = allBiomesFromLandscapeType;
                    }
                    allBiomesFromLandscapeType.Add(biomesWithSameWeatherHash);
                }
            }
        }
        #endregion
    }

}
