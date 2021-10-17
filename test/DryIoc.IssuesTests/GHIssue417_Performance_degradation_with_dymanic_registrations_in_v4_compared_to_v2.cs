using System;
using NUnit.Framework;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue417_Performance_degradation_with_dynamic_registrations_in_v4_compared_to_v2
    {
        [Test]
        public void SlowTest()
        {
            var container = new Container().WithMef();

            container.Register<IService, MyService>();

            var x = new Slow();
            container.InjectPropertiesAndFields(x);

            // var y = new Slow();
            // container.InjectPropertiesAndFields(y);

            Assert.IsInstanceOf<MyService>(x.ImportedServices[0].Value);
        }

        public class Slow
        {
            [Import]
            public Lazy<IService>[] ImportedServices { get; set; }
        }

        public interface IService { }
        public class MyService : IService { }
    }
}
