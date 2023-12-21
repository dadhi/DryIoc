using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MoreAttributedModelTests : ITest
    {
        public int Run()
        {
            Can_export_and_resolve_composite();
            Works_together_with_ConstructorWithResolvableArguments();
            Can_export_and_resolve_composite_as_lazy_enumerable();
            Resolving_with_metadata_with_duplicate_key_should_throw();
            Can_specify_to_throw_on_second_registration();
            Importing_LazyEnumerable();
            return 6;
        }

        [Test]
        public void Can_export_and_resolve_composite()
        {
            var container = new Container().WithMef();

            container.RegisterExports(new [] { typeof(IItem<int>).GetAssembly() });

            var composite = (CompositeItem<int>)container.Resolve<IItem<int>>("root");
            Assert.AreEqual(2, composite.Items.Length);
        }

        [Test]
        public void Can_export_and_resolve_composite_as_lazy_enumerable()
        {
            var container = new Container().WithMef()
                .With(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            container.RegisterExports(new[] { typeof(IItem<int>).GetAssembly() });

            var composite = (CompositeItem<int>)container.Resolve<IItem<int>>("root");
            Assert.AreEqual(2, composite.Items.Length);
        }

        [Test]
        public void Works_together_with_ConstructorWithResolvableArguments()
        {
            var c = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithMefAttributedModel();

            c.RegisterExports(typeof(MultiCtorSample), typeof(MultiCtorDep));

            var s = c.Resolve<MultiCtorSample>();

            Assert.IsNotNull(s.Dep);
        }

        [Test]
        public void Resolving_with_metadata_with_duplicate_key_should_throw()
        {
            var container = new Container();

            var ex = Assert.Throws<AttributedModelException>(() =>
            container.RegisterExports(typeof(ThrowsForMultipleMetaWithDuplicateName)));

            Assert.AreEqual(Error.DuplicateMetadataKey, ex.Error);
        }

        [Test]
        public void Can_specify_to_throw_on_second_registration()
        {
            var container = new Container().WithMefAttributedModel();

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterExports(typeof(DontThrows), typeof(Throws)));

            Assert.AreEqual(DryIoc.Error.UnableToRegisterDuplicateDefault, ex.Error);
        }

        [Test]
        public void Importing_LazyEnumerable()
        {
            var container = new Container(r => r.WithMefAttributedModel().WithResolveIEnumerableAsLazyEnumerable());
            container.RegisterExports(typeof(UseLazyEnumerable), typeof(Me));

            Assert.IsInstanceOf<LazyEnumerable<Me>>(
                container.Resolve<UseLazyEnumerable>().Mes);
        }

        [ExportEx(IfAlreadyExported.Throw)]
        public class DontThrows { }

        [ExportMany]
        [ExportEx(typeof(DontThrows), IfAlreadyExported.Throw)]
        public class Throws : DontThrows { }

        [ExportWithDisplayName("blah"), WithMetadata("DisplayName", "hey")]
        public class ThrowsForMultipleMetaWithDuplicateName { }
    }
}
