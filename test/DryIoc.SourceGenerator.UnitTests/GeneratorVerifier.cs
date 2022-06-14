using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyTests;
using VerifyNUnit;
using DryIoc.CompileTime;
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

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

public static readonly List<PortableExecutableReference> References = 
    AppDomain.CurrentDomain.GetAssemblies()
        .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
        .Select(_ => MetadataReference.CreateFromFile(_.Location))
        .Concat(new[]
        {
            MetadataReference.CreateFromFile(typeof(CompileTimeContainerGenerator).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICompileTimeContainer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
        })
        .ToList();

        public static Task Verify(string source)
        {
            // Parse the provided string into a C# syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            // Create a Roslyn compilation for the syntax tree.
            var sourceCompilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree },
                References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var sourceTreeCount = sourceCompilation.SyntaxTrees.Length;

            var generator = new CompileTimeContainerGenerator();
            var driver = CSharpGeneratorDriver
                .Create(generator)
                .RunGeneratorsAndUpdateCompilation(sourceCompilation, out var generatedCompilation, out var diagnostics);

            Assert.IsEmpty(diagnostics);

            var generatedTrees = generatedCompilation.SyntaxTrees.ToList();
            
            Assert.AreNotEqual(generatedTrees.Count, sourceTreeCount);
            var generatedOutput = generatedTrees[generatedTrees.Count - 1];

            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }
    }
}