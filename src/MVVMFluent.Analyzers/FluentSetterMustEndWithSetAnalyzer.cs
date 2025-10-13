using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MVVMFluent.Analyzers;

/// <summary>
/// Analyzer that ensures property setters using When(value) always end with a call to Set().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FluentSetterMustEndWithSetAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MVVMFLUENT001";
    private const string Category = "Usage";

    private static readonly LocalizableString Title = 
        "Fluent setter must end with Set()";
    
    private static readonly LocalizableString MessageFormat = 
        "Property setter using 'When(value)' must end with a call to 'Set()' to commit the value";
    
    private static readonly LocalizableString Description = 
        "When using the fluent setter pattern with When(value), you must call Set() at the end of the chain to commit the value to the backing field. " +
        "Without Set(), the value will not be properly stored and property change notifications will not be triggered.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzePropertySetter, SyntaxKind.SetAccessorDeclaration);
    }

    private static void AnalyzePropertySetter(SyntaxNodeAnalysisContext context)
    {
        var setAccessor = (AccessorDeclarationSyntax)context.Node;
        
        // Check if the setter has an expression body (e.g., set => When(value)...)
        if (setAccessor.ExpressionBody != null)
        {
            AnalyzeExpression(context, setAccessor.ExpressionBody.Expression, setAccessor.ExpressionBody.Expression.GetLocation());
        }
        // Check if the setter has a block body
        else if (setAccessor.Body != null)
        {
            foreach (var statement in setAccessor.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionStatement)
                {
                    AnalyzeExpression(context, expressionStatement.Expression, statement.GetLocation());
                }
            }
        }
    }

    private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, Location location)
    {
        // Look for invocation expressions that might be part of a fluent chain
        if (expression is InvocationExpressionSyntax invocation)
        {
            // Check if this is the root of a method chain starting with When(value)
            if (IsFluentChainStartingWithWhen(invocation, context.SemanticModel, out var chainRoot))
            {
                // Check if the chain ends with Set()
                if (!DoesChainEndWithSet(chainRoot))
                {
                    var diagnostic = Diagnostic.Create(Rule, location);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsFluentChainStartingWithWhen(InvocationExpressionSyntax invocation, SemanticModel semanticModel, out InvocationExpressionSyntax chainRoot)
    {
        chainRoot = invocation;
        
        // Walk up the chain to find the root invocation
        var current = invocation;
        while (current.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
            {
                current = parentInvocation;
            }
            else
            {
                break;
            }
        }
        
        // Check if the root method is named "When"
        if (current.Expression is MemberAccessExpressionSyntax rootMemberAccess)
        {
            return rootMemberAccess.Name.Identifier.Text == "When";
        }
        else if (current.Expression is IdentifierNameSyntax identifierName)
        {
            return identifierName.Identifier.Text == "When";
        }

        // Check if this is a direct call to When
        var symbolInfo = semanticModel.GetSymbolInfo(current);
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Name == "When")
            {
                // Verify it returns IFluentSetter<T> or IValidationFluentSetter<T>
                var returnType = methodSymbol.ReturnType;
                if (returnType is INamedTypeSymbol namedType)
                {
                    var typeName = namedType.Name;
                    if (typeName == "IFluentSetter" || typeName == "IValidationFluentSetter")
                    {
                        chainRoot = current;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool DoesChainEndWithSet(InvocationExpressionSyntax chainRoot)
    {
        // Find the last method call in the chain
        var current = chainRoot;
        InvocationExpressionSyntax? lastInvocation = null;

        // Traverse to find the outermost invocation (the end of the chain)
        var node = chainRoot.Parent;
        while (node != null)
        {
            if (node is MemberAccessExpressionSyntax memberAccess && 
                memberAccess.Expression == (lastInvocation ?? (ExpressionSyntax)current))
            {
                if (memberAccess.Parent is InvocationExpressionSyntax parentInvocation)
                {
                    lastInvocation = parentInvocation;
                    node = parentInvocation.Parent;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // If we found invocations in the chain, check the last one
        var finalInvocation = lastInvocation ?? current;
        
        if (finalInvocation.Expression is MemberAccessExpressionSyntax finalMemberAccess)
        {
            return finalMemberAccess.Name.Identifier.Text == "Set";
        }

        return false;
    }
}
