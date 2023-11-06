using System;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using Diffusion.Toolkit.Models;

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
                    await MessagePopupManager.Show(message, "Search Model", PopupButtons.OK);
                }
            }
        }

        public Action<string> Toast { get; set; }

        private void CopyPath(object obj)
        {
            if (Model.CurrentImage?.Path == null) return;
            var p = Model.CurrentImage.Path;
            Clipboard.SetDataObject(p, true);
            Toast?.Invoke("Copied path to clipboard");
        }

        private void CopyPrompt(object obj)
        {
            if (Model.CurrentImage?.Prompt == null) return;
            var p = Model.CurrentImage.Prompt;
            Clipboard.SetDataObject(p, true);
            Toast?.Invoke("Copied prompt to clipboard");
        }

        private void CopyNegative(object obj)
        {
            if (Model.CurrentImage?.NegativePrompt == null) return;
            var p = Model.CurrentImage.NegativePrompt;
            Clipboard.SetDataObject(p, true);
            Toast?.Invoke("Copied negative prompt to clipboard");
        }

        private void CopySeed(object obj)
        {
            if (Model.CurrentImage?.Seed == null) return;
            var p = Model.CurrentImage.Seed.ToString();
            Clipboard.SetDataObject(p, true);
            Toast?.Invoke("Copied seed to clipboard");
        }

        private void CopyHash(object obj)
        {
            if (Model.CurrentImage?.ModelHash == null) return;
            var p = Model.CurrentImage.ModelHash;
            Clipboard.SetDataObject(p, true);
            Toast?.Invoke("Copied hash to clipboard");
        }

        private void CopyParameters(object obj)
        {
            var p = Model.CurrentImage.Prompt;
            var n = Model.CurrentImage.NegativePrompt;
            var o = Model.CurrentImage.OtherParameters;
            var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

            Clipboard.SetDataObject(parameters, true);
            Toast?.Invoke("Copied all parameters to clipboard");
        }

        private void CopyOthers(object obj)
        {
            if (Model.CurrentImage?.OtherParameters == null) return;

            var o = Model.CurrentImage.OtherParameters;

            Clipboard.SetDataObject(o, true);

            Toast?.Invoke("Copied other parameters to clipboard");
        }

    }
}
