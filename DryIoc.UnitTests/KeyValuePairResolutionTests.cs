using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class KeyValuePairResolutionTests
    {
        [Test]
        public void Resolve_default_registration_as_pair_should_Succeed()
        {
            var container = new Container();
            container.Register<Service>();

            var pair = container.Resolve<KeyValuePair<object, Service>>();

            Assert.That(pair.Key, Is.Null);
            Assert.That(pair.Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolve_indexed_registration_as_pair_When_resolving_with_that_index_should_Succeed()
        {
            var container = new Container();
            container.Register<Service>(serviceKey: 0);

            var pair = container.Resolve<KeyValuePair<object, Service>>(serviceKey: 0);

            Assert.That(pair.Key, Is.EqualTo(0));
            Assert.That(pair.Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolve_indexed_registration_as_pair_When_resolving_without_index_should_Throw()
        {
            var container = new Container();
            container.Register<Service>(serviceKey: 0);

            Assert.Throws<ContainerException>(() => 
                container.Resolve<KeyValuePair<object, Service>>());
        }

        [Test]
        public void Resolve_named_registration_as_pair_When_resolving_with_name_should_Succeed()
        {
            var container = new Container();
            container.Register<Service>(serviceKey: "blah");

            var pair = container.Resolve<KeyValuePair<object, Service>>("blah");

            Assert.That(pair.Key, Is.EqualTo("blah"));
        }

        [Test]
        public void Resolve_named_registration_as_pair_When_resolving_without_name_should_Throw()
        {
            var container = new Container();
            container.Register<Service>(serviceKey: "blah");

            Assert.Throws<ContainerException>(() =>
                container.Resolve<KeyValuePair<object, Service>>());
        }

        [Test]
        public void Resolve_couple_of_default_registrations_as_pair_array_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<KeyValuePair<object, IService>[]>();

            Assert.That(pairs.Length, Is.EqualTo(2));
            Assert.That(pairs[0].Key, Is.EqualTo(DefaultKey.Value));
            Assert.That(pairs[1].Key, Is.EqualTo(DefaultKey.Value.Next()));
        }

        [Test]
        public void Resolve_couple_of_default_registrations_as_pair_Many_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<LazyEnumerable<KeyValuePair<object, IService>>>();

            Assert.That(pairs.Count(), Is.EqualTo(2));
            Assert.That(pairs.First().Key, Is.EqualTo(DefaultKey.Value));
            Assert.That(pairs.Last().Key, Is.EqualTo(DefaultKey.Value.Next()));
        }

        [Test]
        public void When_present_default_and_named_registrations_Then_resolving_as_pairs_should_return_both()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "Yeah!");
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<IEnumerable<KeyValuePair<object, IService>>>().ToArray();

            Assert.That(pairs.Length, Is.EqualTo(2));
            Assert.That(pairs[0].Key, Is.EqualTo(DefaultKey.Value));
            Assert.That(pairs[1].Key, Is.EqualTo("Yeah!"));
        }

        [Test]
        public void When_present_default_and_named_registrations_Then_resolving_as_pairs_with_default_key_should_return_default_only()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "Yeah!");
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<KeyValuePair<DefaultKey, IService>[]>();

            Assert.That(pairs.Length, Is.EqualTo(1));
            Assert.That(pairs[0].Key, Is.EqualTo(DefaultKey.Value));
        }

        [Test]
        public void When_present_default_and_named_registrations_Then_resolving_as_pairs_with_string_key_should_return_named_only()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "Yeah!");
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<KeyValuePair<string, IService>[]>();

            Assert.That(pairs.Length, Is.EqualTo(1));
            Assert.That(pairs[0].Key, Is.EqualTo("Yeah!"));
        }

        [Test]
        public void When_present_default_and_enum_registrations_Then_resolving_as_pairs_with_enum_key_should_return_enum_only()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: EnumKey.Some);
            container.Register<IService, AnotherService>();

            var pairs = container.Resolve<KeyValuePair<EnumKey, IService>[]>();

            Assert.That(pairs.Length, Is.EqualTo(1));
            Assert.That(pairs[0].Key, Is.EqualTo(EnumKey.Some));
            Assert.That(pairs[0].Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_all_type_of_registrations_with_object_key_type()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(serviceKey: "another");
            container.Register<IService, DisposableService>(serviceKey: EnumKey.Some);

            var pairs = container.Resolve<KeyValuePair<object, Func<IService>>[]>();

            Assert.That(pairs.Length, Is.EqualTo(3));
            CollectionAssert.AreEquivalent(new object[] { DefaultKey.Value, "another", EnumKey.Some }, pairs.Select(p => p.Key));
        }
    }
}
