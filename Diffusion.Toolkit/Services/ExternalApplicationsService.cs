using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class ExternalApplicationsService
{

    public IReadOnlyCollection<ExternalApplication> ExternalApplications =>
        ServiceLocator.Settings.ExternalApplications;

    public bool HasExternalApplications => ServiceLocator.Settings.ExternalApplications is { Count: > 0 };

    public async Task OpenWith(int index)
    {
        if (HasExternalApplications && index <= ServiceLocator.Settings.ExternalApplications.Count)
        {
            await OpenWith(ServiceLocator.Settings.ExternalApplications[index - 1]);
        }
    }

    public async Task OpenWith(ExternalApplication externalApplication)
    {
        string args = "%1";

        if (!string.IsNullOrEmpty(externalApplication.CommandLineArgs))
        {
            args = externalApplication.CommandLineArgs;
        }

        var images = string.Join(" ", ServiceLocator.MainModel.SelectedImages.Select(d => $"\"{d.Path}\""));

        var appPath = externalApplication.Path;

        args = args.Replace("%1", images);

        if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
        {
            await Task.Run(() =>
            {
                var ps = new ProcessStartInfo()
                {
                    FileName = appPath,
                    Arguments = args,
                    UseShellExecute = true
                };

                Process.Start(ps);
            });
        }
        else
        {
            await ServiceLocator.MessageService.ShowMedium($"Failed to launch the application {externalApplication.Name}.\r\n\r\nPath not found", "Error opening External Application", PopupButtons.OK);
        }
    }
}