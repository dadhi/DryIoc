using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue107_Resolve_still_caches_args_values_when_using_instance_factory_method
    {
        [Test]
        public void Resolve_shall_not_cache_args_values()
        {
            var c = new Container(rules => rules.WithDefaultReuse(Reuse.Singleton));

            c.Register<NameFactory>();
            c.Register<IName>(Made.Of(
                r => ServiceInfo.Of<NameFactory>(), 
                f => f.CreateName(Arg.Of<string>())),
                Reuse.Transient); // Here you need a Transient reuse so to have a transient IName services injected, instead of the same service 

            c.Register<IAlpha, Alpha>();
            c.Register<IBravo, Bravo>();

            var a = c.Resolve<IAlpha>(new object[] { "Alice" });
            Assert.AreEqual("Alice", a.Name.Value);

            var b = c.Resolve<IBravo>(new object[] { "Brenda" });
            Assert.AreEqual("Brenda", b.Name.Value);
        }

        private interface IName
        {
            string Value { get; }
        }

        private class NameFactory
        {
            private class NameImpl : IName
            {
                internal NameImpl(string name) => Value = name;

                public string Value { get; }
            }

            public IName CreateName(string name) => new NameImpl(name);
        }

        private interface IAlpha
        {
            IName Name { get; }
        }

        private interface IBravo
        {
            IName Name { get; }
        }

        private class Alpha : IAlpha
        {
            public Alpha(IName name) => Name = name;

            public IName Name { get; }
        }

        private class Bravo : IBravo
        {
            public Bravo(IName name) => Name = name;

            public IName Name { get; }
        }
    }
}
