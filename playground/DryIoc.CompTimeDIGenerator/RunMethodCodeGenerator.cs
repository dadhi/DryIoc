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
using System.Diagnostics;
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
               })
               .Where(x => x.m != null);

        var references = context.CompilationProvider.Select(static (c, t) => c.References);

        context.RegisterImplementationSourceOutput(syntax.Combine(references), static (source, data) =>
        {
            var ((ns, m), references) = data;
            // var (ns, m) = data;
            var methodName = m.Identifier.ToString();
            var body = m.Body.NormalizeWhitespace().ToFullString();

            var usings = m.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(u => u.Name.ToString())
                .ToList();

            var allUsings = usings
                .Append("System")
                .Append("System.Linq")
                .Append("System.Collections.Generic")
                .Append("System.Threading")
                .Append(ns)
                .Distinct()
                .ToList();

            var options = ScriptOptions.Default
                .AddReferences(typeof(Action).Assembly)
                .AddReferences(references)
                .AddImports(allUsings);

            var result = CSharpScript.EvaluateAsync(body, options).GetAwaiter().GetResult();

            source.AddSource($"{ns}{methodName}.generated.cs", SourceText.From(_source, Encoding.UTF8));
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
        new(elems.ToArrayOrSelf());
}

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public readonly T[] Array;
    public EquatableArray(T[] array) => Array = array;

    public bool Equals(EquatableArray<T> array) => AsSpan().SequenceEqual(array.AsSpan());

    public override bool Equals(object obj) => obj is EquatableArray<T> array && Equals(this, array);

    public override int GetHashCode()
    {
        if (Array is not T[] array)
            return 0;

        int hashCode = default;
        foreach (var item in array)
            hashCode = Hasher.Combine(hashCode, item);

        return hashCode;
    }

    public ReadOnlySpan<T> AsSpan() => Array.AsSpan();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(Array ?? System.Array.Empty<T>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)(Array ?? System.Array.Empty<T>())).GetEnumerator();

    public int Count => Array?.Length ?? 0;

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}


