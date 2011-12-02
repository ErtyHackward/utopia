using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Utopia.Shared.Config
{
    /// <summary>
    /// Allows to store user settings
    /// </summary>
    [DebuggerStepThrough]
    public class XmlSettingsManager<T> where T : IConfigClass, new()
    {
        private readonly XmlSerializer _xmlSerializer;

        /// <summary>
        /// Gets or sets custom folder path (use with Storage = CustomPath)
        /// </summary>
        public string CustomSettingsFolderPath { get; set; }

        private string _applicationDataSubFolder;

        /// <summary>
        /// Gets or sets subfolder used with ApplicationData or CommonApplicationData storage locations
        /// </summary>
        public string ApplicationDataSubFolder
        {
            get
            {
                return _applicationDataSubFolder ??
                       (_applicationDataSubFolder = (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute)) as AssemblyProductAttribute).Product);
            }
            set { _applicationDataSubFolder = value; }
        }

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

        private string GetFilePath(SettingsStorage storage, bool createDirectory = true)
        {
            string folder;
            switch (storage)
            {
                //case SettingsStorage.LocalFolder: folder = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; break;
                case SettingsStorage.LocalFolder: folder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); break;
                case SettingsStorage.CustomPath: folder = CustomSettingsFolderPath; break;
                case SettingsStorage.ApplicationData: folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),ApplicationDataSubFolder); break;
                case SettingsStorage.CommonApplicationData: folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationDataSubFolder); break;
                default: folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
            }

            if(createDirectory && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, FileName);
        }


        /// <summary>
        /// Creates new instance of XmlSettings manager
        /// </summary>
        /// <param name="fileName">Settings file name without path</param>
        /// <param name="storage">Storage place</param>
        /// <param name="customFolder"></param>
        public XmlSettingsManager(string fileName, SettingsStorage storage = SettingsStorage.ApplicationData, string customFolder = "")
        {
            FileName = fileName;
            Storage = storage;
            CustomSettingsFolderPath = customFolder;

            _xmlSerializer = new XmlSerializer(typeof(T));
        }

        /// <summary>
        /// Saves settings to file
        /// </summary>
        public void Save()
        {
            using (var fileStream = File.Open(GetFilePath(Storage), FileMode.Create))
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
                path = GetFilePath(Storage);

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
            return File.Exists(GetFilePath(storage, false));
        }

    }

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

}
