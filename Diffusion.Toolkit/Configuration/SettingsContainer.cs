using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Diffusion.Toolkit.Configuration;

public abstract class SettingsContainer : INotifyPropertyChanged
{
    private bool _isDirty;
    private readonly Dictionary<string, bool> _isPropertyDirty = new Dictionary<string, bool>();

    public bool IsPropertyDirty(string name)
    {
        return _isPropertyDirty.TryGetValue(name, out bool val) && val;
    }

    public void SetPristine()
    {
        _isDirty = false;
        _isPropertyDirty.Clear();
    }

    public void SetDirty()
    {
        _isDirty = true;
    }


    public bool IsDirty()
    {
        return _isDirty;
    }


    public event SettingChangedEventHander SettingChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool UpdateValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        var oldValue = field;

        field = value;

        _isPropertyDirty[propertyName] = true;

        _isDirty = true;

        SettingChanged?.Invoke(this, new SettingChangedEventArgs()
        {
            SettingName = propertyName,
            OldValue = oldValue,
            NewValue = value,
        });

        OnPropertyChanged(propertyName);

        return true;
    }

    protected bool UpdateList<T>(ref List<T>? field, List<T>? value, [CallerMemberName] string propertyName = "")
    {
        bool hasDiff = false;
        if (field != null && value != null)
        {
            var firstNotSecond = field.Except(value).ToList();
            var secondNotFirst = value.Except(field).ToList();

            hasDiff = firstNotSecond.Any() || secondNotFirst.Any();
        }
        else if (field == null && value != null)
        {
            hasDiff = true;
        }
        else
        {
            hasDiff = value != null || value!.Any();
        }

        if (!hasDiff) return false;

        field = value;
        _isPropertyDirty[propertyName] = true;
        _isDirty = true;

        OnPropertyChanged(propertyName);

        return true;
    }

}