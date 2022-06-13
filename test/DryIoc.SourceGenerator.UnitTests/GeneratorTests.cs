namespace DryIoc.SourceGenerator.UnitTests;

using System.Threading.Tasks;
using DryIoc;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;


[TestFixture]
// [UsesVerify]
public class GeneratorTests 
{
    [Test]
    public Task Test()
    {
        // The source code to test
        var source = @"
using Tests;

[Register(typeof(IA), typeof(A))]
public partial class Container// : ICompileTimeResolver
{
}

interface IA {}
class A {}";

        return GeneratorVerifier.Verify(source);
    }
}

[Register(typeof(IA), typeof(A))]
public partial class Container// : ICompileTimeResolver 
{
}

interface IA {}
class A {}
