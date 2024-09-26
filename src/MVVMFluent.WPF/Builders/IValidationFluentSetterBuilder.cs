using MVVMFluent.Builders;

namespace MVVMFluent.WPF.Builders
{
    public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
    {
        bool HasErrors { get; }
        global::System.Collections.IEnumerable GetErrors();
    }
}
