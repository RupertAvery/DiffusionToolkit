using System;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    public partial class ThumbnailView
    {
        private async void SearchModel(object obj)
        {
            if (Model.CurrentImage?.ModelHash == null) return;
            
            var hash = Model.CurrentImage.ModelHash;

            using (var client = new CivitaiClient())
            {
                try
                {
                    var modelVersion = await client.GetModelVersionsByHashAsync(hash, CancellationToken.None);

                    Process.Start("explorer.exe", $"\"https://civitai.com/models/{modelVersion.ModelId}?modelVersionId={modelVersion.Id}\"");
                }
                catch (CivitaiRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    var message = "The requested model hash was not found";
                    await ServiceLocator.MessageService.Show(message, "Search Model", PopupButtons.OK);
                }
            }
        }
        
    }
}
