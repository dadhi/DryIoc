using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.Experimental;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportAsDecoratorTests
    {
        [Test]
        public void Decorator_can_be_applied_based_on_Name()
        {
            var container = new Container();
            container.RegisterExports(typeof(LoggingHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var slow = container.Resolve<IHandler>("slow");
            Assert.That(slow, Is.InstanceOf<LoggingHandlerDecorator>());
            Assert.That(((LoggingHandlerDecorator)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_both_name_and_Metadata()
        {
            var container = new Container();
            container.RegisterExports(typeof(TransactHandlerDecorator), typeof(FastHandler), typeof(SlowHandler),
                typeof(TransactHandler));

            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<SlowHandler>());
            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var transact = container.Resolve<IHandler>("transact");
            Assert.That(transact, Is.InstanceOf<TransactHandlerDecorator>());
            Assert.That(((TransactHandlerDecorator)transact).Handler, Is.InstanceOf<TransactHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_custom_condition()
        {
            var container = new Container();
            container.RegisterExports(typeof(CustomHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var fast = container.Resolve<IHandler>("slow");
            Assert.That(fast, Is.InstanceOf<CustomHandlerDecorator>());
            Assert.That(((CustomHandlerDecorator)fast).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_will_ignore_Import_attribute_on_decorated_service_constructor()
        {
            var container = new Container();
            container.RegisterExports(typeof(FastHandler), typeof(SlowHandler), typeof(DecoratorWithFastHandlerImport));

            var slow = container.Resolve<IHandler>("slow");

            Assert.That(slow, Is.InstanceOf<DecoratorWithFastHandlerImport>());
            Assert.That(((DecoratorWithFastHandlerImport)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_supports_matching_by_service_key()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FoohHandler), typeof(BlahHandler), typeof(FoohDecorator));

            var handler = container.Resolve<IHandler>(BlahFooh.Fooh);

            Assert.That(handler, Is.InstanceOf<FoohDecorator>());
            Assert.That(((FoohDecorator)handler).Handler, Is.InstanceOf<FoohHandler>());
        }

        [Test]
        public void Decorator_may_be_applied_to_Func_decorated()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DecoratedResult), typeof(FuncDecorator));

            var me = container.Resolve<IDecoratedResult>();
            var result = me.GetResult();

            Assert.AreEqual(2, result);
        }

        [Test]
        public void Only_single_resolution_should_present_for_decorated_service()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DecoratedResult), typeof(FuncDecorator));

            var registrations = container.GetServiceRegistrations()
                .Where(r => r.ServiceType == typeof(IDecoratedResult))
                .ToArray();

            Assert.AreEqual(1, registrations.Length);
        }

        [Explicit("Related to #141: Support Decorators with open-generic factory methods of T")]
        public void Can_export_decorator_of_T()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(Bee), typeof(Dd));

            container.Resolve<IBug>();
        }

        [Test]
        public void Decorator_RegistrationOrder_can_be_used_to_control_order_of_composition()
        {
            var container = new Container();
            // It's important to note that the outer and inner decorators are provided out of order.
            // This could happen with MEF since the order that the decorators are added to the container is based upon when they are discovered in the assemblies.
            container.RegisterExports(typeof(ServiceAReal), typeof(ServiceADecoratorOuter), typeof(ServiceADecoratorInner));

            var svc = container.Resolve<IServiceA>();
            
            Assert.That(svc, Is.InstanceOf<ServiceADecoratorOuter>(), "Even though the outer decorator was 'discovered' first, it was registered last because of a higher Order");
            Assert.AreEqual(3, svc.GetResult(), "Verify that both decorators were found.");
        }

        [Test]
        public void Can_register_decorator_of_T()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(X), typeof(DecoratorFactory));

            var x = container.Resolve<X>();
            Assert.IsTrue(x.IsStarted);
        }

        [Test]
        public void Can_register_decorator_of_T_without_breaking_other_exports()
        {
            var di = DI.New().WithMef();

            di.RegisterExports(typeof(Y), typeof(X), typeof(DecoratorFactory));

            di.Resolve<Y>();
        }

        public interface IStartable
        {
            void Start();
        }

        [Export]
        public class Y { }

        [Export]
        public class X : IStartable
        {
            public bool IsStarted { get; private set; }

            public void Start()
            {
                IsStarted = true;
            }
        }

        public static class DecoratorFactory
        {
            [Export, AsDecorator]
            public static T Decorate<T>(T service) where T : IStartable
            {
                service.Start();
                return service;
            }
        }

        internal interface IBug { }
        
        [Export(typeof(IBug))]
        internal class Bee : IBug
        {
        }

        internal static class Dd
        {
            [Export(typeof(IBug)), AsDecorator]
            public static TBug Decorate<TBug>(TBug bug) where TBug : IBug
            {
                return bug;
            }
        }
    }
}


