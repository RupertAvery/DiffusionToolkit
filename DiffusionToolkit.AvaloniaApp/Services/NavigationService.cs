using System;
using System.Collections.Generic;

namespace DiffusionToolkit.AvaloniaApp.Services;

public class NavigationService
{
    private readonly Dictionary<string, INavigationTarget> _navigation;

    public NavigationService()
    {
        _navigation = new Dictionary<string, INavigationTarget>();
    }


    public void AddNavigationTarget(string name, INavigationTarget target)
    {
        _navigation.Add(name, target);
    }

    public void Goto(string name)
    {
        Navigate?.Invoke(this, _navigation[name]);
    }

    public event EventHandler<INavigationTarget> Navigate;
}