using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.SpeedTestApp.Net40
{
    [TestFixture]
    public class FastSingletonTests 
    {
        [Test]
        public void When_resolving_singleton_Then_used_expression_should_contain_singleton_variable()
        {
            var container = new Container();
            container.Register<SingletonService>(Reuse.Singleton);

            var service = container.Resolve<SingletonService>();

            Assert.That(service, Is.Not.Null.And.SameAs(container.Resolve<SingletonService>()));
        }

        [Test]
        public void When_singleton_appears_only_once_in_expression_Then_expression_should_be_used_as_is()
        {
            var container = new Container();
            container.Register<SingletonService>(Reuse.Singleton);
            container.Register<DirectSingletonConsumer>();

            var consumer = container.Resolve<DirectSingletonConsumer>();

            Assert.That(consumer.Singleton, 
                Is.Not.Null.And.SameAs(container.Resolve<DirectSingletonConsumer>().Singleton));
        }

        [Test]
        public void When_singleton_is_appears_multiple_times_in_expression_Then_it_should_be_assigned_to_local_var_and_reused()
        {
            var container = new Container();
            container.Register<SingletonService>(Reuse.Singleton);
            container.Register<DirectSingletonConsumer>();
            container.Register<SingletonConsumer>();

            var consumer = container.Resolve<SingletonConsumer>();
            var consumerExpr = container.Resolve<DebugExpression<SingletonConsumer>>();

            Assert.That(consumer.Singleton,
                Is.Not.Null.And.SameAs(consumer.DirectConsumer.Singleton));
        }

        [Test]
        public void Multiple_singletons_test()
        {
            var container = new Container();
            container.Register<UseTwoSingletons>();
            container.Register<OneSingleton>(Reuse.Singleton);
            container.Register<AnotherSingleton>(Reuse.Singleton);

            var once = container.Resolve<UseTwoSingletons>();
            var twice = container.Resolve<UseTwoSingletons>();

            Assert.That(once.One, Is.SameAs(twice.One));
            Assert.That(once.Another, Is.SameAs(twice.Another));
        }
    }

    public class OneSingleton {}

    public class AnotherSingleton {}

    public class UseTwoSingletons
    {
        public OneSingleton One { get; set; }
        public AnotherSingleton Another { get; set; }

        public UseTwoSingletons(OneSingleton one, AnotherSingleton another)
        {
            One = one;
            Another = another;
        }
    }

    public sealed class FastSingletonReuse : IReuse
    {
        public static readonly FastSingletonReuse Instance = new FastSingletonReuse();

        public Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr)
        {
            // save scope into separate var to prevent closure on registry.
            var singletonScope = registry.SingletonScope;

            // Create lazy singleton if we have Func somewhere in dependency chain.
            var parent = request.Parent;
            if (parent != null && parent.Enumerate().Any(p =>
                p.OpenGenericServiceType != null && ContainerSetup.FuncTypes.Contains(p.OpenGenericServiceType)))
                return Reuse.GetScopedServiceExpression(Expression.Constant(singletonScope), factoryID, factoryExpr);

            // Otherwise we can create singleton instance right here, and put it into Scope for later disposal.
            var currentScope = registry.CurrentScope; // same as for singletonScope
            var constants = registry.Constants;

            var singleton = singletonScope.GetOrAdd(factoryID, 
                () => Container.CreateFactoryExpression(factoryExpr).Compile()(constants, currentScope, null));
            return registry.GetConstantExpression(singleton, factoryExpr.Type);
        }
    }

    public class SingletonService {}

    public class DirectSingletonConsumer
    {
        public SingletonService Singleton { get; set; }

        public DirectSingletonConsumer(SingletonService singleton)
        {
            Singleton = singleton;
        }
    }

    public class SingletonConsumer
    {
        public SingletonService Singleton { get; set; }
        public DirectSingletonConsumer DirectConsumer { get; set; }

        public SingletonConsumer(SingletonService singleton, DirectSingletonConsumer directConsumer)
        {
            Singleton = singleton;
            DirectConsumer = directConsumer;
        }
    }
}
