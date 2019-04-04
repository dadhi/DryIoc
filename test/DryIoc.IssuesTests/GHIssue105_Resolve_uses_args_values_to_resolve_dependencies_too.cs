using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue105_Resolve_uses_args_values_to_resolve_dependencies_too
    {
        [Test][Ignore("fix me")]
        public void Resolve_shall_not_use_args_to_resolve_dependencies()
        {
            var c = new Container();
            c.Register<IInterface1, Implementation1>();
            c.Register<IInterface2, Implementation2>();

            Assert.Throws<ContainerException>(() =>
            {
                var id = Guid.NewGuid();
                c.Resolve(typeof(IInterface2), new object[] { id });
            });
        }

        private class Implementation1 : IInterface1
        {
            public Guid Id { get; }
            public Implementation1(Guid id)
            {
                Id = id;
            }
        }

        private class Implementation2 : IInterface2
        {
            public Guid Id { get; }
            public Implementation2(Guid id, IInterface1 interface1)
            {
                Id = id;
            }
        }

        private interface IInterface1 { }

        private interface IInterface2 { }
    }
}
