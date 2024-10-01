namespace MVVMFluent.WPF
{
    public interface IValidationFluentSetter<TValue> : IFluentSetter<TValue>
    {
        global::System.Collections.IEnumerable GetErrors();
        bool HasErrors { get; }
    }
}
