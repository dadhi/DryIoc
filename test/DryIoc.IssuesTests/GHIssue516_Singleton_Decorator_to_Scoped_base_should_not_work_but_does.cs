using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue516_Singleton_Decorator_to_Scoped_base_should_not_work_but_does : ITest
    {
        public int Run()
        {
            Example();
            return 1;
        }

        [Test]
        public void Example()
        {
            var c = new Container();
            c.RegisterMany<A>(Reuse.Scoped);
            c.Register<IFace, ADecorator>(Reuse.Singleton, setup: Setup.Decorator);

            using (var s1 = c.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => s1.Resolve<IFace>());
                Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.ErrorName);
            }
        }

        public interface IFace { }

        public class A : IFace { }

        public class ADecorator : IFace
        {
            public IFace Inner { get; }
            public ADecorator(IFace inner)
            {
                Inner = inner;
            }
        }
    }
}