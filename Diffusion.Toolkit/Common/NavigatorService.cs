using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Common;

public class NavigateException : Exception
{
    public Uri Uri { get; }

    public NavigateException(Uri uri) : base($"Invalid url: {uri.Url}")
    {
        Uri = uri;
    }

}

public class Uri
{
    public string Path { get; set; }
    public string? Fragment { get; set; }
    public string Url { get; set; }

    private static Regex uriRegex = new Regex("(?<path>([a-zA-Z0-9]+)(/([a-zA-Z0-9]+))*)(/\\#(?<fragment>[a-zA-Z0-9]+))?");

    public Uri(string uri)
    {
        Url = uri;

        var match = uriRegex.Match(uri);

        Path = match.Groups["path"].Value;
        if (match.Groups["fragment"].Success)
        {
            Fragment = match.Groups["fragment"].Value;
        }
    }
}


public class NavigateEventArgs
{
    public Uri CurrentUri { get; set; }
    public Uri TargetUri { get; set; }
    public Page? TargetPage { get; set; }
}

//public class NavigationRoute
//{
//    public string Url { get; set; }
//    public Page Page { get; set; }
//}

public class NavigatorService : INavigatorService
{
    public Window Host { get; }

    private Dictionary<string, Page> _pages = new Dictionary<string, Page>();
    private readonly Stack<Uri> _history;
    private Uri? _currentUrl = null;

    public event EventHandler<NavigateEventArgs> OnNavigate;

    public NavigatorService(Window host)
    {
        Host = host;
        _history = new Stack<Uri>();
    }

 
    public void RegisterRoute(string path, Page page)
    {
        _pages.Add(path, page);
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
        var uri = _history.Pop();
        Navigate(uri.Url);
    }


    private void Navigate(string url)
    {
        var uri = new Uri(url);


        if (_pages.TryGetValue(uri.Path, out var page))
        {
            var args = new NavigateEventArgs()
            {
                CurrentUri = _currentUrl,
                TargetUri = uri,
                TargetPage = page
            };
            OnNavigate?.Invoke(this, args);
            _currentUrl = uri;
        }
        else
        {
            throw new NavigateException(uri);
        }
    }
}