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
            ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri($"Themes/{themeName}.xaml", UriKind.Relative));
            ResourceDictionary common = (ResourceDictionary)Application.LoadComponent(new Uri("Themes/Common.xaml", UriKind.Relative));
            ResourceDictionary menu = (ResourceDictionary)Application.LoadComponent(new Uri($"Themes/Menu.xaml", UriKind.Relative));
            ResourceDictionary window = (ResourceDictionary)Application.LoadComponent(new Uri($"Themes/Window.xaml", UriKind.Relative));
            app.Resources.MergedDictionaries.Add(theme);
            app.Resources.MergedDictionaries.Add(common);
            app.Resources.MergedDictionaries.Add(menu);
            app.Resources.MergedDictionaries.Add(window);
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
