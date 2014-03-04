using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    //Holder that keep all biomes that have the same Weather parameters together
    public class WeatherBiomes
    {
        public List<Biome> BiomesListe;
        public int TotalBiomesWeight;
        public int TotalBiomes;
        public RangeD Temperature;
        public RangeD Moisture;
        public bool isWeatherBiomes;
    }

    public class BiomeHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private UtopiaWorldConfiguration _config;
        private Dictionary<enuLandFormType, List<WeatherBiomes>> _biomesConfig;
        #endregion

        #region Public Properties
        #endregion

        public BiomeHelper(UtopiaWorldConfiguration config)
        {
            _config = config;
            Initialize();
        }

        #region Public Methods
        public byte GetBiome(double landFormType, double temperature, double moisture, double zone)
        {
            switch (_config.Version)
            {
                case 0:
                    return GetBiomeV0(landFormType, temperature, moisture);
                case 1:
                    return GetBiomeV1(landFormType, temperature, moisture, zone);
            }

            //By default return the first Biome from the list
            return (byte)0;
        }

        private byte GetBiomeV0(double landFormType, double temperature, double moisture)
        {
            enuLandFormType landformtype = (enuLandFormType)landFormType;

            for (byte biomeId = 0; biomeId <= _config.ProcessorParam.Biomes.Count - 1; biomeId++)
            {
                Biome biome = _config.ProcessorParam.Biomes[biomeId];
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

        private byte GetBiomeV1(double landFormType, double temperature, double moisture, double zone)
        {
            enuLandFormType landformtype = (enuLandFormType)landFormType;

            List<WeatherBiomes> biomeList;
            if (!_biomesConfig.TryGetValue(landformtype, out biomeList)) return (byte)0;

            foreach (var weatherBiome in biomeList)
            {
                //Check the temp range
                if (weatherBiome.isWeatherBiomes)
                {
                    if (temperature >= weatherBiome.Temperature.Min &&
                        temperature <= weatherBiome.Temperature.Max &&
                        moisture >= weatherBiome.Moisture.Min &&
                        moisture <= weatherBiome.Moisture.Max)
                    {
                        if (weatherBiome.TotalBiomes == 1) return weatherBiome.BiomesListe[0].Id;
                        //Compute cell
                        //byte zoneLayer = (byte)(zone * (weatherBiome.Count));
                        //if (zoneLayer == weatherBiome.Count) zoneLayer--;
                        //return weatherBiome[zoneLayer].Id;
                        return GetBiomeId(weatherBiome, zone);
                    }
                }
                else
                {
                    if (weatherBiome.TotalBiomes == 1) return weatherBiome.BiomesListe[0].Id;
                    //Compute cell
                    //byte zoneLayer = (byte)(zone * (weatherBiome.Count));
                    //if (zoneLayer == weatherBiome.Count) zoneLayer--;
                    //return weatherBiome[zoneLayer].Id;
                    return GetBiomeId(weatherBiome, zone);
                }
            }

            //By default return the first Biome from the list
            return (byte)0;
        }

        private byte GetBiomeId(WeatherBiomes wb, double zone)
        {
            int threeshold = (int)(zone * wb.TotalBiomesWeight);
            int runningWeight = 0;
            foreach (var biome in wb.BiomesListe)
            {
                runningWeight += biome.ZoneWeight;
                if (runningWeight > threeshold) return biome.Id;
            }
            return (byte)255; 
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


            _biomesConfig = new Dictionary<enuLandFormType, List<WeatherBiomes>>();
            foreach (var kvp in biomesPerType)
            {
                WeatherBiomes biomesWithSameWeatherHash;
                //Take all the various weatherHash from the biomes within this enuLandFormType
                foreach (int weatherHash in kvp.Value.Select(x => x.WeatherHash).Distinct().OrderByDescending(x => x))
                {
                    biomesWithSameWeatherHash = new WeatherBiomes() { BiomesListe = new List<Biome>(kvp.Value.Where(x => x.WeatherHash == weatherHash).OrderByDescending(x => x.ZoneWeight)) };
                    biomesWithSameWeatherHash.isWeatherBiomes = biomesWithSameWeatherHash.BiomesListe[0].isWeatherBiomes;
                    biomesWithSameWeatherHash.Moisture = biomesWithSameWeatherHash.BiomesListe[0].MoistureFilter;
                    biomesWithSameWeatherHash.Temperature = biomesWithSameWeatherHash.BiomesListe[0].TemperatureFilter;
                    biomesWithSameWeatherHash.TotalBiomes = biomesWithSameWeatherHash.BiomesListe.Count;
                    biomesWithSameWeatherHash.TotalBiomesWeight = biomesWithSameWeatherHash.BiomesListe.Sum(x => x.ZoneWeight);

                    //Add this List to the Main dictionnary
                    List<WeatherBiomes> allBiomesFromLandscapeType;
                    if (!_biomesConfig.TryGetValue(kvp.Key, out allBiomesFromLandscapeType))
                    {
                        allBiomesFromLandscapeType = new List<WeatherBiomes>();
                        _biomesConfig[kvp.Key] = allBiomesFromLandscapeType;
                    }
                    allBiomesFromLandscapeType.Add(biomesWithSameWeatherHash);
                }
            }
        }
        #endregion
    }

}
