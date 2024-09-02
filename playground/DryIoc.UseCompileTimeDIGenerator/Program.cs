using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using DryIoc;
using System.Collections.Generic;
using DryIoc.CompTimeDIGenerator;
using System.Linq;

namespace FooBar;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, Source Generator!");

        var syntaxTree = CSharpSyntaxTree.ParseText(_source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Concat(new[] { MetadataReference.CreateFromFile(typeof(CompileTimeRegisterAttribute).Assembly.Location) })
            .Distinct()
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "USG",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


        var generator = new RunMethodCodeGenerator().AsSourceGenerator();

        var driverOptions = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        var driver = CSharpGeneratorDriver.Create(new[] { generator }, driverOptions: driverOptions);

        var compilationCopy = compilation.Clone();

        var result = driver.RunGenerators(compilation).GetRunResult();
        var steps = result.Results[0]
            .TrackedOutputSteps
            .SelectMany(x => x.Value) // step executions
            .SelectMany(x => x.Outputs) // execution results
            .ToList();

        var nextResult = driver.RunGenerators(compilationCopy).GetRunResult();
        var nextSteps = nextResult.Results[0]
            .TrackedOutputSteps
            .SelectMany(x => x.Value) // step executions
            .SelectMany(x => x.Outputs) // execution results
            .ToList();

        foreach (var r in result.Results)
        {
            foreach (var s in r.GeneratedSources)
            {
                Console.WriteLine(s.HintName);
                Console.WriteLine(s.SourceText);
            }
        }
    }

    static string _source = """
        using System;
        using DryIoc;

        namespace FooBar
        {
            public static class CompileTimeDI
            {
                [CompileTimeRegister]
                public static IContainer GetContainerWithRegistrations()
                {
                    var container = new Container();

                    container.Register<IService, MyService>();
                    container.Register<IDependencyA, DependencyA>();
                    container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

                    container.RegisterPlaceholder<RuntimeDependencyC>();

                    return container;
                }
            }
        }
        """;

    [CompileTimeRegister]
    public static IContainer GetContainerWithRegistrations()
    {
        var container = new Container();

        container.Register<IService, MyService>();
        container.Register<IDependencyA, DependencyA>();
        container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

        container.RegisterPlaceholder<RuntimeDependencyC>();

        return container;
    }

    public interface IService { }

    public class MyService : IService
    {
        public MyService(IDependencyA a, DependencyB<string> b, RuntimeDependencyC c) { }
    }

    public interface IDependencyA { }

    public class DependencyA : IDependencyA { }

    // let's make it struct for fun
    public struct DependencyB<T>
    {
        public readonly IDependencyA A;
        public DependencyB(IDependencyA a) => A = a;
    }

    public class RuntimeDependencyC
    {
    }
}