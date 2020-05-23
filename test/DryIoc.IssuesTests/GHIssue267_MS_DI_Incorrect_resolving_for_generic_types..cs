using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue267_MS_DI_Incorrect_resolving_for_generic_types
    {
        [Test]
        public void Test()
        {
            var container = new Container(Rules.MicrosoftDependencyInjectionRules);

            container.Register<IConfigureOptions<TestOptions>, TestOptionsSetup>();

            var isRegisteredForNonGeneric = container.IsRegistered(typeof(IConfigureOptions<TestOptions>));
            var isRegisteredForGeneric = container.IsRegistered(typeof(IConfigureOptions<TestOptions<TestService>>));
            var actual = container.ResolveMany(typeof(IConfigureOptions<TestOptions<TestService>>));

            Assert.IsTrue(isRegisteredForNonGeneric);
            Assert.IsFalse(isRegisteredForGeneric);
            Assert.AreEqual(0, actual.Count());
        }
        public interface IConfigureOptions<in TOptions> where TOptions : class
        {
            void Configure(TOptions options);
        }

        public class TestService
        {

        }
        public class TestOptions
        {
        }

        public class TestOptions<TService> : TestOptions where TService : class
        {
        }

        public class TestOptionsSetup : IConfigureOptions<TestOptions>
        {
            public void Configure(TestOptions options)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}