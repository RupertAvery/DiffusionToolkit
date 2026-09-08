using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Diffusion.Common;
using Diffusion.Toolkit.Configuration;
using WPFLocalizeExtension.Providers;

namespace Diffusion.Toolkit.Localization
{
    public class JsonLocalizationProvider : FrameworkElement, ILocalizationProvider
    {
        private readonly Dictionary<string, Dictionary<string, string>> _dictionaries;
        private readonly Dictionary<string, string> _defaultDictionary;

        public static JsonLocalizationProvider Instance { get; private set; }

        public JsonLocalizationProvider()
        {
            Instance = this;
            
            var localizationPath = Path.Combine(AppInfo.AppDir, "Localization");
            
            var files = Directory.GetFiles(localizationPath, "*.json");

            _defaultDictionary = new Dictionary<string, string>();
            _dictionaries = new Dictionary<string, Dictionary<string, string>>();

            var assembly = typeof(JsonLocalizationProvider).Assembly;

            using (var defaultStream = assembly.GetManifestResourceStream("Diffusion.Toolkit.Localization.default.json"))
            {
                var reader = new StreamReader(defaultStream);
                var json = reader.ReadToEnd();
                _defaultDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                _dictionaries.Add(key, dictionary);
            }
        }

        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            return new FQAssemblyDictionaryKey(key, null, null);
        }

        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            var currentCulture = Settings.Instance?.Culture ?? Thread.CurrentThread.CurrentCulture.Name;

            //File.AppendAllText("C:\\Temp\\DiffusionToolkit.log", key + "\r\n");

            if (_dictionaries.TryGetValue(currentCulture, out var dictionary))
            {
                if (dictionary.TryGetValue(key, out var value))
                {
                    return value;
                }

                if (_defaultDictionary.TryGetValue(key, out value))
                {
                    return value;
                }
            }
            else
            {
                if (_defaultDictionary.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            
            
            return $"Key: {key}";
        }

        public ObservableCollection<CultureInfo> AvailableCultures
        {
            get
            {
                if (field == null)
                    field = new ObservableCollection<CultureInfo>();

                return field;
            }
        } = null;

        public event ProviderChangedEventHandler? ProviderChanged;

        public event ProviderErrorEventHandler? ProviderError;

        public event ValueChangedEventHandler? ValueChanged;
    }
}
