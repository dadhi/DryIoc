using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using DryIoc;
using DryIoc.CompileTimeDIGenerator;
using System.Linq;
using System.Diagnostics;

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

        var result = driver.RunGenerators(compilation).GetRunResult();
        var output = result.Results.Single();
        Debug.Assert(output.Exception is null);

        var steps = output
            .TrackedOutputSteps
            .SelectMany(x => x.Value) // step executions
            .SelectMany(x => x.Outputs) // execution results
            .ToList();

        steps.ForEach(x => Console.WriteLine(x.Reason));

        var newCompilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy"));
        var newResult = driver.RunGenerators(newCompilation).GetRunResult();
        var newOutput = newResult.Results.Single();
        Debug.Assert(newOutput.Exception is null);
        var newSteps = newOutput
            .TrackedOutputSteps
            .SelectMany(x => x.Value) // step executions
            .SelectMany(x => x.Outputs) // execution results
            .ToList();

        newSteps.ForEach(x => Console.WriteLine(x.Reason));

        // Assert.Collection(allOutputs, output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));

        // // Assert the driver use the cached result from AssemblyName and Syntax
        // var assemblyNameOutputs = result.TrackedSteps["AssemblyName"].Single().Outputs;
        // Assert.Collection(assemblyNameOutputs, output => Assert.Equal(IncrementalStepRunReason.Unchanged, output.Reason));

        // var syntaxOutputs = result.TrackedSteps["Syntax"].Single().Outputs;
        // Assert.Collection(syntaxOutputs, output => Assert.Equal(IncrementalStepRunReason.Unchanged, output.Reason));

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
            public class Program
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
