using DryIoc.ImTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
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
                   var name = m.Identifier.ToString();

                   return (ns, m);
               })
               .WithTrackingName("ExtractMethod")
               .Where(x => x.m != null)
               .WithTrackingName("RemoveNullMethods");

        var references = context.CompilationProvider.Select(static (c, t) =>
            c.GetUsedAssemblyReferences(t));

        context.RegisterImplementationSourceOutput(syntax.Combine(references), static async (source, data) =>
        // context.RegisterSourceOutput(syntax.Combine(references), static async (source, data) =>
        {
            var ((ns, m), references) = data;
            var methodName = m.Identifier.ToString();
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

            source.AddSource($"{ns}.{methodName}.generated.cs", SourceText.From(_source, Encoding.UTF8));
        });
    }

    private const string _source = """
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


public static class GeneratorTools
{
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> elems) where T : IEquatable<T> =>
        new EquatableArray<T>(elems.ToArrayOrSelf());
}

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[] _array;
    public EquatableArray(T[] array) => _array = array;

    public bool Equals(EquatableArray<T> array) => AsSpan().SequenceEqual(array.AsSpan());

    public override bool Equals(object obj) => obj is EquatableArray<T> array && Equals(this, array);

    public override int GetHashCode()
    {
        if (_array is not T[] array)
            return 0;

        int hashCode = default;
        foreach (var item in array)
            hashCode = Hasher.Combine(hashCode, item);

        return hashCode;
    }

    public ReadOnlySpan<T> AsSpan() => _array.AsSpan();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();

    public int Count => _array?.Length ?? 0;

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}


