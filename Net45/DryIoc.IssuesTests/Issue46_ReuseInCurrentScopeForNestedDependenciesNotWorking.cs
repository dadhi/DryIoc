using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue46_ReuseInCurrentScopeForNestedDependenciesNotWorking
    {
        [Test]
        public void Main()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Main>(Reuse.InCurrentScope);
                container.Register<Class1>(Reuse.InCurrentScope);
                container.Register<Class2>(Reuse.InCurrentScope);
                container.Register<Class3>(Reuse.InCurrentScope);

                Main mainScoped;
                using (var scope = container.OpenScope())
                {
                    mainScoped = scope.Resolve<Main>();
                    Assert.That(mainScoped.C1.C, Is.SameAs(mainScoped.C2.C));
                }

                var main = container.Resolve<Main>();
                Assert.That(main, Is.Not.SameAs(mainScoped));
                Assert.That(main.C1, Is.Not.SameAs(mainScoped.C1));
                Assert.That(main.C1.C, Is.Not.SameAs(mainScoped.C1.C));
                Assert.That(main.C1.C, Is.SameAs(main.C2.C));
            }
        }
    }

    public class Main
    {
        public Class1 C1 { get; set; }
        public Class2 C2 { get; set; }

        public Main(Class1 c1, Class2 c2)
        {
            C1 = c1;
            C2 = c2;
        }
    }

    public class Class1
    {
        public Class3 C { get; set; }

        public Class1(Class3 c)
        {
            C = c;
        }
    }

    public class Class2
    {
        public Class3 C { get; set; }

        public Class2(Class3 c)
        {
            C = c;
        }
    }

    public class Class3
    {
    }
}
