using DryIoc.MefAttributedModel;
using NUnit.Framework;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue380_ExportFactory_throws_Container_disposed_exception : ITest
    {
        public int Run()
        {
            // MefExportFactoryDoesntThrow();
            DryIocExportFactoryDoesntThrow();

            return 2;
        }

        [Test]
        public void MefExportFactoryDoesntThrow()
        {
            var rootCatalog = new TypeCatalog(typeof(SessionManager), typeof(Helper));
            var nonSharedPartsCatalog = new FilteredCatalog(rootCatalog, def =>
            {
                var md = def.Metadata;
                var key = CompositionConstants.PartCreationPolicyMetadataName;
                return !md.ContainsKey(key) ||
                    (CreationPolicy)md[key] == CreationPolicy.Any ||
                    (CreationPolicy)md[key] == CreationPolicy.NonShared;
            });

            var rootContainer = new CompositionContainer(rootCatalog);
            Helper.flag = false;

            Assert.DoesNotThrow(() =>
            {
                // first request
                using (var requestScope = new CompositionContainer(nonSharedPartsCatalog, rootContainer))
                {
                    var sessionManager = requestScope.GetExport<ISessionManager>();
                    var sessionId = sessionManager.Value.CreateSession();
                    Assert.AreEqual("123", sessionId);
                }
            },
            "First request has thrown an exception");

            Assert.DoesNotThrow(() =>
            {
                // second request
                using (var requestScope = new CompositionContainer(nonSharedPartsCatalog, rootContainer))
                {
                    var sessionManager = requestScope.GetExport<ISessionManager>();
                    var sessionId = sessionManager.Value.CreateSession();
                    Assert.AreEqual("321", sessionId);
                }
            },
            "Second request has thrown an exception");
        }

        //[Test]
        public void DryIocExportFactoryDoesntThrow()
        {
            var rootContainer = new Container().WithMef()
               .With(rules => rules
               .WithoutThrowIfDependencyHasShorterReuseLifespan()
               .WithDefaultReuse(Reuse.Scoped));

            rootContainer.RegisterExports(typeof(SessionManager), typeof(Helper));
            Helper.flag = false;

            Assert.DoesNotThrow(() =>
            {
                using (var requestScope = rootContainer.OpenScope())
                {
                    var sessionManager = requestScope.Resolve<ISessionManager>();
                    var sessionId = sessionManager.CreateSession();
                    Assert.AreEqual("123", sessionId);
                }
            });

            Assert.DoesNotThrow(() =>
            {
                using (var requestScope = rootContainer.OpenScope())
                {
                    var sessionManager = requestScope.Resolve<ISessionManager>();
                    var sessionId = sessionManager.CreateSession();
                    Assert.AreEqual("321", sessionId);
                }
            });
        }

        public interface ISessionManager { string CreateSession(); }

        [Export(typeof(ISessionManager)), PartCreationPolicy(CreationPolicy.Shared)]
        public class SessionManager : ISessionManager
        {
            [Import]
            private ExportFactory<IHelper> HelperFactory { get; set; }

            public string CreateSession()
            {
                using (var export = HelperFactory.CreateExport())
                {
                    return export.Value.GetNewId();
                }
            }
        }

        public interface IHelper { string GetNewId(); }

        [Export(typeof(IHelper))]
        public class Helper : IHelper
        {
            public static bool flag;

            public string GetNewId()
            {
                flag = !flag;
                return flag ? "123" : "321";
            }
        }
    }
}
