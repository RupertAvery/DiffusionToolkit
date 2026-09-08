using System.Windows;
using System.Windows.Input;

namespace Diffusion.Toolkit.Models;

public class MessagePopupModel : BaseNotify
{
    public MessagePopupModel()
    {

    }

    public UIElement PlacementTarget
    {
        get;
        set => SetField(ref field, value);
    }


    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public string Title
    {
        get;
        set => SetField(ref field, value);
    }

    public string Message
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand OKCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand CancelCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand YesCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommand NoCommand
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasOk
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasCancel
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasYes
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasNo
    {
        get;
        set => SetField(ref field, value);
    }

    public double Width
    {
        get;
        set => SetField(ref field, value);
    }

    public double Height
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Input
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowInput
    {
        get;
        set => SetField(ref field, value);
    }
}