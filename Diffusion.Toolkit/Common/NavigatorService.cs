using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Common;

public class NavigateException : Exception
{
    public string Url { get; }

    public NavigateException(string url) : base($"Invalid url: {url}")
    {
        Url = url;
    }

}

public class NavigateEventArgs
{
    public string? CurrentUrl { get; set; }
    public string? TargetUrl { get; set; }
    public Page? TargetPage { get; set; }
}

public class NavigatorService : INavigatorService
{
    public Window Host { get; }

    private Dictionary<string, Page> _pages;
    private readonly Stack<string> _history;
    private string? _currentUrl;

    public event EventHandler<NavigateEventArgs> OnNavigate;

    public NavigatorService(Window host)
    {
        Host = host;
        _history = new Stack<string>();
    }

    public void SetPages(Dictionary<string, Page> pages)
    {
        _pages = pages;
    }

    public void Goto(string url)
    {
        if (_currentUrl != null)
        {
            _history.Push(_currentUrl);
        }
        Navigate(url);
    }

    public void Back()
    {
        Navigate(_history.Pop());
    }

    private void Navigate(string url)
    {
        if (_pages.TryGetValue(url, out var page))
        {
            var args = new NavigateEventArgs()
            {
                CurrentUrl = _currentUrl,
                TargetUrl = url,
                TargetPage = page
            };
            OnNavigate?.Invoke(this, args);
            _currentUrl = url;
        }
        else
        {
            throw new NavigateException(url);
        }
    }
}