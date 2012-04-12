using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Utopia.Shared.Config;
using System.IO;
using System.Reflection;

namespace Utopia.Shared.Settings
{
    /// <summary>
    /// Enumerates all possible storage states
    /// </summary>
    public enum SettingsStorage
    {
        /// <summary>
        /// Path where executing assembly located
        /// </summary>
        LocalFolder,
        /// <summary>
        /// Custom path
        /// </summary>
        CustomPath,
        /// <summary>
        /// User specific location
        /// </summary>
        ApplicationData,
        /// <summary>
        /// Computer specific location
        /// </summary>
        CommonApplicationData
    }

    public class GameSystemSettings
    {
        public static XmlSettingsManager<GameSystemSetting> Current;

        private static string _applicationDataSubFolder;
        /// <summary>
        /// Gets or sets subfolder used with ApplicationData or CommonApplicationData storage locations
        /// </summary>
        public static string ApplicationDataSubFolder
        {
            get
            {
                return _applicationDataSubFolder ??
                       (_applicationDataSubFolder = (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute)) as AssemblyProductAttribute).Product);
            }
            set { _applicationDataSubFolder = value; }
        }

        public static string GetFilePath(string FileName, string storagePath, bool createDirectory = true)
        {
            return GetFilePath(FileName, SettingsStorage.CustomPath, createDirectory, storagePath);
        }

        public static string GetFilePath(string FileName, SettingsStorage storage, bool createDirectory = true, string CustomSettingsFolderPath = null)
        {
            string folder;
            switch (storage)
            {
                //case SettingsStorage.LocalFolder: folder = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; break;
                case SettingsStorage.LocalFolder: folder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); break;
                case SettingsStorage.CustomPath: folder = CustomSettingsFolderPath; break;
                case SettingsStorage.ApplicationData: folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubFolder); break;
                case SettingsStorage.CommonApplicationData: folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationDataSubFolder); break;
                default: folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
            }

            //If the file name contain directory info, add them inside the folder value
            if (FileName.Contains('\\'))
            {
                folder = Path.Combine(folder, FileName.Substring(0, FileName.LastIndexOf('\\')));
                FileName = FileName.Substring(FileName.LastIndexOf('\\') + 1, FileName.Length - FileName.LastIndexOf('\\') - 1);
            }

            if (createDirectory && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, FileName);
        }
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
