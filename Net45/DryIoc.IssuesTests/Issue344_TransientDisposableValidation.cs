using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue344_TransientDisposableValidation
    {
        [Test, Ignore("fails")]
        public void Container_with_default_reuse_set_to_singleton_shouldnt_complain_about_transient_disposable_registration()
        {
            var container = new Container().With(r =>
                r.WithDefaultReuseInsteadOfTransient(Reuse.Singleton));

            container.Register<IService, Service>();
        }

        interface IService : IDisposable { }

        class Service : IService
        {
            public void Dispose() { }
        }
    }
}
