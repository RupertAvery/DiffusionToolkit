using Microsoft.Win32;
using System;
using System.Windows;

namespace Diffusion.Toolkit.Themes
{
    public static class ThemeManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";

        public static void ChangeTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName) || themeName  == "System")
            {
                themeName = GetWindowsTheme();
            }

            var app = (App)Application.Current;
            app.Resources.MergedDictionaries.Clear();

            LoadResource(app, "Themes/ToolTips.xaml");
            LoadResource(app, $"Themes/{themeName}.xaml");
            LoadResource(app, "Themes/Common.xaml");
            LoadResource(app, "Themes/Menu.xaml");
            LoadResource(app, "Themes/SWStyles.xaml");
            LoadResource(app, "Themes/Window.xaml");
        }

        private static void LoadResource(App app, string url)
        {
            ResourceDictionary resource = (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative));
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
