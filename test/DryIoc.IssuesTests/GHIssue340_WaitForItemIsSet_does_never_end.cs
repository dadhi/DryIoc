using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue340_WaitForItemIsSet_does_never_end
    {
        [Test]
        public void WaitForItemIsSet_does_never_end()
        {
            var c = new Container();
            c.Register<Session>(Reuse.Singleton);
            c.Register<Instance>(Reuse.Scoped);
            c.Register<Func<string, Instance>>(Reuse.Singleton, Made.Of(() => Factory.Make(Arg.Of<Func<Session>>())));
            var session = c.Resolve<Session>();

            // The resolve sequence goes like this
            // Resolve<Session> // !!! Session
            //  -> _scope.New<Dependency>()
            //      -> Dependency(Func<string, Instance> instCtor)
            //          -> Func<string, Instance> Factory.Make(Func<Session> session) // !!! Session again
            //              -> instCtor("test")                                       // !!! session() called - so the RECURSION while getting the Session!
        }

        class Factory
        {
            public static Func<string, Instance> Make(Func<Session> session) => 
                str => {
                    return session().GetScopeForContext(str)?.Resolve<Instance>();
                };
        }

        class Session
        {
            IResolverContext _scope;
            public Session(IResolverContext cont)
            {
                _scope = cont.OpenScope("test");
                var dep = _scope.New<Dependency>();
            }

            // I use the string as placeholder now, it is for scope lookup
            public IResolverContext GetScopeForContext(string ctx) => _scope;
        }

        class Instance { }

        class Dependency
        {
            public Func<string, Instance> GetInstance;
            public Dependency(Func<string, Instance> instCtor)
            {
                GetInstance = instCtor; // This is legal and should work!
                // var inst = instCtor("test"); // It should not work anyway because of recursion explained above
            }
        }
    }
}
