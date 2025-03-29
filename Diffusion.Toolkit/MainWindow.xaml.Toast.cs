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
            var point = new Point(ActualWidth - popupSize.Width, ActualHeight - popupSize.Height - 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }
    }
}