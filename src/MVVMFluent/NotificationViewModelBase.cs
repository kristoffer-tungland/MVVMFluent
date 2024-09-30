namespace MVVMFluent
{
    public abstract class NotificationViewModelBase : System.ComponentModel.INotifyPropertyChanged, System.IDisposable
    {
        protected bool _disposed = false;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
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