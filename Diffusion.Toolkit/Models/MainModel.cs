using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diffusion.IO;

namespace Diffusion.Toolkit.Models;

public class MainModel : BaseNotify
{
    private Page _page;
    private ICommand _rescan;
    private ICommand _close;
    private ICommand _settings;

    public Page Page
    {
        get => _page;
        set => SetField(ref _page, value);
    }

    public ICommand Rescan
    {
        get => _rescan;
        set => SetField(ref _rescan, value);
    }

    public ICommand Settings
    {
        get => _settings;
        set => SetField(ref _settings, value);
    }

    public ICommand Close
    {
        get => _close;
        set => SetField(ref _close, value);
    }
}