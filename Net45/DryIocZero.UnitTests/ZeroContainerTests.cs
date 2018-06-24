using System.Linq;
using NUnit.Framework;
using DryIoc.MefAttributedModel.UnitTests.CUT;

namespace DryIocZero.UnitTests
{
    [TestFixture]
    public class ZeroContainerTests
    {
        [Test]
        public void Can_Register_default_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new Potato());

            var potato = container.Resolve<Potato>();

            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_keyed_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new Potato(), "mashed");

            var potato = container.Resolve<Potato>("mashed");

            Assert.IsNotNull(potato);
        }

        [Test]
        public void Should_throw_on_incompatible_service_produced_by_delegate_and_required_service_type()
        {
            var container = new Container();

            container.RegisterDelegate(typeof(Cabbage), _ => new Potato());

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Cabbage>());

            Assert.AreEqual(Error.ProducedServiceIsNotAssignableToRequiredServiceType, ex.Error);
        }

        [Test]
        public void Should_throw_if_null_keyed_service_is_nor_registered_nor_generated()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve(typeof(Potato)));

            Assert.AreEqual(Error.UnableToResolveDefaultService, ex.Error);
        }

        [Test]
        public void Should_throw_if_non_null_keyed_service_is_nor_registered_nor_generated()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve(typeof(Potato), "x"));

            Assert.AreEqual(Error.UnableToResolveKeyedService, ex.Error);
        }

        [Test]
        public void Can_use_instance()
        {
            var container = new Container();
            var potato = new Potato();
            container.UseInstance(potato);

            var resolvedPotato = container.Resolve(typeof(Potato), false);

            Assert.AreSame(potato, resolvedPotato);
        }

        internal class Potato { }
        internal class Cabbage { }

        [Test]
        public void Can_open_scope()
        {
            var container = new Container();
            container.RegisterDelegate(_ => new Potato());
            using (var scope = container.OpenScope())
            {
                var potato = scope.Resolve(typeof(Potato), IfUnresolved.Throw);
                Assert.IsNotNull(potato);
            }
        }

        [Test]
        public void Can_register_scoped_delegate_and_resolve_it_in_open_scope()
        {
            var container = new Container();

            container.RegisterDelegate(Reuse.InCurrentScope, _ => new Potato());

            using (var scope = container.OpenScope())
            {
                var potato = scope.Resolve<Potato>();
                Assert.IsNotNull(potato);
                Assert.AreSame(potato, scope.Resolve<Potato>());
            }

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<Potato>());

            Assert.AreEqual(Error.NoCurrentScope, ex.Error);
        }

        [Test]
        public void Can_register_singleton_delegate()
        {
            var container = new Container();

            container.RegisterDelegate(Reuse.Singleton, _ => new Potato());

            var potato = container.Resolve<Potato>();
            using (var scope = container.OpenScope())
            {
                Assert.IsNotNull(potato);
                Assert.AreSame(potato, scope.Resolve<Potato>());
            }

            Assert.AreSame(potato, container.Resolve<Potato>());
        }

        [Test]
        public void Dispose_should_remove_registrations()
        {
            var container = new Container();
            container.RegisterDelegate(_ => new Potato());
            container.Dispose();
            Assert.Throws<ContainerException>(() => container.Resolve(typeof(Potato), false));
        }

        [Test]
        public void Can_resolve_singleton()
        {
            var container = new Container();

            var service = container.Resolve(typeof(ISomeDb), false);
            Assert.IsNotNull(service);
            Assert.AreSame(service, container.Resolve(typeof(ISomeDb), false));
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var container = new Container();

            var service = container.Resolve(typeof(IMultiExported), "c");
            Assert.IsNotNull(service);
            Assert.AreSame(service, container.Resolve(typeof(IMultiExported), "c"));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve(typeof(NotRegistered), false));

            Assert.AreEqual(ex.Error, Error.UnableToResolveDefaultService);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var container = new Container();

            var nullService = container.Resolve(typeof(NotRegistered), true);

            Assert.IsNull(nullService);
        }

        [Test]
        public void Can_resolve_many()
        {
            var container = new Container();

            var handlers = container.ResolveMany<ISomeDb>().ToArray();

            Assert.AreEqual(1, handlers.Length);
        }

        [Test]
        public void Can_resolve_generated_only_service()
        {
            var container = new Container();

            var b = container.ResolveGeneratedOrGetDefault(typeof(B));
            Assert.IsNotNull(b);

            var nr = container.ResolveGeneratedOrGetDefault(typeof(NotRegistered));
            Assert.IsNull(nr);
        }

        [Test]
        public void Can_resolve_generated_only_keyed_service()
        {
            var container = new Container();
            var m = container.ResolveGeneratedOrGetDefault(typeof(MultiExported), "b");
            Assert.IsNotNull(m);

            var b = container.ResolveGeneratedOrGetDefault(typeof(B), "b");
            Assert.IsNull(b);
        }

        [Test]
        public void Can_resolve_generated_only_many_services()
        {
            var container = new Container();
            var ms = container.ResolveManyGeneratedOrGetEmpty(typeof(IMultiExported));
            Assert.IsTrue(ms.Any());

            var ns = container.ResolveManyGeneratedOrGetEmpty(typeof(NotRegistered));
            Assert.IsTrue(!ns.Any());
        }

        [Test]
        public void Can_resolve_many_of_both_generated_and_runtime_default_and_keyed_services()
        {
            var container = new Container();

            container.RegisterDelegate<IMultiExported>(_ => new AnotherMulti());
            container.RegisterDelegate<IMultiExported>(_ => new AnotherMulti(), "another");

            var ms = container.ResolveMany(typeof(IMultiExported)).Cast<IMultiExported>().ToArray();
            Assert.AreEqual(2, ms.Count(m => m.GetType() == typeof(AnotherMulti)));
            Assert.Greater(ms.Length, 2);
        }

        [Test]
        public void Can_resolve_many_of_both_generated_and_runtime_keyed_service_with_specified_key()
        {
            var container = new Container();

            container.RegisterDelegate<IMultiExported>(_ => new AnotherMulti(), "c");

            var ms = container.ResolveMany<IMultiExported>("c").ToArray();
            Assert.AreEqual(1, ms.Count(m => m.GetType() == typeof(AnotherMulti)));
            Assert.AreEqual(2, ms.Length);
        }

        [Test]
        public void Should_exclude_composite_key_from_many()
        {
            var container = new Container();

            var ms = container.ResolveMany<IMultiExported>().ToArray();
            Assert.AreEqual(3, ms.Length);
        }

        internal class AnotherMulti : IMultiExported { }

        internal class NotRegistered { }

        internal class X
        {
            public Y Y1 { get; private set; }

            public Y Y2 { get; private set; }

            public X(Y y1, Y y2)
            {
                Y1 = y1;
                Y2 = y2;
            }
        }

        internal class Y
        {
        }
    }
}