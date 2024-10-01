namespace MVVMFluent.WPF
{
    public interface IValidationFluentSetterViewModel : IFluentSetterViewModel, global::System.ComponentModel.INotifyDataErrorInfo
    {
        void CheckErrorsFor(string? propertyName);
    }

}
