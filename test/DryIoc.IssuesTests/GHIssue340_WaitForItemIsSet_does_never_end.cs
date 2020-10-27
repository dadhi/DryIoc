using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue340_WaitForItemIsSet_does_never_end
    {
        [Test]
        [Ignore("fixme")]
        public void WaitForItemIsSet_does_never_end()
        {
            var c = new Container();
            c.Register<Session>(Reuse.Singleton);
            c.Register<Instance>(Reuse.Scoped);
            c.Register<Func<string, Instance>>(Reuse.Singleton, Made.Of(() => Factory.Make(Arg.Of<Func<Session>>())));
            var session = c.Resolve<Session>();
        }

        class Factory
        {
            public static Func<string, Instance> Make(Func<Session> session) => str => session().GetScopeForContext(str)?.Resolve<Instance>();
        }

        class Session
        {
            IResolverContext _scope;
            public Session(IContainer cont)
            {
                _scope = cont.OpenScope("test");
                var dep = cont.New<Dependency>();
            }

            // I use the string as placeholder now, it is for scope lookup
            public IResolverContext GetScopeForContext(string ctx) => _scope;
        }

        class Instance { }

        class Dependency
        {
            public Dependency(Func<string, Instance> instCtor)
            {
                var inst = instCtor("test");
            }
        }
    }
}
