using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using S33M3CoreComponents.Config;

namespace Utopia.Shared.Settings
{    
    public class GameSystemSettings
    {
        public static XmlSettingsManager<GameSystemSetting> Current;
        public static List<LocalWorlds.LocalWorldsParam> LocalWorldsParams;
    }

    [Serializable]
    public class EntityProfile
    {
        public int ClassID;
        public int SpriteID;
        public int NbrGrowSprites;
    }

    //Class that will old all settings attached to the game.
    [XmlRoot("GameSystemSettings")]
    [Serializable]
    public class GameSystemSetting : IConfigClass
    {
        #region Private variables
        #endregion

        #region Public variables/Properties
        /// <summary>
        /// Game parameters section
        /// </summary>
        [XmlElement("Cube")]
        public CubeProfile[] CubesProfile;
        [XmlElement("Entity")]
        public EntityProfile[] EntityProfileLst;

        //Derived from EntityProfile
        [XmlIgnore]
        public Dictionary<int, EntityProfile> EntityProfile = new Dictionary<int,EntityProfile>();
        #endregion

        public GameSystemSetting()
        {
        }

        #region Public methods
        public void CleanUp()
        {
            CubesProfile = null;
            EntityProfile.Clear();
            EntityProfile = null;
            EntityProfileLst = null;
        }

        public void Initialize()
        {
            foreach (var data in EntityProfileLst)
            {
                EntityProfile.Add(data.ClassID, data);
            }
        }
        #endregion

        #region Private methods
        #endregion
    }
}
