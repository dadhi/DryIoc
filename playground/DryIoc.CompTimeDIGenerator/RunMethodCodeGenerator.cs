using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace DryIoc.CompTimeDIGenerator;

[Generator]
public class RunMethodCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntax = context.SyntaxProvider
           .ForAttributeWithMetadataName(
               $"{nameof(DryIoc)}.{nameof(CompileTimeRegisterAttribute)}",
               predicate: static (_, _) => true,
               transform: static (ctx, _) =>
               {
                   var ns = ctx.TargetSymbol.ContainingNamespace.ToString();
                   var m = ctx.TargetNode as MethodDeclarationSyntax;
                   return (ns, m);
               });

        var references = context.CompilationProvider.Select(static (c, _) => c.References);

        context.RegisterSourceOutput(syntax.Combine(references), static async (source, data) =>
        {
            var ((ns, m), references) = data;
            var name = m.Identifier.ToString();
            var body = m.Body.NormalizeWhitespace().ToFullString();

            var usings = m.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(u => u.Name.ToString())
                // .Append("System")
                // .Append("System.Linq")
                // .Append("System.Collections.Generic")
                // .Append("System.Threading")
                .Append(ns)
                .Distinct()
                .ToList();

            var options = ScriptOptions.Default
                .AddReferences(typeof(Action).Assembly)
                .AddReferences(references)
                .AddImports(usings);

            var result = await CSharpScript.EvaluateAsync(body, options);

            source.AddSource($"CompileTimeDI.{"Foo"}.g.cs", SourceText.From(_sourceTemplate, Encoding.UTF8));
        });
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
