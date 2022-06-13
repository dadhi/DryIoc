using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;
using DryIoc;

namespace DryIoc.SourceGenerator.UnitTests
{
    [TestFixture]
    public class GeneratorTests : ITest
    {
        public int Run()
        {
            Test().GetAwaiter().GetResult();
            return 1;
        }

        [Test]
        public Task Test()
        {
            var source = @"
using DryIoc;

[Register(typeof(IA), typeof(A))]
public partial class CompileTimeContainer
{
}

interface IA {}
class A {}";

            return GeneratorVerifier.Verify(source);
        }
    }

    [Register(typeof(IA), typeof(A))]
    public partial class Container
    {
    }

    interface IA { }
    class A { }
}