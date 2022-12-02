using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Diffusion.Common
{


    public class Configuration<T>
    {
        private readonly string _settingsPath;

        public string SettingsPath => _settingsPath;

        public Configuration(string appName, string configName = "config.json")
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsPath = Path.Combine(local, appName, configName);
        }

        public bool TryLoad(out T? obj)
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    obj = JsonSerializer.Deserialize<T>(json);
                    return true;
                }

                obj = default(T);

                return false;
            }
            catch (Exception e)
            {
                obj = default(T);

                return false;
            }
        }

        public void Save(T obj)
        {
            var path = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true });

            File.WriteAllText(_settingsPath, json);
        }
    }
}
