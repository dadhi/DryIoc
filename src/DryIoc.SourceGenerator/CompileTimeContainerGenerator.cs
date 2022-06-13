namespace DryIoc.SourceGenerator;

using Microsoft.CodeAnalysis;

/// <summary>Generates the compile time container based on the registrations found by the registration attributes</summary>
[Generator]
public class CompileTimeContainerGenerator : IIncrementalGenerator
{
    /// <summary>Generates the implementation of the `DryIoc.ICompileTimeContainerAttribute`.</summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        // context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
        //     "EnumExtensionsAttribute.g.cs", 
        //     SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        // TODO: implement the remainder of the source generator
    }
}