using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

namespace MVVMFluent.Analyzers.Tests;

public class FluentSetterMustEndWithSetCodeFixTests
{
    [Fact]
    public async Task CodeFix_AddsSetToSimpleWhenCall()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set => When(value);
    }
}";

        var fixedCode = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set => When(value).Set();
    }
}";

        var expected = CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 9, 27)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSetToWhenWithValidation()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ValidationViewModelBase
{
    public string Email
    {
        get => Get<string>() ?? string.Empty;
        set => When(value).HasValue(""Email is required"");
    }
}";

        var fixedCode = @"
using MVVMFluent;

public class TestViewModel : ValidationViewModelBase
{
    public string Email
    {
        get => Get<string>() ?? string.Empty;
        set => When(value).HasValue(""Email is required"").Set();
    }
}";

        var expected = CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 9, 57)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSetToChainedMethods()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set => When(value)
            .Changing(() => System.Console.WriteLine(""Changing""))
            .Changed(() => System.Console.WriteLine(""Changed""));
    }
}";

        var fixedCode = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set => When(value)
            .Changing(() => System.Console.WriteLine(""Changing""))
            .Changed(() => System.Console.WriteLine(""Changed"")).Set();
    }
}";

        var expected = CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 11, 64)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSetInBlockBody()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set
        {
            When(value).Changing(() => System.Console.WriteLine(""Changing""));
        }
    }
}";

        var fixedCode = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set
        {
            When(value).Changing(() => System.Console.WriteLine(""Changing"")).Set();
        }
    }
}";

        var expected = CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(11, 13, 11, 78)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpCodeFixVerifier<FluentSetterMustEndWithSetAnalyzer, FluentSetterMustEndWithSetCodeFixProvider>
            .VerifyCodeFixAsync(test, expected, fixedCode);
    }
}
