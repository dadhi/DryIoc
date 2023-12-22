using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue608_Multiple_same_type_same_keyed : ITest
    {
        public int Run()
        {
            Multiple_exports_with_the_same_key();
            return 1;
        }

        [Test]
        public void Multiple_exports_with_the_same_key()
        {
           var c = new Container().WithMef();

            c.RegisterExports(new[] { typeof(Consumer), typeof(A), typeof(B), typeof(C) });

            var consumer = c.Resolve<Consumer>();
            Assert.IsNotNull(consumer);
            Assert.AreEqual(2, consumer.Keyed.Length);
            Assert.AreEqual(3, consumer.All.Length);
        }

        [Export]
        public class Consumer
        {
            public IKeyed[] Keyed { get; }
            public IKeyed[] All { get; }
            public Consumer([Import("the key")] IKeyed[] keyed, IKeyed[] all)
            {
                Keyed = keyed;
                All = all;
            }
        }

        public interface IKeyed {}

        [Export("the key", typeof(IKeyed))]
        public class A : IKeyed
        {
        }

        [Export("the key", typeof(IKeyed))]
        public class B : IKeyed
        {
        }

        [Export(typeof(IKeyed))]
        public class C : IKeyed
        {
        }
    }
}
