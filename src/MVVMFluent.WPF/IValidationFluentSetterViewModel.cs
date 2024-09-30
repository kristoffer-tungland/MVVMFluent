namespace MVVMFluent.WPF
{
    public interface IValidationFluentSetterViewModel : IFluentSetterViewModel
    {
        event global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? ErrorsChanged;

        void CheckErrorsFor(string? propertyName);
    }

}
