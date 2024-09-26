namespace MVVMFluent.WPF.Builders
{
    public interface IValidationFluentSetter<TValue> : IFluentSetter<TValue>
    {
        System.Collections.IEnumerable GetErrors();
        bool HasErrors { get; }
    }
}
