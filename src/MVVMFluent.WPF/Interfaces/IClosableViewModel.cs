using System;

namespace MVVMFluent;

/// <summary>
/// Represents a view model that can request the closure of its associated view.
/// </summary>
public interface IClosableViewModel
{
    /// <summary>
    /// Gets or sets the callback that triggers the view to close.
    /// </summary>
    Action? RequestCloseView { get; set; }

    /// <summary>
    /// Determines whether the view can currently be closed.
    /// </summary>
    /// <returns><see langword="true"/> if the view may close; otherwise, <see langword="false"/>.</returns>
    bool CanCloseView();
}
