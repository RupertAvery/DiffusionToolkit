using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;

namespace Diffusion.Toolkit.Models;

[Flags]
public enum PopupButtons
{
    OK = 1,
    Cancel = 2,
    OkCancel = OK | Cancel,
    Yes = 4,
    No = 8,
    YesNo = Yes | No
}


public enum PopupResult
{
    OK = 1,
    Cancel = 2,
    Yes = 3,
    No = 4,
}

public class MessagePopupModel : BaseNotify
{
    private ICommand _okCommand;
    private bool _isVisible;
    private string _message;
    private string _title;
    private ICommand _cancelCommand;
    private ICommand _yesCommand;
    private ICommand _noCommand;
    private bool _hasOk;
    private bool _hasCancel;
    private bool _hasYes;
    private bool _hasNo;

    private int _height;
    private int _width;
    private UIElement _placementTarget;


    public MessagePopupModel()
    {

    }

    public UIElement PlacementTarget
    {
        get => _placementTarget;
        set => SetField(ref _placementTarget, value);
    }


    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public ICommand OKCommand
    {
        get => _okCommand;
        set => SetField(ref _okCommand, value);
    }

    public ICommand CancelCommand
    {
        get => _cancelCommand;
        set => SetField(ref _cancelCommand, value);
    }

    public ICommand YesCommand
    {
        get => _yesCommand;
        set => SetField(ref _yesCommand, value);
    }

    public ICommand NoCommand
    {
        get => _noCommand;
        set => SetField(ref _noCommand, value);
    }

    public bool HasOk
    {
        get => _hasOk;
        set => SetField(ref _hasOk, value);
    }

    public bool HasCancel
    {
        get => _hasCancel;
        set => SetField(ref _hasCancel, value);
    }

    public bool HasYes
    {
        get => _hasYes;
        set => SetField(ref _hasYes, value);
    }

    public bool HasNo
    {
        get => _hasNo;
        set => SetField(ref _hasNo, value);
    }

    public int Width
    {
        get => _width;
        set => SetField(ref _width, value);
    }

    public int Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }
}