using Microsoft.Win32;
using System;
using System.Windows;

namespace Diffusion.Toolkit.Themes
{
    public static class ThemeManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";

        public static string CurrentTheme { get; private set; }

        public static void ChangeTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName) || themeName  == "System")
            {
                themeName = GetWindowsTheme();
            }

            CurrentTheme = themeName;

            var app = (App)Application.Current;
            app.Resources.MergedDictionaries.Clear();

            LoadResourceDictionary(app, "Themes/ToolTips.xaml");
            LoadResourceDictionary(app, $"Themes/{themeName}.xaml");
            LoadResourceDictionary(app, "Themes/Common.xaml");
            LoadResourceDictionary(app, "Themes/Menu.xaml");
            LoadResourceDictionary(app, "Themes/SWStyles.xaml");
            LoadResourceDictionary(app, "Themes/Scrollbars.xaml");
            LoadResourceDictionary(app, "Themes/Window.xaml");
        }

        private static void LoadResourceDictionary(App app, string url)
        {
            var resource = (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative));
            app.Resources.MergedDictionaries.Add(resource);
        }

        private static string GetWindowsTheme()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            var registryValueObject = key?.GetValue(RegistryValueName);
            if (registryValueObject == null)
            {
                return "Light";
            }
            var registryValue = (int)registryValueObject;

            return registryValue > 0 ? "Light" : "Dark";
        }

    }
}
