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
            var container = new Container();
            container.Register<Interface, Implementation>();

            var id1 = Guid.NewGuid();
            var x1 = container.Resolve<Interface>(new object[] { id1 });
            Assert.AreEqual(id1, x1.Id);

            var id2 = Guid.NewGuid();
            var x2 = container.Resolve<Interface>(new object[] { id2 });
            Assert.AreEqual(id2, x2.Id, "'Object[] args' provided to Resolve were not injected");
        }

        interface Interface
        {
            Guid Id { get; }
        }

        class Implementation : Interface
        {
            public Guid Id { get; }
            public Implementation(Guid id)
            {
                Id = id;
            }
        }
    }
}
