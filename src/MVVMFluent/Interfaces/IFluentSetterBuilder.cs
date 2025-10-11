namespace MVVMFluent;

public interface IFluentSetterBuilder
{
    bool IsBuilt { get; }

    void _intBuild();

    string GetPropertyName();
}
