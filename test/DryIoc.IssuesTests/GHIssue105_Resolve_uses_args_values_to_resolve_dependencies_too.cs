using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue105_Resolve_uses_args_values_to_resolve_dependencies_too
    {
        [Test]
        public void Resolve_shall_not_use_args_to_resolve_dependencies()
        {
            var c = new Container();
            c.Register<IInterface1, Implementation1>();
            c.Register<IInterface2, Implementation2>();

            var id = Guid.NewGuid();
            var i2 = c.Resolve<IInterface2>(new object[] { id });

            Assert.AreEqual(i2.Id, i2.Interface1.Id);
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
            public IInterface1 Interface1 { get; }

            public Implementation2(Guid id, IInterface1 interface1)
            {
                Id = id;
                Interface1 = interface1;
            }
        }

        private interface IInterface1 {
            Guid Id { get; }
        }

        private interface IInterface2 {
            Guid Id { get; }
            IInterface1 Interface1 { get; }
        }
    }
}
