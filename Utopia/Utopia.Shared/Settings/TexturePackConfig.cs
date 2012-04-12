using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Config;
using System.Xml.Serialization;
using SharpDX.Direct3D11;
using Utopia.Shared.GameDXStates;

namespace Utopia.Shared.Settings
{
    public class TexturePackConfig
    {
        public static XmlSettingsManager<TexturePackSetting> Current;
    }

    //Class that will hold all settings attached to the game.
    [XmlRoot("TexturePackConfig")]
    [Serializable]
    public class TexturePackSetting : IConfigClass
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private string _samplingFilter;
        private string _texMipCreationFiltering;
        #endregion

        #region Public variables/Properties
        /// <summary>
        /// Game parameters section
        /// </summary>
        public string PackageName { get; set; }
        public string TexMipCreationFiltering
        {
            get { return _texMipCreationFiltering; }
            set { _texMipCreationFiltering = value; ParseTexMipCreationFiltering(value); }
        }
        public string SamplingFilter
        {
            get { return _samplingFilter; }
            set { _samplingFilter = value; ParseSamplingFilter(value); }
        }

        [XmlIgnore]
        public int enuTexMipCreationFilteringId;
        [XmlIgnore]
        public FilterFlags enuSamplingFilter;

        #endregion

        public TexturePackSetting()
        {
        }

        #region Public methods
        #endregion

        #region Private methods
        private void ParseTexMipCreationFiltering(string MipFilter)
        {
            try
            {
                enuSamplingFilter = (FilterFlags)Enum.Parse(typeof(FilterFlags), MipFilter);
            }
            catch (Exception)
            {
                //Set Default value for Filter
                logger.Error("Error while trying to get FilterFlags from string value {0}, default value has been set to FilterFlags.Point", MipFilter);
                enuSamplingFilter = FilterFlags.Point;
            }
        }

        public void ParseSamplingFilter(string samplingFilter)
        {
            samplingFilter = "UVWrap_" + samplingFilter;
            Type type = typeof(DXStates.Samplers);
            //Set Default value
            enuTexMipCreationFilteringId = DXStates.Samplers.UVWrap_MinMagPointMipLinear;
            foreach (var field in type.GetFields())
            {
                if (field.Name == samplingFilter)
                {
                    enuTexMipCreationFilteringId = (int)field.GetValue(null);
                    return;
                }
            }
        }
        #endregion

        public void Initialize()
        {
        }
    }
}
