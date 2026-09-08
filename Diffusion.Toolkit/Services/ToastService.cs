using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Toolkit.Configuration;

namespace Diffusion.Toolkit.Services;

public class ToastService
{
    private readonly Queue<Toast> _toastMessages = new Queue<Toast>();
    private readonly Popup _popup;
    private Dispatcher _dispatcher => ServiceLocator.Dispatcher;
    private Settings _settings => ServiceLocator.Settings;
    public ToastService(Popup popup)
    {
        _popup = popup;
    }

    public void DismissToast()
    {
        _dispatcher.Invoke(() => { _popup.IsOpen = false; });
        if (_toastMessages.Count > 0)
        {
            DisplayToast();
        }
    }

    private void DisplayToast()
    {
        if (_settings.ShowNotifications)
        {
            _dispatcher.Invoke(() =>
            {
                if (_toastMessages.Any())
                {
                    var toast = _toastMessages.Dequeue();
                    _popup.IsOpen = true;
                    ServiceLocator.MainModel.ToastMessage = toast.Message;

                    Task.Delay(toast.Timeout * 1000).ContinueWith((_) =>
                    {
                        DismissToast();
                    });
                }
            });
        }

    }

    public void Toast(string message, string caption, int timeout = 5)
    {
        if (!_settings.ShowNotifications)
        {
            return;
        }

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

        if (!_dispatcher.Invoke(() => _popup.IsOpen))
        {
            DisplayToast();
        }
    }

}