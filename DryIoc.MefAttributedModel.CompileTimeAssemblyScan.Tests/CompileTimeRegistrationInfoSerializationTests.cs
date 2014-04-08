using System;
using System.IO;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;
using ProtoBuf;
using ProtoBuf.Meta;

namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    [TestFixture]
    public class CompileTimeRegistrationInfoSerializationTests
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
            var services = AttributedModel.DiscoverExportsInAssemblies(new[] { assembly }).ToArray();

            // When
            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var model = CreateModel();
            using (var file = File.Create(DATA_FILE))
                model.Serialize(file, services);

            // Then
            var loadedModel = CreateModel();
            TypeExportInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = (TypeExportInfo[])loadedModel.Deserialize(file, null, typeof(TypeExportInfo[]));

            Assert.That(services, Is.EqualTo(infos));
        }

        [Test]
        public void Given_deserialized_data_When_registering_scanned_data_into_container_Then_metadata_should_correctly_registered_too()
        {
            // Given
            var assembly = typeof(TransientService).Assembly;
            var services = AttributedModel.DiscoverExportsInAssemblies(new[] { assembly }).ToArray();

            if (File.Exists(DATA_FILE))
                File.Delete(DATA_FILE);

            var model = CreateModel();
            using (var file = File.Create(DATA_FILE))
                model.Serialize(file, services);

            var loadedModel = CreateModel();
            TypeExportInfo[] infos;
            using (var file = File.OpenRead(DATA_FILE))
                infos = (TypeExportInfo[])loadedModel.Deserialize(file, null, typeof(TypeExportInfo[]));

            // When
            var container = new Container();
            container.RegisterExports(infos);

            // Then
            var factories = container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));
        }

        private static RuntimeTypeModel CreateModel()
        {
            var model = TypeModel.Create();
            model.Add<TypeExportInfo>();
            model.Add(typeof(ExportInfo), false).SetSurrogate(typeof(ExportInfoSurrogate));
            model.Add<GenericWrapperInfo>();
            model.Add<DecoratorInfo>();
            return model;
        }
    }

    public static class RuntimeTypeModelExt
    {
        public static MetaType Add<T>(this RuntimeTypeModel model)
        {
            var publicFields = typeof(T).GetFields().Select(x => x.Name).ToArray();
            return model.Add(typeof(T), false).Add(publicFields);
        }
    }

    [ProtoContract]
    public sealed class ExportInfoSurrogate
    {
        [ProtoMember(1)]
        public Type ServiceType { get; set; }

        [ProtoMember(2)]
        public string ServiceKeyAsString { get; set; }
        [ProtoMember(3)]
        public int? ServiceKeyAsInt { get; set; }
        [ProtoMember(4)]
        public ServiceKey? ServiceKeyAsServiceKey { get; set; }
        // TODO: other types here

        public static implicit operator ExportInfoSurrogate(ExportInfo info)
        {
            if (info == null)
                return null;

            var surrogate = new ExportInfoSurrogate { ServiceType = info.ServiceType };

            var serviceKey = info.ServiceKey;
            if (serviceKey is ServiceKey)
                surrogate.ServiceKeyAsServiceKey = (ServiceKey?)serviceKey;
            if (serviceKey is int)
                surrogate.ServiceKeyAsInt = (int?)serviceKey;
            if (serviceKey is string)
                surrogate.ServiceKeyAsString = (string)serviceKey;
            return surrogate;
        }

        public static implicit operator ExportInfo(ExportInfoSurrogate surrogate)
        {
            if (surrogate == null)
                return null;

            var info = new ExportInfo { ServiceType = surrogate.ServiceType };
            if (surrogate.ServiceKeyAsServiceKey != null)
                info.ServiceKey = surrogate.ServiceKeyAsServiceKey;
            if (surrogate.ServiceKeyAsInt != null)
                info.ServiceKey = surrogate.ServiceKeyAsInt;
            if (surrogate.ServiceKeyAsString != null)
                info.ServiceKey = surrogate.ServiceKeyAsString;

            return info;
        }
    }
}
