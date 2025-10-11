using System.Collections;

namespace MVVMFluent;

public interface IValidationFluentSetter<TValue> : IFluentSetter<TValue>
{
    IEnumerable GetErrors();

    bool HasErrors { get; }
}
