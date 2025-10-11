using System.Collections;

namespace MVVMFluent;

public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
{
    bool HasErrors { get; }

    void CheckForErrors(object? value);

    IEnumerable GetErrors();
}
