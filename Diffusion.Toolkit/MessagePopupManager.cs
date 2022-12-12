using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

    public Task<PopupResult> Show(string message, string title)
    {
        _host.Visibility = Visibility.Visible;
        var popup = new MessagePopup(this, _placementTarget);
        _popups.Add(popup);
        _host.Children.Add(popup);
        return popup.Show(message, title)
            .ContinueWith(t =>
            {
                _dispatcher.Invoke(() =>
                {
                    _host.Visibility = Visibility.Hidden;
                });
                return t.Result;
            });
    }

    public Task<PopupResult> Show(string message, string title, PopupButtons buttons)
    {
        _host.Visibility = Visibility.Visible;
        var popup = new MessagePopup(this, _placementTarget);
        _popups.Add(popup);
        _host.Children.Add(popup);
        return popup.Show(message, title, buttons)
            .ContinueWith(t =>
            {
                _dispatcher.Invoke(() =>
                {
                    _host.Visibility = Visibility.Hidden;
                });
                return t.Result;
            });
    }

    public Task<PopupResult> ShowMedium(string message, string title, PopupButtons buttons)
    {
        _host.Visibility = Visibility.Visible;
        var popup = new MessagePopup(this, _placementTarget);
        _popups.Add(popup);
        _host.Children.Add(popup);
        return popup.ShowMedium(message, title, buttons)
            .ContinueWith(t =>
            {
                _dispatcher.Invoke(() =>
                {
                    _host.Visibility = Visibility.Hidden;
                });
                return t.Result;
            });
    }

    public void Close(MessagePopup messagePopup)
    {
        _dispatcher.Invoke(() =>
        {
            _host.Children.Remove(messagePopup);
        });
        _popups.Remove(messagePopup);
    }
}