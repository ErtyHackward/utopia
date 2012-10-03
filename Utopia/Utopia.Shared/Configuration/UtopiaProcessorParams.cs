using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors.Utopia;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Class that will hold the various parameters needed for landscape generation by the processor Utopia
    /// </summary>
    public class UtopiaProcessorParams : IBinaryStorable
    {
        public static class DefaultConfig
        {
            public const int BasicPlain_Flat = 0;
            public const int BasicPlain_Plain = 1;
            public const int BasicPlain_Hill = 2;
            public const int BasicMidLand_Midland = 0;
            public const int BasicMontain_Montain = 0;
            public const int BasicOcean_Ocean = 0;

            public const int Ground_BasicPlain = 0;
            public const int Ground_BasicMidLand = 1;
            public const int Ground_BasicMontain = 2;

            public const int Ocean_BasicOcean = 0;
            
            public const int World_Ocean = 0;
            public const int World_Ground = 1;
        }

        #region Private Variables
        #endregion

        #region Public Properties
        public List<LandscapeRange> BasicPlain { get; set; }
        public List<LandscapeRange> BasicMidland { get; set; }
        public List<LandscapeRange> BasicMontain { get; set; }
        public List<LandscapeRange> BasicOcean { get; set; }
        public List<LandscapeRange> Ground { get; set; }
        public List<LandscapeRange> Ocean { get; set; }
        public List<LandscapeRange> World { get; set; }
        #endregion

        public UtopiaProcessorParams()
        {
        }

        #region Public Methods
        public void CreateDefaultConfiguration()
        {
            //Create BasicPlain
            BasicPlain = new List<LandscapeRange>();
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Flat",
                Color = Color.AliceBlue,
                Size = 0.2,
                MixedNextArea = 0.05
            });
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Plain",
                Color = Color.YellowGreen,
                Size = 0.5,
                MixedNextArea = 0.05
            });
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Hill",
                Color = Color.Tomato,
                Size = 0.3
            });

            //Create BasicMidLand
            BasicMidland = new List<LandscapeRange>();
            BasicMidland.Add(new LandscapeRange()
            {
                Name = "Midland",
                Color = Color.Wheat,
                Size = 1
            });
            //Create BasicMontain
            BasicMontain = new List<LandscapeRange>();
            BasicMontain.Add(new LandscapeRange()
            {
                Name = "Montain",
                Color = Color.Brown,
                Size = 1
            });
            //Create BasicOcean
            BasicOcean = new List<LandscapeRange>();
            BasicOcean.Add(new LandscapeRange()
            {
                Name = "Ocean",
                Color = Color.Navy,
                Size = 1
            });



            //Create Ground 
            Ground = new List<LandscapeRange>();
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicPlain",
                Color = Color.Green,
                Size = 0.4,
                MixedNextArea = 0.05
            });
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicMidLand",
                Color = Color.YellowGreen,
                Size = 0.3,
                MixedNextArea = 0.05
            });
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicMontain",
                Color = Color.Brown,
                Size = 0.3
            });


            //Create Ocean
            Ocean = new List<LandscapeRange>();
            Ocean.Add(new LandscapeRange()
            {
                Name = "BasicOcean",
                Color = Color.Navy,
                Size = 1
            });


            //Create World
            World = new List<LandscapeRange>();
            World.Add(new LandscapeRange()
            {
                Name = "Ocean",
                Color = Color.Navy,
                Size = 0.1,
                MixedNextArea = 0.02
            });
            World.Add(new LandscapeRange()
            {
                Name = "Ground",
                Color = Color.Gold,
                Size = 0.9
            });
        }
        #endregion

        #region Private Methods
        #endregion

        public void Save(System.IO.BinaryWriter writer)
        {
        }

        public void Load(System.IO.BinaryReader reader)
        {
        }
    }
}
