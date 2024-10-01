namespace MVVMFluent.WPF
{
    public static class WindowExtensions
    {
        /// <summary>
        /// Attaches a view model to a window.
        /// </summary>
        /// <param name="window">The window to attach the view model to.</param>
        /// <param name="closableViewModel">The view model that implements <see cref="IClosableViewModel"/>.</param>
        public static void AttachToWindow(this global::System.Windows.Window window, IClosableViewModel closableViewModel)
        {
            window.DataContext = closableViewModel;

            closableViewModel.RequestCloseView = () => window.DialogResult = true;

            window.Closing += CanClose;
            void CanClose(object? sender, global::System.ComponentModel.CancelEventArgs e)
            {
                if (!closableViewModel.CanCloseView())
                    e.Cancel = true;
            }
            window.Closed += OnClosed;
            void OnClosed(object? sender, global::System.EventArgs e)
            {
                window.Closing -= CanClose;
                window.Closed -= OnClosed;
                closableViewModel.RequestCloseView = null;
            }
        }

        /// <summary>
        /// Shows a dialog window and returns the result of the view model.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="window">The window to show.</param>
        /// <param name="resultViewModel">The view model that implements <see cref="IResultViewModel{TResult}"/>.</param>
        /// <returns></returns>
        public static TResult? ShowDialog<TResult>(this global::System.Windows.Window window, IResultViewModel<TResult> resultViewModel)
        {
            try
            {
                window.DataContext = resultViewModel;

                if (resultViewModel is IClosableViewModel closableViewModel)
                    window.AttachToWindow(closableViewModel);

                if (window.ShowDialog() == true)
                {
                    return resultViewModel.GetResult();
                }
                return default;
            }
            finally
            {
                if (window.DataContext is global::System.IDisposable disposable)
                {
                    disposable.Dispose();
                }

                window.DataContext = null;
            }
        }

        /// <summary>
        /// Gets the result of the view model.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="window">Window to get the result from.</param>
        /// <param name="resultViewModel">The view model that implements <see cref="IResultViewModel{TResult}"/>.</param>
        /// <returns></returns>
        public static TResult? GetResult<TResult>(this global::System.Windows.Window window, IResultViewModel<TResult> resultViewModel)
        {
            if (window.DialogResult == true)
            {
                var result = resultViewModel.GetResult();
            }

            return default;
        }
    }
}
