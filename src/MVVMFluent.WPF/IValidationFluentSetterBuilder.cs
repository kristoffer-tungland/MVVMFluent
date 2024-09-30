namespace MVVMFluent.WPF
{
    public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
    {
        bool HasErrors { get; }

        void CheckForErrors(object? value);
        System.Collections.IEnumerable GetErrors();
    }
}
