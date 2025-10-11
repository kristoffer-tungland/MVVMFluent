namespace MVVMFluent
{
    public interface IValidationFluentSetterViewModel : IFluentSetterViewModel, global::System.ComponentModel.INotifyDataErrorInfo
    {
        void CheckErrorsFor(string? propertyName);
    }
}
