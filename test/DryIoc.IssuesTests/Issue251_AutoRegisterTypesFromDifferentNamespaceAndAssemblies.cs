using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue251_AutoRegisterTypesFromDifferentNamespaceAndAssemblies : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(GetType().Assembly);

            var repo = container.Resolve<IRepository<string>>();

            Assert.IsInstanceOf<Repository<string>>(repo);
        }

        public interface IRepository<T> {}
        public class Repository<T> : IRepository<T> {}
    }
}
