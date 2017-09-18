using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue251_AutoRegisterTypesFromDifferentNamespaceAndAssemblies
    {
        [Test]
        public void Test()
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
