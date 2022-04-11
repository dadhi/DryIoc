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
            c.Register<Blah>(Reuse.InWebRequest);

            var contextProxy = new HttpContextScopeContext(() => fakeContextItems);
            using (contextProxy.SetCurrent(current => Scope.Of(current, Reuse.WebRequestScopeName)))
            {
                var expected = c.Resolve<Blah>();
                Assert.AreSame(expected, c.Resolve<Blah>());

                using (contextProxy.SetCurrent(current => Scope.Of(current, null)))
                {
                    Assert.AreSame(expected, c.Resolve<Blah>());
                }
            }
        }

        public class Blah { }
    }
}
