using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if PROGPU
using System.Windows.Media.ProGPU.Platform;
#else
using Microsoft.WindowsAPICodePack.Dialogs;
#endif

namespace Diffusion.Toolkit.Services
{
    public class DialogService
    {
        public async Task<string?> GetFolder(Window window)
        {
#if PROGPU
            var dialog = new ProcessWpfFileDialogService();
            var result = await dialog.PickFolderAsync();
            return result;
#else
            using var dialog = new CommonOpenFileDialog();

            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
#endif
        }

        public string? GetYesNo(Window window, string message, string caption)
        {
#if PROGPU
            var messageBoxService = new ProcessWpfMessageBoxService();

            return messageBoxService.Show(new WpfMessageBoxOptions()
            {
                MessageBoxText = message,
                Caption = caption,
                Button = "YesNo",
                Icon = "Question",
            });
#else
            var result = MessageBox.Show(window,
                message,
                caption, MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes ? "Yes" : "No";
#endif
        }

        public async Task<string?> GetFile(Window window, string title, string[] filters, string defaultExtension)
        {
#if PROGPU
            var dialog = new ProcessWpfFileDialogService();
            var result = await dialog.OpenFileAsync(new WpfFileDialogOptions()
            {
                AllowMultipleSelection = true,
                Title = title,
                FileTypePatterns = filters,
                //SuggestedFileName = defaultExtension
            });
            return result;
#else
            using var dialog = new CommonOpenFileDialog();

            dialog.Title = title;

            foreach (var filter in filters)
            {
                var parts = filter.Split('|');
                dialog.Filters.Add(new CommonFileDialogFilter(parts[0], $"*.{parts[1].ToLower()}"));
            }

            dialog.DefaultExtension = defaultExtension;

            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
#endif
        }

        public async Task<string[]?> GetFiles(Window window, string title, string[] filters, string defaultExtension)
        {
#if PROGPU
            var dialog = new ProcessWpfFileDialogService();
            var result = await dialog.OpenFilesAsync(new WpfFileDialogOptions()
            {
                AllowMultipleSelection = true,
                Title = title,
                FileTypePatterns = filters,
                //SuggestedFileName = defaultExtension
            });
            return result;
#else
            using var dialog = new CommonOpenFileDialog();

            dialog.Title = title;

            foreach (var filter in filters)
            {
                var parts = filter.Split('|');
                dialog.Filters.Add(new CommonFileDialogFilter(parts[0], $"*.{parts[1].ToLower()}"));
            }

            dialog.DefaultExtension = defaultExtension;

            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                return File.ReadAllLines(dialog.FileName);
            }

            return null;
#endif
        }

        public string Message(Window owner, string message, string caption, string buttons, string icon)
        {
            var dialog = new ProcessWpfFileDialogService();

            var buttonMapping = new Dictionary<string, MessageBoxButton>()
            {
                { "OK", MessageBoxButton.OK },
                { "OKCancel", MessageBoxButton.OKCancel },
                { "YesNo", MessageBoxButton.YesNo },
                { "YesNoCancel", MessageBoxButton.YesNoCancel }
            };

            var iconMapping = new Dictionary<string, MessageBoxImage>()
            {
                { "None", MessageBoxImage.None },
                { "Error", MessageBoxImage.Error },
                { "Question", MessageBoxImage.Question },
                { "Exclamation", MessageBoxImage.Exclamation },
                { "Information", MessageBoxImage.Information }
            };

            var resultMapping = new Dictionary<MessageBoxResult, string>()
            {
                { MessageBoxResult.None, "None" },
                { MessageBoxResult.OK, "OK" },
                { MessageBoxResult.Cancel, "Cancel" },
                { MessageBoxResult.Yes, "Yes" },
                { MessageBoxResult.No, "No" }
            };

            var result = MessageBox.Show(owner,
                message,
                caption, buttonMapping[buttons],
                iconMapping[icon]);

            return resultMapping[result];
        }

    }
}