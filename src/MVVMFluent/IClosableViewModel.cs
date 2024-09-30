namespace MVVMFluent
{
    internal interface IClosableViewModel
    {
        /// <summary>
        /// Action invoked when the ViewModel requests to close the view.
        /// </summary>
        System.Action? RequestCloseView { get; set; }

        /// <summary>
        /// Determines if the ViewModel can close the view.
        /// </summary>
        bool CanCloseView();
    }
}
