using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var services = AttributedModel.Scan(new[] { assembly }).ToArray();

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

            Assert.That(services, Is.EqualTo(infos));
        }

        [Test]
        public void Given_deserialized_data_When_registering_scanned_data_into_container_Then_metadata_should_correctly_registered_too()
        {
            // Given
            var assembly = typeof(TransientService).Assembly;
            var services = AttributedModel.Scan(new[] { assembly }).ToArray();

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
            container.RegisterExports(infos);

            // Then
            var factories = container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));
        }

        private static RuntimeTypeModel CreateModel()
        {
            var model = TypeModel.Create();
            model.Add(typeof(ServiceKeyInfo), false).SetSurrogate(typeof(ServiceKeyInfoSurrogate));
            model.Add<RegistrationInfo>();
            model.Add<ExportInfo>();
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
    public sealed class ServiceKeyInfoSurrogate
    {
        [ProtoMember(1)] public string KeyAsString;
        [ProtoMember(2)] public int? KeyAsInt;
        [ProtoMember(3)] public ServiceKey? KeyAsServiceKey;
        [ProtoMember(4)] public BlahFooh? KeyAsBlahFooh;
        // NOTE: add your types here

        public static implicit operator ServiceKeyInfoSurrogate(ServiceKeyInfo info)
        {
            if (info == null)
                return null;

            var surrogate = new ServiceKeyInfoSurrogate();
            var key = info.Key;
            if (key is ServiceKey)
                surrogate.KeyAsServiceKey = (ServiceKey)key;
            if (key is BlahFooh)
                surrogate.KeyAsBlahFooh = (BlahFooh)key;
            if (key is int)
                surrogate.KeyAsInt = (int?)key;
            if (key is string)
                surrogate.KeyAsString = (string)key;
            return surrogate;
        }

        public static implicit operator ServiceKeyInfo(ServiceKeyInfoSurrogate surrogate)
        {
            if (surrogate == null)
                return null;

            var info = new ServiceKeyInfo();
            if (surrogate.KeyAsServiceKey != null)
                info.Key = surrogate.KeyAsServiceKey;
            if (surrogate.KeyAsBlahFooh != null)
                info.Key = surrogate.KeyAsBlahFooh;
            if (surrogate.KeyAsInt != null)
                info.Key = surrogate.KeyAsInt;
            if (surrogate.KeyAsString != null)
                info.Key = surrogate.KeyAsString;
            return info;
        }
    }
}
