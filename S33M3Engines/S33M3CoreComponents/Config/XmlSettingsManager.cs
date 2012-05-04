using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Linq;

namespace S33M3CoreComponents.Config
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

    public static class XmlSettingsManager
    {
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

    /// <summary>
    /// Allows to store user settings
    /// </summary>
    //[DebuggerStepThrough]
    public class XmlSettingsManager<T> where T : IConfigClass, new()
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            using (var fileStream = File.Open(XmlSettingsManager.GetFilePath(FileName, Storage, true, _customSettingsFolderPath != null ? _customSettingsFolderPath : null), FileMode.Create))
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
                path = XmlSettingsManager.GetFilePath(FileName, Storage, true, _customSettingsFolderPath != null ? _customSettingsFolderPath : null);

                if (File.Exists(path))
                {
                    logger.Info("Loading existing configuration file : {0}", path);

                    using (var fileStream = File.OpenRead(path))
                        Settings = (T)_xmlSerializer.Deserialize(fileStream);
                }
                else
                {
                    logger.Info("Configuration file not existing ({0}), loading default settings", path);

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
            return File.Exists(XmlSettingsManager.GetFilePath(FileName, storage, false, _customSettingsFolderPath != null ? _customSettingsFolderPath : null));
        }

    }

}
