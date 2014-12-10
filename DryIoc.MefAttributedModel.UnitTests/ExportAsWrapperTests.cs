using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportAsWrapperTests
    {
        [Test]
        public void Exporting_IFactory_as_generic_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(typeof(FactoryConsumer), typeof(One), typeof(DryFactory<>));

            var consumer = container.Resolve<FactoryConsumer>();

            Assert.That(consumer.One, Is.Not.Null);
        }

        [Test]
        public void Exporting_IFactory_with_arguments_as_generic_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(typeof(FactoryWithArgsConsumer), typeof(Two), typeof(DryFactory<,>));

            var consumer = container.Resolve<Func<string, FactoryWithArgsConsumer>>();

            Assert.That(consumer("blah").Two.Message, Is.EqualTo("blah"));
        }

        [Test]
        public void Exporting_as_non_generic_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(typeof(Service), typeof(MyDisposable));

            var disposable = container.Resolve<MyDisposable>(typeof(Service));

            Assert.That(disposable.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Exporting_non_generic_wrapper_with_generic_index_should_Throw()
        {
            var container = new Container();
            container.RegisterExports(typeof(Service), typeof(MyDisposableWronglyExported));

            var ex = Assert.Throws<AttributedModelException>(() => 
                container.Resolve<MyDisposableWronglyExported>(typeof(Service)));

            Assert.AreEqual(ex.Error, Error.NO_WRAPPED_TYPE_EXPORTED_WRAPPER);
            Assert.That(ex.Message, Is.StringContaining(
                "Exported non-generic wrapper type DryIoc.MefAttributedModel.UnitTests.ExportAsWrapperTests.MyDisposableWronglyExported"));
        }

        [Test]
        public void Exporting_generic_wrapper_with_wrong_generic_arg_index_should_throw_meaningful_exception()
        {
            var container = new Container();
            container.RegisterExports(typeof(MyFactoryWrapperExportedWithWrongIndex<>), typeof(Service));

            var ex = Assert.Throws<AttributedModelException>(() => 
                container.Resolve<MyFactoryWrapperExportedWithWrongIndex<IService>>());

            Assert.AreEqual(ex.Error, Error.WRAPPED_ARG_INDEX_OUT_OF_BOUNDS);
            Assert.That(ex.Message, Is.StringContaining(
                "Exported generic wrapper type DryIoc.MefAttributedModel.UnitTests.ExportAsWrapperTests.MyFactoryWrapperExportedWithWrongIndex<DryIoc.MefAttributedModel.UnitTests.CUT.IService> specifies generic argument index 1 outside of argument list size"));
        }

        [Export, AsWrapper(typeof(IService))]
        public class MyDisposable
        {
            public IService Service { get; set; }

            public MyDisposable(IService service)
            {
                Service = service;
            }
        }

        [Export, AsWrapper(0)]
        public class MyDisposableWronglyExported
        {
            public IService Service { get; set; }

            public MyDisposableWronglyExported(IService service)
            {
                Service = service;
            }
        }

        [Export, AsWrapper(1)]
        public class MyFactoryWrapperExportedWithWrongIndex<T>
        {
            public T Service { get; set; }

            public MyFactoryWrapperExportedWithWrongIndex(T service)
            {
                Service = service;
            }
        }
    }
}