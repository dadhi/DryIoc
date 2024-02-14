using System;
using System.Linq;
using DryIoc.ImTools;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue228_Updated_DryIoc_from_4_to_4_1_in_Unity_Engine_project_keyed_register_resolve_wont_work_anymore : ITest
    {
        public int Run()
        {
            // Mef Lazy<T, M> wrapper
            Test_the_metadata_with_LazyMeta_and_the_same_keys_But_only_one_key_matches_the_metadata();

            Test_the_Func_metadata_and_the_Diff_keys_But_only_one_key_matches_the_metadata();
            Test_the_Func_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata();
            Test_the_Lazy_metadata_and_the_Diff_keys_But_only_one_key_matches_the_metadata();
            Test_the_Lazy_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata();
            Test_the_metadata_with_Lazy_and_the_same_keys_But_only_one_key_matches_the_metadata();
            Test_the_Lazy_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata_When_resolving_single_thing();
            Test_the_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata_When_resolving_single_thing();
            Test_the_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata();
            Test_the_metadata_and_the_same_keys();
            For_multiple_same_key_registrations_all_should_be_returned();
            Should_be_able_to_get_two_keyed_registrations();

            return 12;
        }

        [Test]
        public void Should_be_able_to_get_two_keyed_registrations()
        {
            var container = new Container();

            container.Register<Iface, A>(serviceKey: Keys.A);
            container.Register<Iface, B>(serviceKey: Keys.B);

            var ab = container.Resolve<Iface[]>();
            Assert.AreEqual(2, ab.Length);

            var a = container.Resolve<Iface>(Keys.A);
            Assert.IsInstanceOf<A>(a);

            var rs = container.GetServiceRegistrations().Where(x => x.ServiceType == typeof(Iface)).ToArray();
            Assert.AreEqual(2, rs.Length);

            var fs = container.GetRegisteredFactories(typeof(Iface), null, FactoryType.Service).ToArray();
            Assert.AreEqual(2, fs.Length);
        }

        [Test]
        public void For_multiple_same_key_registrations_all_should_be_returned()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A);
            container.Register<Iface, B>(serviceKey: Keys.A);

            var ab = container.Resolve<Iface[]>();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.Resolve<Iface[]>(serviceKey: Keys.A);
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.ResolveMany<Iface>().ToArray();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.ResolveMany<Iface>(serviceKey: Keys.A).ToArray();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Iface>(Keys.A));
            Assert.AreEqual(Error.NameOf(Error.ExpectedSingleDefaultFactory), ex.ErrorName);

            var rs = container.GetServiceRegistrations().Where(x => x.ServiceType == typeof(Iface)).ToArray();
            Assert.AreEqual(2, rs.Length);

            var fs = container.GetRegisteredFactories(typeof(Iface), null, FactoryType.Service).ToArray();
            Assert.AreEqual(2, fs.Length);
        }

        [Test]
        public void Test_the_metadata_and_the_same_keys()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "b"));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Meta<Iface, string>[]>(serviceKey: Keys.A);
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0].Value);
            Assert.AreEqual("a", ab[0].Metadata);
            Assert.IsInstanceOf<B>(ab[1].Value);
            Assert.AreEqual("b", ab[1].Metadata);
        }

        [Test]
        public void Test_the_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Meta<Iface, string>[]>(serviceKey: Keys.A);
            Assert.AreEqual(1, ab.Length);
            Assert.IsInstanceOf<A>(ab[0].Value);
            Assert.AreEqual("a", ab[0].Metadata);
        }

        [Test]
        public void Test_the_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata_When_resolving_single_thing()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var a = container.Resolve<Meta<Iface, string>>(serviceKey: Keys.A);
            Assert.IsInstanceOf<A>(a.Value);
            Assert.AreEqual("a", a.Metadata);
        }

        [Test]
        public void Test_the_Lazy_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata_When_resolving_single_thing()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var a = container.Resolve<Lazy<Meta<Iface, string>>>(serviceKey: Keys.A);
            Assert.IsInstanceOf<A>(a.Value.Value);
            Assert.AreEqual("a", a.Value.Metadata);
        }

        [Test]
        public void Test_the_Lazy_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Lazy<Meta<Iface, int>>[]>(serviceKey: Keys.A);

            // todo: @feature Curently the Lazy is not able to filter based on the metadata, and other wrappers down the line
            // the workaround is to use the Meta<Lazy<T>, M> instead of Lazy<Meta<T, M>>
            Assert.AreEqual(2, ab.Length);
        }

        [Test]
        public void Test_the_Lazy_metadata_and_the_Diff_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container();

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.B, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Lazy<Meta<Iface, int>>[]>();

            // todo: @feature Curently the Lazy is not able to filter based on the metadata, and other wrappers down the line
            Assert.AreEqual(3, ab.Length);
        }

        [Test]
        public void Test_the_metadata_with_Lazy_and_the_same_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Meta<Lazy<Iface>, int>[]>(serviceKey: Keys.A);
            Assert.AreEqual(1, ab.Length);
            Assert.IsInstanceOf<B>(ab[0].Value.Value);
            Assert.AreEqual(42, ab[0].Metadata);
        }

        [Test]
        public void Test_the_metadata_with_LazyMeta_and_the_same_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container().WithMef();

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Lazy<Iface, int>[]>(serviceKey: Keys.A);
            Assert.AreEqual(1, ab.Length);
            Assert.IsInstanceOf<B>(ab[0].Value);
            Assert.AreEqual(42, ab[0].Metadata);
        }

        [Test]
        public void Test_the_Func_metadata_and_the_Diff_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container();

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.B, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Func<Meta<Iface, int>>[]>();
            Assert.AreEqual(1, ab.Length);

            var b = ab[0].Invoke();
            Assert.IsInstanceOf<B>(b.Value);
            Assert.AreEqual(42, b.Metadata);
        }

        [Test]
        public void Test_the_Func_metadata_and_the_same_keys_But_only_one_key_matches_the_metadata()
        {
            var container = new Container();

            container.Register<Iface, A>(serviceKey: Keys.A, setup: Setup.With(metadataOrFuncOfMetadata: "a"));
            container.Register<Iface, B>(serviceKey: Keys.B, setup: Setup.With(metadataOrFuncOfMetadata: 42));
            container.Register<Iface, C>(serviceKey: Keys.C, setup: Setup.With(metadataOrFuncOfMetadata: "c"));

            var ab = container.Resolve<Func<Meta<Iface, string>>[]>(serviceKey: Keys.A);
            Assert.AreEqual(1, ab.Length);

            var a = ab[0].Invoke();
            Assert.IsInstanceOf<A>(a.Value);
            Assert.AreEqual("a", a.Metadata);
        }

        public interface Iface {}

        public class A : Iface {}
        public class B : Iface {}
        public class C : Iface {}

        public enum Keys { A, B, C }
    }
}