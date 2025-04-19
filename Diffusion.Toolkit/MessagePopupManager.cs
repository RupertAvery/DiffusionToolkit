using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit;

public class MessagePopupManager
{
    private readonly Window _parent;
    private readonly Panel _host;
    private readonly UIElement _placementTarget;
    private readonly Dispatcher _dispatcher;
    private readonly List<MessagePopup> _popups;

    public MessagePopupManager(Window parent, Panel host, UIElement placementTarget, Dispatcher dispatcher)
    {
        _parent = parent;
        _parent.Activated += ParentOnActivated;
        _parent.Deactivated += ParentOnDeactivated;
        _host = host;
        _placementTarget = placementTarget;
        _dispatcher = dispatcher;
        _popups = new List<MessagePopup>();
    }

    private void ParentOnDeactivated(object? sender, EventArgs e)
    {
        //throw new NotImplementedException();
        _dispatcher.Invoke(() =>
        {
            foreach (var popup in _popups)
            {
                try
                {
                    popup.Hide();
                }
                catch (Exception exception)
                {
                }
            }
        });
    }

    private Timer t;

    private void ParentOnActivated(object? sender, EventArgs e)
    {
        t = new Timer(Callback, null, 200, Timeout.Infinite);
    }

    private void Callback(object? state)
    {
        t.Dispose();

        _dispatcher.Invoke(() =>
        {
            foreach (var popup in _popups)
            {
                try
                {
                    popup.Show();
                }
                catch (Exception exception)
                {
                }
            }

        });
    }

    public Task<(PopupResult, string?)> ShowInput(string message, string title, string? defaultText = null, bool selectAll = true)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, 0, true);
            popup.Text = defaultText;
            _popups.Add(popup);
            _host.Children.Add(popup);

            return popup.Show(message, title, PopupButtons.OkCancel, PopupResult.Cancel, selectAll)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; });
                    return (t.Result, popup.Text);
                });
        });
    }


    public MessagePopupHandle ShowMessage(string message, string title, int timeout = 0)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, timeout);
            _popups.Add(popup);
            _host.Children.Add(popup);
            return popup.ShowMessage(message, title).ContinueWith(() => { _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; }); });
        });
    }

    public Task<PopupResult> Show(string message, string title, int timeout = 0)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, timeout);
            _popups.Add(popup);
            _host.Children.Add(popup);
            return popup.Show(message, title)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; });
                    return t.Result;
                });
        });
    }

    public Task<PopupResult> Show(string message, string title, PopupButtons buttons, int timeout = 0)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, timeout);
            _popups.Add(popup);
            _host.Children.Add(popup);
            return popup.Show(message, title, buttons, GetDefaultResult(buttons))
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; });
                    return t.Result;
                });
        });
    }

    public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons, int timeout = 0)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, timeout);
            _popups.Add(popup);
            _host.Children.Add(popup);
            return popup.ShowMedium(message, title, buttons, GetDefaultResult(buttons))
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; });
                    return t.Result;
                });
        });
    }

    public Task<PopupResult> ShowCustom(string message, string title, PopupButtons buttons, int width, int height, int timeout = 0)
    {
        return _dispatcher.Invoke(() =>
        {
            _host.Visibility = Visibility.Visible;
            var popup = new MessagePopup(this, _placementTarget, timeout);
            _popups.Add(popup);
            _host.Children.Add(popup);
            return popup.ShowCustom(message, title, buttons, GetDefaultResult(buttons), width, height)
                .ContinueWith(t =>
                {
                    _dispatcher.Invoke(() => { _host.Visibility = Visibility.Hidden; });
                    return t.Result;
                });
        });
    }

    private PopupResult GetDefaultResult(PopupButtons buttons)
    {
        var defaultResult = PopupResult.No;

        if (buttons.HasFlag(PopupButtons.OkCancel))
        {
            defaultResult = PopupResult.Cancel;
        }
        else if (buttons.HasFlag(PopupButtons.YesNo))
        {
            defaultResult = PopupResult.No;
        }
        else if (buttons.HasFlag(PopupButtons.OK))
        {
            defaultResult = PopupResult.OK;
        }

        return defaultResult;
    }

    public void Close(MessagePopup messagePopup)
    {
        _dispatcher.Invoke(() =>
        {
            _host.Children.Remove(messagePopup);
            _popups.Remove(messagePopup);
        });
    }

    public void CloseAll()
    {
        foreach (var popup in _popups.ToList())
        {
            Close(popup);
            popup.Close();
        }
    }

    public void Cancel()
    {
        _dispatcher.Invoke(() =>
        {
            var popup = _popups.LastOrDefault();
            if (popup != null)
            {
                popup.Cancel();
            }
        });
    }
}