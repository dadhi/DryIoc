using System;
using System.IO;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;
using ProtoBuf.Meta;

namespace DryIoc.AttributedRegistration.CompileTimeAssembliesScan
{
    [TestFixture]
    public class SerializingContainerRegistrationsTests
    {
        private const string DATA_FILE = "DryExports.bin";

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
        //[Ignore]
        public void Given_scnanned_assembly_When_serialize_data_Then_deserialize_will_return_the_same_data()
        {
            // Given
            var assembly = typeof(TransientService).Assembly;
            var services = AttributedRegistrator.ScanAssemblies(new[] { assembly }).ToArray();

            // When
            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var model = CreateModel();
            using (var file = File.Create(DATA_FILE))
                model.Serialize(file, services);

            // Then
            var loadedModel = CreateModel();
            RegistrationInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = (RegistrationInfo[])loadedModel.Deserialize(file, null, typeof(RegistrationInfo[]));

            Assert.AreEqual(services.Length, infos.Length);
            for (int i = 0; i < services.Length; i++)
                Assert.AreEqual(services[i], infos[i]);
        }

        [Test]
        public void Given_deserialized_data_When_registering_scanned_data_into_container_Then_metadata_should_correctly_registered_too()
        {
            // Given
            var assembly = typeof(TransientService).Assembly;
            var services = AttributedRegistrator.ScanAssemblies(new[] { assembly }).ToArray();

            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var model = CreateModel();
            using (var file = File.Create(DATA_FILE))
                model.Serialize(file, services);

            var loadedModel = CreateModel();
            RegistrationInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = (RegistrationInfo[])loadedModel.Deserialize(file, null, typeof(RegistrationInfo[]));

            // When
            var container = new Container();
            container.RegisterExported(infos);

            // Then
            var factories = container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));
        }

        private static RuntimeTypeModel CreateModel()
        {
            var model = TypeModel.Create();

            var serializedTypes = new[] { typeof(ExportInfo), typeof(RegistrationInfo) };

            foreach (var type in serializedTypes)
                model.Add(type, false).Add(type.GetFields().Select(x => x.Name).ToArray());

            return model;
        }
    }
}
