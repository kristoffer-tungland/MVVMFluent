using System.ComponentModel;

namespace MVVMFluent.Interfaces;

public interface IValidationFluentSetterViewModel : IFluentSetterViewModel, INotifyDataErrorInfo
{
    void CheckErrorsFor(string? propertyName);
    void RaiseErrorsChanged(string? propertyName);
}
