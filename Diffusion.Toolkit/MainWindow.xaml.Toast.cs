using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {

        private void Toast(string message, string caption, int timeout = 5)
        {
            ToastPopup.IsOpen = true;
            ToastMessage.Content = message;
            Task.Delay(timeout * 1000).ContinueWith((_) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ToastPopup.IsOpen = false;
                });
            });
        }

        public CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            var point = new Point(ActualWidth - popupSize.Width, ActualHeight - popupSize.Height - 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }

        private void CloseToast(object sender, MouseButtonEventArgs e)
        {
            ToastPopup.IsOpen = false;
        }
    }
}