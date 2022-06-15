using System.Threading.Tasks;
using NUnit.Framework;
using DryIoc.CompileTime;

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
using DryIoc.CompileTime;
namespace DryIoc.SourceGenerator.UnitTests
{
    [Register(typeof(IA), typeof(A))]
    public partial class CompileTimeContainer
    {
    }

    interface IA {}
    class A {}
}";

            return GeneratorVerifier.Verify(source);
        }
    }

    [Register(typeof(IA), typeof(A))]
    public partial class CompileTimeContainer
    {
    }

    interface IA { }
    class A { }
}