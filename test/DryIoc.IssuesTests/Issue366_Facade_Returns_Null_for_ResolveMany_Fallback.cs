using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue366_Facade_Returns_Null_for_ResolveMany_Fallback
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.RegisterMany<Class1>();
            var facade = container.CreateFacade();

            facade.RegisterMany<Class2>();

            var ss = facade.ResolveMany<IService1>(behavior: ResolveManyBehavior.AsFixedArray);
            Assert.AreEqual(2, ss.Count());
        }

        public interface IService1 {}
        public class Class1 : IService1 {}
        public class Class2 : IService1 {}
    }
}
