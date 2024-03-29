﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Diffusion.Toolkit.Classes;

public class NavigatorService : INavigatorService
{
    public Window Host { get; }

    private Dictionary<string, Page> _pages;
    private readonly Stack<string> _history;
    private string _currentUrl;
    public Action<Page> OnNavigate { get; set; }

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
        _currentUrl = url;
        Navigate();
    }

    public void Back()
    {
        _currentUrl = _history.Pop();
        Navigate();
    }

    private void Navigate()
    {
        OnNavigate?.Invoke(_pages[_currentUrl]);
    }
}