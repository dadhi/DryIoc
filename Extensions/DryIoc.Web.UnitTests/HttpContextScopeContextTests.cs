using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.Web.UnitTests
{
    [TestFixture]
    public class HttpContextScopeContextTests
    {
        [Test]
        public void Can_open_scope_without_container_for_stateless_context()
        {
            var fakeContextItems = new Dictionary<object, object>();

            var c = new Container(scopeContext: new HttpContextScopeContext(() => fakeContextItems));
            c.Register<Blah>(ReuseInWeb.Request);

            var contextProxy = new HttpContextScopeContext(() => fakeContextItems);
            using (contextProxy.SetCurrent(current => new Scope(current, contextProxy.RootScopeName)))
            {
                var expected = c.Resolve<Blah>();
                Assert.AreSame(expected, c.Resolve<Blah>());

                using (contextProxy.SetCurrent(current => new Scope(current)))
                {
                    Assert.AreSame(expected, c.Resolve<Blah>());                   
                }
            }
        }

        public class Blah {}
    }
}
