using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Analyzers.Tests;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => new DiagnosticResult(diagnosticId, DiagnosticSeverity.Error);

    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync(CancellationToken.None);
    }

    private class Test : CSharpAnalyzerTest<TAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>
    {
        public Test()
        {
            // Add reference to MVVMFluent to get the ViewModelBase and interfaces
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
            TestState.AdditionalReferences.Add(typeof(ViewModelBase).Assembly);
        }
    }
}
