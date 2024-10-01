namespace MVVMFluent
{
    public abstract class NotificationViewModelBase : global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        protected bool _disposed = false;

        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                PropertyChanged = null;
                DisposeInternal();
            }

            _disposed = true;
        }

        protected virtual void DisposeInternal() { }

        ~NotificationViewModelBase()
        {
            Dispose(false);
        }
    }
}