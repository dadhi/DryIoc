namespace DryIoc.SourceGenerator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>Generates the compile time container based on the registrations provided by the registration attributes</summary>
[Generator]
public class CompileTimeContainerGenerator : IIncrementalGenerator
{
    /// <summary>Generates the empty container class for testing purposes.</summary>
    public static readonly string ResolveShortStart = @"
using DryIoc;

public partial class CompileTimeContainer : ICompileTimeContainer
{
    public void ResolveGenerated(ref object service, Type serviceType)
    {";
    public static readonly string ResolveFullStart = @"
    }

    public void ResolveGenerated(ref object service, Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
    {";
    public static readonly string ResolveManyStart = @"
    }

    public IEnumerable<Container.ResolveManyResult> ResolveManyGenerated(Type serviceType)
    {";
    public static readonly string ResolveManyEnd = @"
        yield break;
    }
}";

    /// <summary>Generates the implementation of the `DryIoc.ICompileTimeContainer`.</summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "CompileTimeContainer.generated.cs",
            SourceText.From(ResolveShortStart + ResolveFullStart + ResolveManyStart + ResolveManyEnd, Encoding.UTF8)));

        // todo: @wip the rest
    }
}