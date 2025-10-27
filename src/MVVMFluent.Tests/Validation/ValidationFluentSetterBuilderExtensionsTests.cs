using MVVMFluent.Interfaces;

namespace MVVMFluent.Tests.Validation;

public class ValidationFluentSetterBuilderExtensionsTests
{
    [Fact]
    public void IsEmail_AllowsEmptyValues()
    {
        var builder = CreateBuilder<string?>(string.Empty, nameof(IsEmail_AllowsEmptyValues));
        builder.IsEmail();

        CheckForErrors(builder, string.Empty);

        Assert.False(builder.HasErrors);
    }

    [Fact]
    public void IsEmail_AddsDefaultMessageForInvalidValue()
    {
        var builder = CreateBuilder<string?>(null, nameof(IsEmail_AddsDefaultMessageForInvalidValue));
        builder.IsEmail();

        CheckForErrors(builder, "not-an-email");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must be a valid email address.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsUrl_AddsDefaultMessageForInvalidValue()
    {
        var builder = CreateBuilder<string?>(null, nameof(IsUrl_AddsDefaultMessageForInvalidValue));
        builder.IsUrl();

        CheckForErrors(builder, "notaurl");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must be a valid URL.", EnumerateErrors(builder));
    }

    [Fact]
    public void HasLengthBetween_UsesCustomMessage()
    {
        var builder = CreateBuilder<string?>(null, nameof(HasLengthBetween_UsesCustomMessage));
        builder.HasLengthBetween(3, 5, "Value should be between three and five characters.");

        CheckForErrors(builder, "to");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value should be between three and five characters.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsAgeBetween_RejectsValueOutsideRange()
    {
        var builder = CreateBuilder<int?>(0, nameof(IsAgeBetween_RejectsValueOutsideRange));
        builder.IsAgeBetween(18, 65);

        CheckForErrors(builder, 70);

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must represent an age between 18 and 65.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsMinimumAge_UsesCustomMessage()
    {
        var builder = CreateBuilder<int?>(0, nameof(IsMinimumAge_UsesCustomMessage));
        builder.IsMinimumAge(21, "You must be 21 or older.");

        CheckForErrors(builder, 20);

        Assert.True(builder.HasErrors);
        Assert.Contains("You must be 21 or older.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsInRange_AllowsValueWithinBounds()
    {
        var builder = CreateBuilder<decimal?>(0m, nameof(IsInRange_AllowsValueWithinBounds));
        builder.IsInRange(1m, 5m);

        CheckForErrors(builder, 3m);

        Assert.False(builder.HasErrors);
    }

    [Fact]
    public void IsDateInPast_AddsDefaultMessage()
    {
        var builder = CreateBuilder<DateTime?>(DateTime.Today, nameof(IsDateInPast_AddsDefaultMessage));
        builder.IsDateInPast();

        CheckForErrors(builder, DateTime.Today.AddDays(1));

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must be a past date.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsDateBetween_ValidatesRangeInclusively()
    {
        var builder = CreateBuilder<DateTime?>(DateTime.Today, nameof(IsDateBetween_ValidatesRangeInclusively));
        var min = DateTime.Today.AddDays(-1);
        var max = DateTime.Today.AddDays(1);
        builder.IsDateBetween(min, max);

        CheckForErrors(builder, DateTime.Today.AddDays(2));

        Assert.True(builder.HasErrors);
        Assert.Contains($"Value must be between {min.Date:d} and {max.Date:d}.", EnumerateErrors(builder));
    }

    private static IValidationFluentSetter<TValue> CreateBuilder<TValue>(TValue value, string propertyName)
    {
        var viewModel = new TestValidationViewModel();
        return viewModel.CreateBuilder(value, propertyName);
    }

    private static void CheckForErrors<TValue>(IValidationFluentSetter<TValue> builder, object? value)
    {
        if (builder is IValidationFluentSetterBuilder validationBuilder)
        {
            validationBuilder.CheckForErrors(value);
        }
    }

    private static IReadOnlyCollection<string> EnumerateErrors<TValue>(IValidationFluentSetter<TValue> builder)
    {
        return builder.GetErrors().OfType<string>().ToArray();
    }

    private sealed class TestValidationViewModel : ValidationViewModelBase
    {
        public IValidationFluentSetter<TValue> CreateBuilder<TValue>(TValue value, string propertyName)
        {
            return When(value, propertyName);
        }
    }
}
