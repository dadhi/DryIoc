using Autofac;
using Autofac.Core;
using NUnit.Framework;

#if DRYIOC_SYNTAX_TESTS
namespace DryIoc.Syntax.Autofac.UnitTests
#else
namespace DryIoc.IssuesTests
#endif
{
    [TestFixture]
    public class Issue123_TipsForMigrationFromAutofac_WithParameter : ITest
    {
        public int Run()
        {
            Injecting_const_string_via_delegate();
            Injecting_const_string_via_named_parameter();
            Injecting_const_string_via_typed_parameter();
            Injecting_const_string_via_resolved_parameter();
            return 4;
        }

        [Test]
        public void Injecting_const_string_via_delegate()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new ConfigReader("sectionName")).As<IConfigReader>();

            var container = builder.Build();

            var x = container.Resolve<IConfigReader>();
            Assert.AreEqual("sectionName", x.SectionName);
        }

        [Test]
        public void Injecting_const_string_via_named_parameter()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConfigReader>()
                   .As<IConfigReader>()
                   .WithParameter("configSectionName", "sectionName");

            var container = builder.Build();

            var x = container.Resolve<IConfigReader>();
            Assert.AreEqual("sectionName", x.SectionName);
        }

        [Test]
        public void Injecting_const_string_via_typed_parameter()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConfigReader>()
                   .As<IConfigReader>()
                   .WithParameter(new TypedParameter(typeof(string), "sectionName"));

            var container = builder.Build();

            var x = container.Resolve<IConfigReader>();
            Assert.AreEqual("sectionName", x.SectionName);
        }

        [Test]
        public void Injecting_const_string_via_resolved_parameter()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConfigReader>()
                   .As<IConfigReader>()
                   .WithParameter(
                       (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "configSectionName",
                       (pi, ctx) => "sectionName");

            var container = builder.Build();

            var x = container.Resolve<IConfigReader>();
            Assert.AreEqual("sectionName", x.SectionName);
        }
    }

    public interface IConfigReader
    {
        string SectionName { get; }
    }

    public class ConfigReader : IConfigReader
    {
        public string SectionName { get; }

        public ConfigReader(string configSectionName) => SectionName = configSectionName;
    }
}
