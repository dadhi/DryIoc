namespace DryIoc.SourceGenerator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DryIoc.CompileTime;
using System.Linq;

/// <summary>Generates the compile time container based on the registrations provided by the registration attributes</summary>
[Generator]
public class CompileTimeContainerGenerator : IIncrementalGenerator
{
    /// <summary>Generates the empty container class for testing purposes.</summary>
    public static readonly string UsingsString = @"
    using System;
    using System.Collections.Generic;
    using DryIoc;
    using DryIoc.CompileTime;
    using DryIoc.ImTools;
    ";
    internal static readonly string ClassStartAndResolveShortString = @"
    public partial class CompileTimeContainer : ICompileTimeContainer
    {
        public void ResolveGenerated(ref object service, Type serviceType)
        {";
        internal static readonly string ResolveFullString = @"
        }

        public void ResolveGenerated(ref object service, Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {";
        internal static readonly string ResolveManyString = @"
        }

        public IEnumerable<ResolveManyResult> ResolveManyGenerated(Type serviceType)
        {";
        internal static readonly string ClassEndString = @"
            yield break;
        }
    }";

    internal static readonly string FullRegisterAttributeName = "DryIoc.CompileTime." + nameof(RegisterAttribute);

    /// <summary>Generates the implementation of the `DryIoc.ICompileTimeContainer`.</summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
        //     "CompileTimeContainer.generated.cs",
        //     SourceText.From(FileHeaderString + ClassStartAndResolveShortString + ResolveFullString + ResolveManyString + ClassEndString, Encoding.UTF8)));

        // find and collect compiler classes with Register attributes
        var classWithRegisterAttributesSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0,
                transform: static (context, _) =>
                {
                    var classNode = (ClassDeclarationSyntax)context.Node;
                    foreach (var attrList in classNode.AttributeLists)
                        foreach (var attrNode in attrList.Attributes)
                        {
                            var attrInfo = context.SemanticModel.GetSymbolInfo(attrNode);
                            var attrSymbol = attrInfo.Symbol ?? attrInfo.CandidateSymbols.FirstOrDefault();
                            if (attrSymbol is IMethodSymbol attrUsage &&
                                attrUsage.ContainingType.ToDisplayString() == FullRegisterAttributeName)
                               return context.SemanticModel.GetDeclaredSymbol(classNode);
                        }
                    return null;
                })
            .Where(static x => x is not null)
            .Collect();

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(context.CompilationProvider.Combine(classWithRegisterAttributesSymbols),
            static (context, collected) =>
            {
                var (compilation, classSymbols) = collected;
                if (classSymbols.IsDefaultOrEmpty)
                    return;

                var attrClass = compilation.GetTypeByMetadataName(FullRegisterAttributeName);

                foreach (var classDecl in classSymbols)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    foreach (var attrData in classDecl.GetAttributes())
                    {
                        if (!attrClass.Equals(attrData.AttributeClass, SymbolEqualityComparer.Default))
                            continue;
                    }

                    // var result = GenerateCompilerClass(classesToGenerate);

                    var source = new StringBuilder(1024)
                        .Append("namespace ").Append(classDecl.ContainingNamespace.ToString())
                        .AppendLine().Append('{')
                        .Append(UsingsString)
                        .Append(ClassStartAndResolveShortString)
                        .Append(ResolveFullString)
                        .Append(ResolveManyString)
                        .Append(ClassEndString)
                        .AppendLine().Append('}')
                        .ToString();

                    context.AddSource(classDecl.Name + ".generated.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
    }
}