using MVVMFluent.Interfaces;

namespace MVVMFluent.WPF.Interfaces
{
    public interface IValidationFluentSetterViewModel : IFluentSetterViewModel
    {
        event global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? ErrorsChanged;
    }

}
