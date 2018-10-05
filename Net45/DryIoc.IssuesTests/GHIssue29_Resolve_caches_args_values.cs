using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue29_Resolve_caches_args_values
    {
        [Test]
        public void Resolve_shall_not_cache_args()
        {
            var c = new Container();
            c.Register<Interface, Implementation>();

            var id1 = Guid.NewGuid();
            c.Resolve(typeof(Interface), new object[] {id1});

            var id2 = Guid.NewGuid();
            var instance2 = (Implementation) c.Resolve(typeof(Interface), new object[] {id2});

            Assert.AreEqual(id2, instance2.Id, "'Object[] args' provided to Resolve where not injected");
        }

        private class Implementation : Interface
        {
            public Guid Id { get; }
            public Implementation(Guid id)
            {
                Id = id;
            }
        }

        private interface Interface { }
    }
}
