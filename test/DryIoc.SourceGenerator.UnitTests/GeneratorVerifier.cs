using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyTests;
using VerifyNUnit;

namespace DryIoc.SourceGenerator.UnitTests
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifySourceGenerators.Enable();
        }
    }

    public static class GeneratorVerifier
    {
        public static Task Verify(string source)
        {
            // Parse the provided string into a C# syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            // Create a Roslyn compilation for the syntax tree.
            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree });

            var generator = new CompileTimeContainerGenerator();

            var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

            // Use verify to snapshot test the source generator output!
            return Verifier.Verify(driver);
        }
    }
}