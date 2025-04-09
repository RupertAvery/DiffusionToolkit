using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class ContextMenuService
{

    public ContextMenuService()
    {
        if (ServiceLocator.Settings != null)
        {
            ServiceLocator.Settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Settings.ExternalApplications))
                {
                    BuildOpenWithContextMenu();
                }
            };
        }

        BuildOpenWithContextMenu();
    }

    public void Go()
    {
        // Dummy method to instantiate this class
    }

    private void BuildOpenWithContextMenu()
    {
        ServiceLocator.MainModel.OpenWithMenuItems = new ObservableCollection<Control>();

        if (ServiceLocator.ExternalApplicationsService.HasExternalApplications)
        {
            var index = 1;
            foreach (var externalApplication in ServiceLocator.ExternalApplicationsService.ExternalApplications)
            {
                var menuItem = new MenuItem()
                {
                    Header = externalApplication.Name,
                    InputGestureText = $"Shift+{index}"
                };

                menuItem.Click += (o, eventArgs) =>
                {
                    _ = ServiceLocator.ExternalApplicationsService.OpenWith(externalApplication);
                };

                index++;

                ServiceLocator.MainModel.OpenWithMenuItems.Add(menuItem);
            }
        }
    }

    private ImageViewModel CurrentImage => ServiceLocator.MainModel.CurrentImage;


    public void CopyPath(object obj)
    {
        if (CurrentImage?.Path == null) return;
        var p = CurrentImage.Path;
        Clipboard.SetDataObject(p, true);
        ServiceLocator.ToastService.Toast("Copied path to clipboard", "");
    }

    public void CopyPrompt(object obj)
    {
        if (CurrentImage.Prompt == null) return;
        var p = CurrentImage.Prompt;
        Clipboard.SetDataObject(p, true);
        ServiceLocator.ToastService.Toast("Copied prompt to clipboard", "");
    }

    public void CopyNegative(object obj)
    {
        if (CurrentImage.NegativePrompt == null) return;
        var p = CurrentImage.NegativePrompt;
        Clipboard.SetDataObject(p, true);
        ServiceLocator.ToastService.Toast("Copied negative prompt to clipboard", "");
    }

    public void CopySeed(object obj)
    {
        if (CurrentImage.Seed == null) return;
        var p = CurrentImage.Seed.ToString();
        Clipboard.SetDataObject(p, true);
        ServiceLocator.ToastService.Toast("Copied seed to clipboard", "");
    }

    public void CopyHash(object obj)
    {
        if (CurrentImage.ModelHash == null) return;
        var p = CurrentImage.ModelHash;
        Clipboard.SetDataObject(p, true);
        ServiceLocator.ToastService.Toast("Copied hash to clipboard", "");
    }

    public void CopyParameters(object obj)
    {
        var p = CurrentImage.Prompt;
        var n = CurrentImage.NegativePrompt;
        var o = CurrentImage.OtherParameters;
        var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

        Clipboard.SetDataObject(parameters, true);
        ServiceLocator.ToastService.Toast("Copied all parameters to clipboard", "");
    }

    public void CopyOthers(object obj)
    {
        if (CurrentImage.OtherParameters == null) return;

        var o = CurrentImage.OtherParameters;

        Clipboard.SetDataObject(o, true);

        ServiceLocator.ToastService.Toast("Copied other parameters to clipboard", "");
    }

}