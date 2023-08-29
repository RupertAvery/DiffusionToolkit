using System.Windows;

namespace Diffusion.Toolkit.Controls
{
    public partial class ThumbnailView
    {
        private void CopyPath(object obj)
        {
            if (Model.CurrentImage?.Path == null) return;
            var p = Model.CurrentImage.Path;
            Clipboard.SetText(p);
        }

        private void CopyPrompt(object obj)
        {
            if (Model.CurrentImage?.Prompt == null) return;
            var p = Model.CurrentImage.Prompt;
            Clipboard.SetText(p);
        }

        private void CopyNegative(object obj)
        {
            if (Model.CurrentImage?.NegativePrompt == null) return;
            var p = Model.CurrentImage.NegativePrompt;
            Clipboard.SetText(p);
        }

        private void CopySeed(object obj)
        {
            if (Model.CurrentImage?.Seed == null) return;
            var p = Model.CurrentImage.Seed.ToString();
            Clipboard.SetText(p);
        }

        private void CopyHash(object obj)
        {
            if (Model.CurrentImage?.ModelHash == null) return;
            var p = Model.CurrentImage.ModelHash;
            Clipboard.SetText(p);
        }

        private void CopyParameters(object obj)
        {
            if (Model.CurrentImage?.Prompt == null) return;

            var p = Model.CurrentImage.Prompt;
            var n = Model.CurrentImage.NegativePrompt;
            var o = Model.CurrentImage.OtherParameters;
            var parameters = $"{p}\r\n\r\nNegative prompt: {n}\r\n{o}";

            Clipboard.SetText(parameters);
        }

    }
}
