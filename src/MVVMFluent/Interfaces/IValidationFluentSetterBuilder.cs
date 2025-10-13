using System.Collections;

namespace MVVMFluent.Interfaces;

public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
{
    bool HasErrors { get; }

    void CheckForErrors(object? value);

    IEnumerable GetErrors();
}