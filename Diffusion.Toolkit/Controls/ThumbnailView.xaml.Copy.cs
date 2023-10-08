using System.Windows;

namespace Diffusion.Toolkit.Controls
{
    public partial class ThumbnailView
    {
        private void CopyPath(object obj)
        {
            if (Model.CurrentImage?.Path == null) return;
            var p = Model.CurrentImage.Path;
            Clipboard.SetDataObject(p, true);
        }

        private void CopyPrompt(object obj)
        {
            if (Model.CurrentImage?.Prompt == null) return;
            var p = Model.CurrentImage.Prompt;
            Clipboard.SetDataObject(p, true);
        }

        private void CopyNegative(object obj)
        {
            if (Model.CurrentImage?.NegativePrompt == null) return;
            var p = Model.CurrentImage.NegativePrompt;
            Clipboard.SetDataObject(p, true);
        }

        private void CopySeed(object obj)
        {
            if (Model.CurrentImage?.Seed == null) return;
            var p = Model.CurrentImage.Seed.ToString();
            Clipboard.SetDataObject(p, true);
        }

        private void CopyHash(object obj)
        {
            if (Model.CurrentImage?.ModelHash == null) return;
            var p = Model.CurrentImage.ModelHash;
            Clipboard.SetDataObject(p, true);
        }

        private void CopyParameters(object obj)
        {
            if (Model.CurrentImage?.Prompt == null) return;

            var p = Model.CurrentImage.Prompt;
            var n = Model.CurrentImage.NegativePrompt;
            var o = Model.CurrentImage.OtherParameters;
            var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

            Clipboard.SetDataObject(parameters, true);
        }

    }
}
