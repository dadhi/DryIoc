using System;
using System.IO;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;
using Wire;

namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    [TestFixture]
    public class CompileTimeRegistrationInfoSerializationTests
    {
        private const string DATA_FILE = "SerializedExports.bin";

        private string _originalDirectory;
        private string _temporaryTestDirectory;

        [SetUp]
        public void SetupTestDirectory()
        {
            _temporaryTestDirectory = Path.GetRandomFileName();
            Directory.CreateDirectory(_temporaryTestDirectory);

            _originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_temporaryTestDirectory);
        }

        [TearDown]
        public void TearDownTestDirectory()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
            if (Directory.Exists(_temporaryTestDirectory))
                Directory.Delete(_temporaryTestDirectory, true);
        }

        [Test]
        public void Given_scnanned_assembly_When_serialize_data_Then_deserialize_will_return_the_same_data()
        {
            var assembly = typeof(TransientService).GetAssembly();
            var services = AttributedModel.Scan(new[] {assembly}).ToArray();

            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var serializer = new Serializer();

            using (var file = File.Create(DATA_FILE))
                serializer.Serialize(services, file);

            ExportedRegistrationInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = serializer.Deserialize<ExportedRegistrationInfo[]>(file);

            Assert.AreEqual(services, infos);
        }

        [Test]
        public void Given_deserialized_data_When_registering_scanned_data_into_container_Then_metadata_should_correctly_registered_too()
        {
            var assembly = typeof(TransientService).GetAssembly();
            var services = AttributedModel.Scan(new[] {assembly}).ToArray();

            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var serializer = new Serializer();
            using (var file = File.Create(DATA_FILE))
                serializer.Serialize(services, file);

            ExportedRegistrationInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = serializer.Deserialize<ExportedRegistrationInfo[]>(file);

            var container = new Container().WithMef();
            container.RegisterExports(infos);

            var factories = container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));
        }
    }
}
