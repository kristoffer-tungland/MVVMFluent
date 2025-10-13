using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

public abstract class NotificationViewModelBase : INotifyPropertyChanged, IDisposable
{
    private bool _disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            PropertyChanged = null;
            DisposeInternal();
        }

        _disposed = true;
    }

    protected virtual void DisposeInternal()
    {
    }
}
