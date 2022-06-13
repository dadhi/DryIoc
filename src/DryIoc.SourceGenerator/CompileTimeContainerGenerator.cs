namespace DryIoc.SourceGenerator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>Generates the compile time container based on the registrations provided by the registration attributes</summary>
[Generator]
public class CompileTimeContainerGenerator : IIncrementalGenerator
{
    /// <summary>Generates the empty container class for testing purposes.</summary>
    public static readonly string EmptyContainerSource = @"
using DryIoc;

public partial class CompileTimeContainer : ICompileTimeContainer
{
    public void ResolveGenerated(ref object service, Type serviceType)
    {
    }

    public void ResolveGenerated(ref object service, Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
    {
    }

    public IEnumerable<Container.ResolveManyResult> ResolveManyGenerated(Type serviceType)
    {
        yield break;
    }
}
";

    /// <summary>Generates the implementation of the `DryIoc.ICompileTimeContainer`.</summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "CompileTimeContainer.generated.cs",
            SourceText.From(EmptyContainerSource, Encoding.UTF8)));

        // todo: @wip the rest
    }
}