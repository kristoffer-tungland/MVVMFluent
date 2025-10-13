using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Analyzers;

/// <summary>
/// Code fix provider that adds .Set() to the end of fluent setter chains.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentSetterMustEndWithSetCodeFixProvider)), Shared]
public class FluentSetterMustEndWithSetCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add .Set() to complete fluent setter";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(FluentSetterMustEndWithSetAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the node at the diagnostic location
        var node = root.FindToken(diagnosticSpan.Start).Parent;
        if (node == null)
        {
            return;
        }

        // Find the setter accessor or expression
        var setterNode = node.AncestorsAndSelf()
            .FirstOrDefault(n => n is AccessorDeclarationSyntax or ExpressionStatementSyntax);

        if (setterNode == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => AddSetCallAsync(context.Document, setterNode, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> AddSetCallAsync(Document document, SyntaxNode setterNode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        SyntaxNode newNode;

        if (setterNode is AccessorDeclarationSyntax accessor && accessor.ExpressionBody != null)
        {
            // Handle expression-bodied setter: set => When(value).Validate()
            var originalExpression = accessor.ExpressionBody.Expression;
            var newExpression = CreateSetCall(originalExpression);
            var newExpressionBody = accessor.ExpressionBody.WithExpression(newExpression);
            newNode = accessor.WithExpressionBody(newExpressionBody);
        }
        else if (setterNode is ExpressionStatementSyntax expressionStatement)
        {
            // Handle statement in block body
            var originalExpression = expressionStatement.Expression;
            var newExpression = CreateSetCall(originalExpression);
            newNode = expressionStatement.WithExpression(newExpression);
        }
        else
        {
            return document;
        }

        var newRoot = root.ReplaceNode(setterNode, newNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax CreateSetCall(ExpressionSyntax originalExpression)
    {
        // Create: originalExpression.Set()
        var setIdentifier = SyntaxFactory.IdentifierName("Set");
        var memberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            originalExpression,
            setIdentifier);
        
        var invocation = SyntaxFactory.InvocationExpression(memberAccess)
            .WithArgumentList(SyntaxFactory.ArgumentList());

        return invocation;
    }
}
