using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Diffusion.Toolkit.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void CloseToast(object sender, MouseButtonEventArgs e)
        {
            ServiceLocator.ToastService.DismissToast();
        }

        public CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiX = 96, dpiY = 96;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var width = ActualWidth * dpiX / 96.0;
            var height = ActualHeight * dpiY / 96.0;

            var point = new Point(width - popupSize.Width, height - popupSize.Height - 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }
    }
}