namespace DryIoc.SourceGenerator;

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DryIoc.CompileTime;

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

                // todo: @wip read the configureRules from the specific attribute
                IConfigureRules configureRules = null;
                var rules = (configureRules == null ? Rules.Default : configureRules.Configure(Rules.Default));
                var container = new Container(rules.ForExpressionGeneration());

                var attrClass = compilation.GetTypeByMetadataName(FullRegisterAttributeName);

                foreach (var classDecl in classSymbols)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    foreach (var attrData in classDecl.GetAttributes())
                    {
                        if (!attrClass.Equals(attrData.AttributeClass, SymbolEqualityComparer.Default))
                            continue;

                        Type serviceType = null, implementationType = null;
                        if (!attrData.ConstructorArguments.IsEmpty)
                        {
                            var args = attrData.ConstructorArguments;
                            for (var i = 0; i < args.Length; i++)
                            {
                                var argValue = args[i];
                                if (argValue.Kind == TypedConstantKind.Error)
                                {
                                    Debug.WriteLine("Error in attribute: " + attrData.ToString());
                                    return;
                                }
                                if (i == 0)
                                    serviceType = (Type)argValue.Value;
                                else if (i == 1)
                                    implementationType = (Type)argValue.Value;
                                else 
                                {
                                    // todo: @wip
                                }
                            }
                        }
                        if (!attrData.NamedArguments.IsEmpty)
                        {
                            foreach (var arg in attrData.NamedArguments)
                            {
                                var argValue = arg.Value;
                                if (argValue.Kind == TypedConstantKind.Error)
                                {
                                    Debug.WriteLine("Error in attribute: " + attrData.ToString());
                                    return;
                                }
                                if (arg.Key == nameof(RegisterAttribute.ServiceType))
                                    serviceType = (Type)argValue.Value;
                                else if (arg.Key == nameof(RegisterAttribute.ImplementationType))
                                    implementationType = (Type)argValue.Value;
                                else 
                                {
                                    // todo: @wip
                                }
                            }
                        }

                        container.Register(serviceType, implementationType);

                        // todo: @wip provide the rest of the arguments
                        // public static void Register(this IRegistrator registrator, Type serviceType, Type implementationType,
                        //     IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
                        //     object serviceKey = null)
                    }

                    // todo: @wip
                    // var result = container.GenerateResolutionExpressions(x => x.SelectMany(r =>
                    //     SpecifyResolutionRoots(r).EmptyIfNull()).Concat(
                    //     CustomResolutionRoots.EmptyIfNull()));

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