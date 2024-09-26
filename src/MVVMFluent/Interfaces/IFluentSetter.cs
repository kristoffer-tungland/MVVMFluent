namespace MVVMFluent
{
    public interface IFluentSetter<TValue>
    {
        string PropertyName { get; }

        void Set(TValue? valueToSet);
    }
}