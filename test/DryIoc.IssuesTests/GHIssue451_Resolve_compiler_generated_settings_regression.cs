using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue451_Resolve_compiler_generated_settings_regression
    {
        [Test]
        public void ExportSingletonPropertyWorks()
        {
            var container = new Container().WithMef()
                .With(rules => rules
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithDefaultReuse(Reuse.ScopedOrSingleton));

            container.RegisterExports(typeof(Settings), typeof(Application));

            var app = container.Resolve<IApplication>();
            Assert.IsInstanceOf<Settings>(app.Settings);
        }

        public interface IApplication
        {
            ISettings Settings { get; }
        }

        [Export(typeof(IApplication))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        internal class Application : IApplication
        {
            [Import]
            public ISettings Settings { get; set; }
        }

        // read-only interface to access the application settings
        public interface ISettings { }

        // additional code
        partial class Settings : ISettings
        {
            [Export(typeof(ISettings))]
            private static ISettings Singleton => Default; // returns default instance
        }

        // generated code from App.settings
        [System.Runtime.CompilerServices.CompilerGenerated]
        internal sealed partial class Settings
        {
            public static Settings Default { get; } = new Settings();
        }
    }
}