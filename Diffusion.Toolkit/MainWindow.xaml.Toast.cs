using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Diffusion.Toolkit
{
    public class Toast
    {
        public string Message { get; set; }
        public string Caption { get; set; }
        public int Timeout { get; set; }
    }

    public partial class MainWindow
    {
        private Queue<Toast> _toastMessages = new Queue<Toast>();
        private void DismissToast()
        {
            ToastPopup.IsOpen = false;
            if (_toastMessages.Count > 0)
            {
                DisplayToast();
            }
        }

        private void DisplayToast()
        {
            ToastPopup.IsOpen = true;
            var toast = _toastMessages.Dequeue();
            ToastMessage.Text = toast.Message;
            Task.Delay(toast.Timeout * 1000).ContinueWith((_) =>
            {
                Dispatcher.Invoke(() =>
                {
                    DismissToast();
                });
            });
        }

        private void Toast(string message, string caption, int timeout = 5)
        {
            if (timeout == 0)
            {
                timeout = 5;
            }

            _toastMessages.Enqueue(new Toast()
            {
                Message = message,
                Caption = caption,
                Timeout = timeout
            });

            if (!ToastPopup.IsOpen)
            {
                DisplayToast();
            }
        }

        public CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            var point = new Point(ActualWidth - popupSize.Width, ActualHeight - popupSize.Height - 50);
            return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
        }

        private void CloseToast(object sender, MouseButtonEventArgs e)
        {
            DismissToast();
        }
    }
}