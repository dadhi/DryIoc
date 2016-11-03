using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MoreAttributedModelTests
    {
        [Test]
        public void Can_export_and_resolve_composite()
        {
            var container = new Container().WithMef()
                .With(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            container.RegisterExports(new [] { typeof(IItem<int>).GetAssembly() });

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

        [Test, Ignore("fails on registration, not on resolution")]
        public void Resolving_with_metadata_with_duplicate_key_should_throw()
        {
            var container = new Container();

            container.RegisterExports(typeof(ThrowsForMultipleMetaWithDuplicateName));

            var ex = Assert.Throws<AttributedModelException>(() =>
            container.Resolve<Meta<ThrowsForMultipleMetaWithDuplicateName, object>>());

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
