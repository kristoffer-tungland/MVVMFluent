using System;

namespace MVVMFluent;

public interface IClosableViewModel
{
    Action? RequestCloseView { get; set; }

    bool CanCloseView();
}
