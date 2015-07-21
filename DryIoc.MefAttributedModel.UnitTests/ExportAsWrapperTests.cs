using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
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
        public void Exporting_non_generic_wrapper_with_generic_index_should_just_ignore_the_index()
        {
            var container = new Container();
            container.RegisterExports(typeof(Service), typeof(MyDisposableWronglyExported));

            var wrapper = container.Resolve<MyDisposableWronglyExported>();

            Assert.IsInstanceOf<Service>(wrapper.Service);
        }

        [Test]
        public void Exporting_generic_wrapper_with_wrong_generic_arg_index_should_throw_meaningful_exception()
        {
            var container = new Container().WithMefAttributedModel();
            var ex = Assert.Throws<ContainerException>(() => 
            container.RegisterExports(typeof(MyFactoryWrapperExportedWithWrongIndex<>), typeof(Service)));

            Assert.AreEqual(DryIoc.Error.GenericWrapperTypeArgIndexOutOfBounds, ex.Error);
        }

        [Export, AsWrapper(AlwaysWrapsRequiredServiceType = true)]
        public class MyDisposable
        {
            public IService Service { get; set; }

            public MyDisposable(IService service)
            {
                Service = service;
            }
        }

        [Export, AsWrapper]
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