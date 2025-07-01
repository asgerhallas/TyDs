using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TyDs;

[Generator]
public class IdSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var values = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(static m => m is not null)
            .Select(static (x, _) => ((ISymbol DeclaredSymbol, string Prefix))x!);

        var combined = context.CompilationProvider.Combine(values.Collect());

        context.RegisterSourceOutput(combined,
            static (sourceProductionContext, source) =>
                Execute(sourceProductionContext, source.Right));
    }

    public bool Predicate(SyntaxNode syntaxNode, CancellationToken _)
    {
        if (syntaxNode is not RecordDeclarationSyntax recordDeclaration)
        {
            return false;
        }

        if (recordDeclaration.BaseList?.Types.FirstOrDefault() is not PrimaryConstructorBaseTypeSyntax ctorBase)
        {
            return false;
        }

        var identifier = ctorBase.Type switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            _ => null
        };

        return identifier is "Id";
    }

    public (ISymbol DeclaredSymbol, string Prefix)? Transform(GeneratorSyntaxContext context, CancellationToken _)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;
        var ctorBaseType = (PrimaryConstructorBaseTypeSyntax)recordDeclaration.BaseList!.Types.First();
        var baseCtorFirstArg = (LiteralExpressionSyntax)ctorBaseType.ArgumentList.Arguments.First().Expression;

        if (context.SemanticModel.GetDeclaredSymbol(recordDeclaration) is not ITypeSymbol recordDeclarationSymbol)
        {
            return null;
        }

        var baseTypeType = context.SemanticModel.GetTypeInfo(recordDeclaration.BaseList.Types[0].Type).Type!;

        if (baseTypeType.ToDisplayString() != "TyDs.Id")
        {
            return null;
        }

        return (recordDeclarationSymbol, baseCtorFirstArg.Token.ValueText);
    }

    static void Execute(SourceProductionContext context, ImmutableArray<(ISymbol DeclaredSymbol, string Prefix)> ids)
    {
        CheckForDuplicates(ids);

        var resourceName = $"{typeof(IdSourceGenerator).Namespace}.IdParser.cs";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);

        var source = reader.ReadToEnd()
            .Replace("%SwitchBranches%",
                string.Join("", ids.Select(GenerateSwitchBranch)));

        context.AddSource("IdParser.g.cs", source);
    }

    static string GenerateSwitchBranch((ISymbol DeclaredSymbol, string Prefix) x) => $"\"{x.Prefix.ToLowerInvariant()}\" => typeof({x.DeclaredSymbol.ToDisplayString()}),{Environment.NewLine}";

    static void CheckForDuplicates(ImmutableArray<(ISymbol DeclaredSymbol, string Prefix)> ids)
    {
        var duplicates = ids.Select(x => x.Prefix.ToLowerInvariant())
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException($"Duplicate Ids found in the source code: {string.Join(", ", duplicates)}");
        }
    }
}