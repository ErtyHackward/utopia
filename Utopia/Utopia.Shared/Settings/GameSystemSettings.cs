using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Utopia.Shared.Config;

namespace Utopia.Shared.Settings
{

    public class GameSystemSettings
    {
        public static XmlSettingsManager<GameSystemSetting> Current;
    }

    //Class that will old all settings attached to the game.
    [XmlRoot("GameSystemSettings")]
    [Serializable]
    public class GameSystemSetting
    {
        #region Private variables
        #endregion

        #region Public variables/Properties
        /// <summary>
        /// Game parameters section
        /// </summary>
        [XmlElement("Cube")]
        public List<CubeProfileNEW> Servers = new List<CubeProfileNEW>();

        /// <summary>
        /// Dictionnary Profiles acces
        /// </summary>
        //public static Dictionary<int, EntityProfile> EntityProfiles = new Dictionary<int, EntityProfile>();
        #endregion

        public GameSystemSetting()
        {
        }

        #region Public methods
        public void AddCubeProfiles(string XMLCubeFilePath)
        {
        }

        public void AddEntitiesProfiles(string XMLEntityFilePath)
        {
        }
        #endregion

        #region Private methods
        #endregion
    }
}
