using MVVMFluent.Builders;

namespace MVVMFluent.Interfaces
{
    public interface IFluentSetterViewModel
    {
        void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder);
        IFluentSetterBuilder? GetFluentSetterBuilder(string propertyName);

        T? GetFieldValue<T>(string propertyName);
        void SetFieldValue<T>(string propertyName, T? value);

        void OnPropertyChanged(string? propertyName);
    }
}