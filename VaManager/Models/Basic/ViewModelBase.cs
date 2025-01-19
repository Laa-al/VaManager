using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VaManager.Models.Basic;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public abstract class ViewModelBase<TSelf> : ViewModelBase
    where TSelf : ViewModelBase<TSelf>, new()
{
    public static TSelf Instance { get; set; } = new();
}