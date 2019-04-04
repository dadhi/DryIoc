using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue107_Resolve_still_caches_args_values_when_using_instance_factory_method
    {
        [Test][Ignore("fix me")]
        public void Resolve_shall_not_cache_args_values()
        {
            var c = new Container(rules => rules.WithDefaultReuse(Reuse.Singleton));
            c.Register<NameFactory>();
            c.Register<IName>(made: Made.Of(r => ServiceInfo.Of<NameFactory>(), f => f.CreateName(Arg.Of<string>())));

            c.Register<IAlpha, Alpha>();
            c.Register<IBravo, Bravo>();

            c.Resolve<IAlpha>(args: new object[] { "Alice" });
            Assert.AreEqual("Brenda", c.Resolve<IBravo>(args: new object[] { "Brenda" }).Name.Name);
        }

        private interface IName
        {
            string Name { get; }
        }

        private class NameFactory
        {
            private class NameImpl : IName
            {
                internal NameImpl(string name)
                {
                    Name = name;
                }

                public string Name { get; }
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
            public Alpha(IName name)
            {
                Name = name;
            }

            public IName Name { get; }
        }

        private class Bravo : IBravo
        {
            public Bravo(IName name)
            {
                Name = name;
            }

            public IName Name { get; }
        }
    }
}
