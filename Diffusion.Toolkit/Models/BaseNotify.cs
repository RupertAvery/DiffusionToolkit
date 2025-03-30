using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Diffusion.Toolkit;

public class BaseNotify : INotifyPropertyChanged
{
    protected bool _isDirty;

    public virtual bool IsDirty
    {
        get => _isDirty;
    }

    public void SetPristine()
    {
        _isDirty = false;
        OnPropertyChanged(nameof(IsDirty));
    }

    public void SetDirty()
    {
        _isDirty = true;
        OnPropertyChanged(nameof(IsDirty));
    }

    protected void RegisterObservableChanges<T>(ObservableCollection<T>? observableCollection, Action? alternateDirtyAction = null)
    {
        if (observableCollection != null)
        {
            observableCollection.CollectionChanged += (sender, args) =>
            {
                if (alternateDirtyAction != null)
                {
                    alternateDirtyAction(); }
                else
                {
                    SetDirty();
                }

                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems)
                    {
                        if (item is BaseNotify notify)
                        {
                            notify.PropertyChanged += (o, eventArgs) =>
                            {
                                if (alternateDirtyAction != null)
                                {
                                    alternateDirtyAction();
                                }
                                else
                                {
                                    SetDirty();
                                }
                            };
                        }
                    }
                }
            
            };

            foreach (var item in observableCollection)
            {
                if (item is BaseNotify notify)
                {
                    notify.PropertyChanged += (o, eventArgs) =>
                    {
                        if (alternateDirtyAction != null)
                        {
                            alternateDirtyAction();
                        }
                        else
                        {
                            SetDirty();
                        }
                    };
                }
            }
        }
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool PreventOnPropertyChanged { get; set; }

    protected bool SetField<T>(ref T field, T value, bool setDirty = true, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        if(!PreventOnPropertyChanged) OnPropertyChanged(propertyName);
        if (setDirty)
        {
            SetDirty();
        }
        return true;
    }
}