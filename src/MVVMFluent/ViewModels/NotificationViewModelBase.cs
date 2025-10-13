using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Provides a base implementation for view models that raise property change notifications and manage disposable resources.
/// </summary>
public abstract class NotificationViewModelBase : INotifyPropertyChanged, IDisposable
{
    private bool _disposed;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Releases the resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the view model and optionally disposes of managed resources.
    /// </summary>
    /// <param name="disposing">A value indicating whether to dispose managed resources.</param>
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

    /// <summary>
    /// Performs additional cleanup when the view model is disposed.
    /// </summary>
    protected virtual void DisposeInternal()
    {
    }
}
