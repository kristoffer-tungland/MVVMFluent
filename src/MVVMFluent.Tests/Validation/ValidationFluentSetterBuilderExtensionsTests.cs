using System;
using System.Collections.Generic;
using System.Linq;
using MVVMFluent;

namespace MVVMFluent.Tests.Validation;

public class ValidationFluentSetterBuilderExtensionsTests
{
    [Fact]
    public void IsEmail_AllowsEmptyValues()
    {
        var builder = CreateBuilder<string?>(string.Empty, nameof(IsEmail_AllowsEmptyValues));
        builder.IsEmail();

        builder.CheckForErrors(string.Empty);

        Assert.False(builder.HasErrors);
    }

    [Fact]
    public void IsEmail_AddsDefaultMessageForInvalidValue()
    {
        var builder = CreateBuilder<string?>(null, nameof(IsEmail_AddsDefaultMessageForInvalidValue));
        builder.IsEmail();

        builder.CheckForErrors("not-an-email");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must be a valid email address.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsUrl_AddsDefaultMessageForInvalidValue()
    {
        var builder = CreateBuilder<string?>(null, nameof(IsUrl_AddsDefaultMessageForInvalidValue));
        builder.IsUrl();

        builder.CheckForErrors("notaurl");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must be a valid URL.", EnumerateErrors(builder));
    }

    [Fact]
    public void HasLengthBetween_UsesCustomMessage()
    {
        var builder = CreateBuilder<string?>(null, nameof(HasLengthBetween_UsesCustomMessage));
        builder.HasLengthBetween(3, 5, "Value should be between three and five characters.");

        builder.CheckForErrors("to");

        Assert.True(builder.HasErrors);
        Assert.Contains("Value should be between three and five characters.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsAgeBetween_RejectsValueOutsideRange()
    {
        var builder = CreateBuilder<int?>(0, nameof(IsAgeBetween_RejectsValueOutsideRange));
        builder.IsAgeBetween(18, 65);

        builder.CheckForErrors(70);

        Assert.True(builder.HasErrors);
        Assert.Contains("Value must represent an age between 18 and 65.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsMinimumAge_UsesCustomMessage()
    {
        var builder = CreateBuilder<int?>(0, nameof(IsMinimumAge_UsesCustomMessage));
        builder.IsMinimumAge(21, "You must be 21 or older.");

        builder.CheckForErrors(20);

        Assert.True(builder.HasErrors);
        Assert.Contains("You must be 21 or older.", EnumerateErrors(builder));
    }

    [Fact]
    public void IsInRange_AllowsValueWithinBounds()
    {
        var builder = CreateBuilder<decimal?>(0m, nameof(IsInRange_AllowsValueWithinBounds));
        builder.IsInRange(1m, 5m);

        builder.CheckForErrors(3m);

        Assert.False(builder.HasErrors);
    }

    [Fact]
    public void IsDateInPast_AddsDefaultMessage()
    {
        var builder = CreateBuilder<DateTime?>(DateTime.Today, nameof(IsDateInPast_AddsDefaultMessage));
        builder.IsDateInPast();

        builder.CheckForErrors(DateTime.Today.AddDays(1));

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

        builder.CheckForErrors(DateTime.Today.AddDays(2));

        Assert.True(builder.HasErrors);
        Assert.Contains($"Value must be between {min.Date:d} and {max.Date:d}.", EnumerateErrors(builder));
    }

    private static ValidationFluentSetterBuilder<TValue> CreateBuilder<TValue>(TValue value, string propertyName)
    {
        var viewModel = new TestValidationViewModel();
        return viewModel.CreateBuilder(value, propertyName);
    }

    private static IReadOnlyCollection<string> EnumerateErrors(IValidationFluentSetterBuilder builder)
    {
        return builder.GetErrors().OfType<string>().ToArray();
    }

    private sealed class TestValidationViewModel : ValidationViewModelBase
    {
        public ValidationFluentSetterBuilder<TValue> CreateBuilder<TValue>(TValue value, string propertyName)
        {
            return When(value, propertyName);
        }
    }
}
