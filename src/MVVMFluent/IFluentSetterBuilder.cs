namespace MVVMFluent
{
    public interface IFluentSetterBuilder
    {
        bool IsBuilt { get; }
        void Build();
        string GetPropertyName();
    }
}