using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue344_TransientDisposableValidation
    {
        [Test]
        public void Container_with_default_reuse_set_to_singleton_shouldnt_complain_about_transient_disposable_registration()
        {
            var container = new Container().With(r =>
                r.WithDefaultReuseInsteadOfTransient(Reuse.Singleton));

            Assert.DoesNotThrow(() =>
                container.Register<IService, Service>());
        }

        [Test, Ignore("fails")]
        public void Container_with_Mef_support_with_tracking_disposable_transients_shouldnt_complain_about_transient_disposable_registration()
        {
            var container = new Container().WithMef()
                .With(rules => rules.WithTrackingDisposableTransients());

            Assert.DoesNotThrow(() =>
                container.RegisterExports(typeof(DataContext)));
        }

        interface IService : IDisposable { }

        class Service : IService
        {
            public void Dispose() { }
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        class DataContext : IDisposable
        {
            public void Dispose() { }
        }
    }
}
