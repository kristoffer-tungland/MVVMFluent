namespace MVVMFluent.Interfaces;

public interface IFluentSetterBuilder
{
    bool IsBuilt { get; }

    void Build();

    string GetPropertyName();
}
