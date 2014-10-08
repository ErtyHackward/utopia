using System.Collections.Generic;
using System.IO;

namespace Utopia.Shared.Server.Utils
{
    public class IniSettings
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        private readonly string _settingsFilePath;
        
        public IniSettings(string filepath)
        {
            _settingsFilePath = filepath;
            Load();
        }

        public string this[string settingName]
        {
            get
            {
                if (_settings.ContainsKey(settingName))
                    return _settings[settingName];
                return string.Empty;
            }
            set
            {
                if (_settings.ContainsKey(settingName))
                    _settings[settingName] = value;
                else _settings.Add(settingName, value);
            }
        }

        public string GetString(string settingName, string defaultValue = "")
        {
            if (_settings.ContainsKey(settingName))
                return _settings[settingName];
            return defaultValue;
        }

        public int GetInt(string settingName, int defaultValue = 0)
        {
            if (_settings.ContainsKey(settingName))
                return int.Parse(_settings[settingName]);
            return defaultValue;
        }

        public void SetInt(string settingName, int value)
        {
            if (_settings.ContainsKey(settingName))
                _settings[settingName] = value.ToString();
            else _settings.Add(settingName, value.ToString());
        }

        public void Save()
        {
            try
            {
                var di = new DirectoryInfo(Path.GetDirectoryName(_settingsFilePath));
                if (!di.Exists) di.Create();

                using (
                    var fs =
                        new FileStream(_settingsFilePath, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        foreach (KeyValuePair<string, string> pair in _settings)
                        {
                            sw.WriteLine("{0}={1}", pair.Key, pair.Value);
                        }
                    }
                }
            }
            catch { }
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_settingsFilePath)) return;
                _settings.Clear();
                using (var fs = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string data = sr.ReadToEnd();
                        string[] lines = data.Split('\n');
                        foreach (string line in lines)
                        {
                            int i = line.IndexOf('=');
                            if (i != -1)
                            {
                                _settings.Add(line.Substring(0, i), line.Substring(i + 1).Trim());

                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
