using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Config
{
    /// <summary>
    /// Allows to store user settings
    /// </summary>
    [DebuggerStepThrough]
    public class XmlSettingsManager<T> where T : IConfigClass, new()
    {
        private readonly XmlSerializer _xmlSerializer;
        private string _customSettingsFolderPath;

        /// <summary>
        /// Gets or sets settings file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets storage place for settings
        /// </summary>
        public SettingsStorage Storage { get; set; }

        /// <summary>
        /// Gets or sets settings instance
        /// </summary>
        public T Settings { get; set; }


        public XmlSettingsManager(string fileName) : this(fileName, SettingsStorage.ApplicationData)
        {
            
        }

        /// <summary>
        /// Creates new instance of XmlSettings manager
        /// </summary>
        /// <param name="fileName">Settings file name without path</param>
        /// <param name="storage">Storage place</param>
        /// <param name="customFolder"></param>
        public XmlSettingsManager(string fileName, SettingsStorage storage = SettingsStorage.ApplicationData, string CustomSettingsFolderPath = null)
        {
            FileName = fileName;
            Storage = storage;
            _customSettingsFolderPath = CustomSettingsFolderPath;

            _xmlSerializer = new XmlSerializer(typeof(T));
        }

        /// <summary>
        /// Saves settings to file
        /// </summary>
        public void Save()
        {
            using (var fileStream = File.Open(GameSystemSettings.GetFilePath(FileName, Storage, true, _customSettingsFolderPath != null ? _customSettingsFolderPath : null), FileMode.Create))
                _xmlSerializer.Serialize(fileStream, Settings);
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public void Load()
        {
            string path = string.Empty;
            try
            {
                path = GameSystemSettings.GetFilePath(FileName, Storage, true, _customSettingsFolderPath != null ? _customSettingsFolderPath:null);

                if (File.Exists(path))
                {
                    using (var fileStream = File.OpenRead(path))
                        Settings = (T)_xmlSerializer.Deserialize(fileStream);
                }
                else
                {
                    LoadDefault();
                }

                Settings.Initialize();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error loading configuration file : " + path + Environment.NewLine + "Error : " + e.InnerException);
                throw;
            }
        }

        /// <summary>
        /// Initializes new instance of Settings class and sets all it's values to default (no I/O operation performed)
        /// </summary>
        public void LoadDefault()
        {
            Settings = new T();
            foreach (var propertyInfo in Settings.GetType().GetProperties())
            {
                foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                {
                    if (attribute is DefaultValueAttribute)
                    {
                        var dv = attribute as DefaultValueAttribute;
                        propertyInfo.SetValue(Settings, dv.Value, null);
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the settings file exists
        /// </summary>
        /// <param name="storage"></param>
        /// <returns>Returns true is file exists otherwise return false</returns>
        public bool SettingsExists(SettingsStorage storage)
        {
            return File.Exists(GameSystemSettings.GetFilePath(FileName, storage, false, _customSettingsFolderPath != null ? _customSettingsFolderPath : null));
        }

    }

}
