namespace MVVMFluent.Tests;

public class ViewModelBasePropertiesTests
{
    [Fact]
    internal void Set_SetsBackingField()
    {
        var viewModel = new PropertyTestViewModel
        {
            Property = "Test"
        };

        Assert.Equal("Test", viewModel.Property);
    }

    [Fact]
    internal void Set_RaisesPropertyChanged()
    {
        var viewModel = new PropertyTestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };
        viewModel.Property = "Test";
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    internal void Get_ReturnsDefaultValue()
    {
        var viewModel = new PropertyTestViewModel();
        var value = viewModel.Property;
        Assert.Null(value);
    }

    [Fact]
    internal void Get_PropertyWithDefaultValue_ReturnsDefaultValue()
    {
        var viewModel = new PropertyTestViewModel();
        var value = viewModel.PropertyWithDefault;
        Assert.Equal("defaultValue", value);
    }

    [Fact]
    internal void Get_PropertyWithOnChanged_RaisesOnChangedAction()
    {
        var viewModel = new PropertyTestViewModel();
        string? changedToValue = null;
        viewModel.OnChangedAction = newValue =>
        {
            changedToValue = newValue;
        };
        viewModel.PropertyWithOnChanged = "Test";
        Assert.Equal("Test", changedToValue);
    }

    [Fact]
    internal void Set_PropertyWithOnChanging_RaisesOnChangingAction()
    {
        var viewModel = new PropertyTestViewModel
        {
            PropertyWithOnChanging = "Exsisting"
        };

        string? changingToValue = null;
        string? exsistingValue = null;
        viewModel.OnChangingAction = newValue =>
        {
            changingToValue = newValue;
            exsistingValue = viewModel.PropertyWithOnChanging;
        };

        viewModel.PropertyWithOnChanging = "New";

        Assert.Equal("Exsisting", exsistingValue);
        Assert.Equal("New", changingToValue);
    }

    [Fact]
    internal void Set_FirstName_RaisesPropertyChangedForFullName()
    {
        var viewModel = new PropertyTestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(viewModel.FullName))
                propertyChangedRaised = true;
        };

        viewModel.FirstName = "John";
        Assert.True(propertyChangedRaised);
    }
}

internal class PropertyTestViewModel : ViewModelBase
{
    internal Action<string?>? OnChangedAction { get; set; }
    internal Action<string?>? OnChangingAction { get; set; }

    public string? Property
    {
        get => Get<string?>();
        set => Set(value);
    }

    public string? PropertyWithDefault
    {
        get => Get(defaultValue: "defaultValue");
        set => Set(value);
    }

    public string? PropertyWithOnChanged
    {
        get => Get<string?>();
        set => When(value)
            .OnChanged(newValue => OnChangedAction?.Invoke(newValue))
            .Set();
    }

    public string? PropertyWithOnChanging
    {
        get => Get<string?>();
        set => When(value)
            .OnChanging(newValue => OnChangingAction?.Invoke(newValue))
            .Set();
    }

    public string? FullName { get; set; }
    public string? FirstName
    {
        get => Get("First");
        set => When(value).Notify(nameof(FullName)).Set();
    }
}