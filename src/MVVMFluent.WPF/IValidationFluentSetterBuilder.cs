namespace MVVMFluent.WPF
{
    public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
    {
        bool HasErrors { get; }
        System.Collections.IEnumerable GetErrors();
    }
}
