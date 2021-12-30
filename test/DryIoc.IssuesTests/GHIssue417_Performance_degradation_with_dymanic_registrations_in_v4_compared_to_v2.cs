using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NUnit.Framework;
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

            Assert.IsInstanceOf<MyService>(x.ImportedServices[0].Value);
        }

        [Test, Ignore("todo: @fixme")]
        public void SuperSlowTest()
        {
            var dynamicRulesTriggered = new List<object>();
            IEnumerable<DynamicRegistration> CountDynamicRulesTriggered(Type type, object key) 
            { 
                dynamicRulesTriggered.Add(new { type, key });
                return null;
            }

            var container = new Container(rules => rules.WithDynamicRegistrations(CountDynamicRulesTriggered)).WithMef();

            container.Register<IService, MyService>(setup: Setup.With(metadataOrFuncOfMetadata: "42"));

            var x = new SuperSlow();
            container.InjectPropertiesAndFields(x);

            Assert.IsInstanceOf<MyService>(x.ImportedServices[0].Value);
            Assert.AreEqual(1, dynamicRulesTriggered.Count);
        }

        public class Slow
        {
            [Import]
            public Lazy<IService>[] ImportedServices { get; set; }
        }

        public class SuperSlow
        {
            [Import]
            public Lazy<IService, string>[] ImportedServices { get; set; }
        }

        public interface IService { }
        public class MyService : IService { }
    }
}
