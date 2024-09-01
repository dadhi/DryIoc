using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace DryIoc.CompTimeDIGenerator;

public record GeneratorData(SemanticModel Model, SyntaxNode MethodSyntax);

[Generator]
public class RunMethodCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
           .ForAttributeWithMetadataName(
               $"{nameof(DryIoc)}.{nameof(CompileTimeRegisterAttribute)}",
               predicate: static (s, _) => true,
               transform: static (ctx, _) => new GeneratorData(ctx.SemanticModel, ctx.TargetNode))
           .Where(static m => m is not null);

        context.RegisterSourceOutput(provider, Generate);
    }

    private static void Generate(SourceProductionContext context, GeneratorData source)
    {
        var methodSymbol = source.Model.GetDeclaredSymbol(source.MethodSyntax) as IMethodSymbol;
        if (methodSymbol is null) return;

        // var options = ScriptOptions.Default
        //     .AddReferences(typeof(Func<>).Assembly)
        //     .AddReferences(source.Model.References)
        //     .AddImports(imports);

        // var result = await CSharpScript.EvaluateAsync(code, options);

        context.AddSource($"CompileTimeDI.{"Foo"}.g.cs", SourceText.From(_sourceTemplate, Encoding.UTF8));
    }

    private const string _sourceTemplate = """
    using System;
    namespace CompileTimeDI
    {
        public static class Foo {
            public static void Run() {
                Console.WriteLine(""Hello from generated code!"");
            }
        }
    """;
}
