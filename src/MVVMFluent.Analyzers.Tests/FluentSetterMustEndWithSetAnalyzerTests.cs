using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

namespace MVVMFluent.Analyzers.Tests;

public class FluentSetterMustEndWithSetAnalyzerTests
{
    [Fact]
    public async Task NoDiagnostic_WhenSetterDoesNotUseWhen()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    private string _name = string.Empty;
    
    public string Name
    {
        get => _name;
        set { _name = value; }
    }
}";

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoDiagnostic_WhenWhenEndsWithSet()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>() ?? string.Empty;
        set => When(value).Set();
    }
}";

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoDiagnostic_WhenWhenWithValidationEndsWithSet()
    {
        var test = @"
using MVVMFluent;

public class TestViewModel : ValidationViewModelBase
{
    public string Email
    {
        get => Get<string>() ?? string.Empty;
        set => When(value).HasValue(""Email is required"").Set();
    }
}";

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoDiagnostic_WhenWhenWithChainedMethodsEndsWithSet()
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
            .Changed(() => System.Console.WriteLine(""Changed""))
            .Set();
    }
}";

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Diagnostic_WhenWhenDoesNotEndWithSet()
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

        var expected = CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 9, 27)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Diagnostic_WhenWhenWithValidationDoesNotEndWithSet()
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

        var expected = CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 9, 57)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Diagnostic_WhenWhenWithChainedMethodsDoesNotEndWithSet()
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

        var expected = CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(9, 16, 11, 64)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Diagnostic_WhenWhenInBlockBodyDoesNotEndWithSet()
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

        var expected = CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>
            .Diagnostic(FluentSetterMustEndWithSetAnalyzer.DiagnosticId)
            .WithSpan(11, 13, 11, 78)
            .WithMessage("Property setter using 'When(value)' must end with a call to 'Set()' to commit the value");

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NoDiagnostic_WhenWhenInBlockBodyEndsWithSet()
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
            When(value).Changing(() => System.Console.WriteLine(""Changing"")).Set();
        }
    }
}";

        await CSharpAnalyzerVerifier<FluentSetterMustEndWithSetAnalyzer>.VerifyAnalyzerAsync(test);
    }
}
