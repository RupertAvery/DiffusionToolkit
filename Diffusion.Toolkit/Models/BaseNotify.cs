using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Diffusion.Toolkit;

public class BaseNotify : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool PreventOnPropertyChanged { get; set; }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        if(!PreventOnPropertyChanged) OnPropertyChanged(propertyName);
        return true;
    }
}