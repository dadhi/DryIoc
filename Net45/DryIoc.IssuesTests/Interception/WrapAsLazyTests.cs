using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class WrapAsLazyTests
    {
        [Test]
        public void Service_can_be_registered_as_always_lazy()
        {
            var c = new Container();
            c.RegisterAsLazy<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Interface_can_be_resolved_as_always_lazy()
        {
            var c = new Container();
            c.ResolveAsLazy<IAlwaysLazy>();
            c.Register<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Interface_not_available_at_compile_time_can_be_resolved_as_always_lazy()
        {
            var c = new Container();
            var typeLoadedFromExternalAssembly = Assembly.GetExecutingAssembly().GetType("DryIoc.IssuesTests.Interception.IAlwaysLazy");
            c.ResolveAsLazy(typeLoadedFromExternalAssembly);

            c.Register<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Circular_dependency_is_normally_not_allowed()
        {
            var container = new Container();
            container.Register<IChicken, Chicken>();
            container.Register<IEgg, Egg>();

            // this call doesn't detect the circular dependency
            container.Validate();

            // but the resolution fails
            Assert.Throws<ContainerException>(() => container.Resolve<IEgg>());
        }

        [Test]
        public void Circular_dependency_is_allowed_when_services_are_registered_as_lazy()
        {
            var container = new Container();
            container.RegisterAsLazy<IChicken, Chicken>();
            container.RegisterAsLazy<IEgg, Egg>();

            var e = container.Resolve<IEgg>();
            Assert.NotNull(e);
            Assert.NotNull(e.Chicken);

            var c = container.Resolve<IChicken>();
            Assert.NotNull(c);
            Assert.NotNull(c.Egg);
        }

        [Test, Ignore]
        public void Circular_dependency_handling_doesnt_actually_work_for_the_deeper_levels()
        {
            var container = new Container();
            container.RegisterAsLazy<IChicken, Chicken>();
            container.RegisterAsLazy<IEgg, Egg>();

            var e = container.Resolve<IEgg>();
            Assert.NotNull(e);
            Assert.NotNull(e.Chicken);
            Assert.NotNull(e.Chicken.Egg); // this call throws a circular dependency exception
            Assert.NotNull(e.Chicken.Egg.Chicken);
            Assert.NotNull(e.Chicken.Egg.Chicken.Egg);
            Assert.NotNull(e.Chicken.Egg.Chicken.Egg.Chicken);
        }

        [Test]
        public void Resolve_array_works_for_simple_decorators()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            container.Register<ICommand, TrivialDecorator>(setup: Setup.Decorator);

            // everything resolves fine
            var commands = container.Resolve<ICommand[]>();
            Assert.AreEqual(2, commands.Length);
            Assert.IsInstanceOf<TrivialDecorator>(commands[0]);
            Assert.IsInstanceOf<TrivialDecorator>(commands[1]);

            // and also works fine
            var res1 = commands[0].Execute(10);
            var res2 = commands[1].Execute(10);
            Assert.AreEqual(50, res1 + res2);
        }

        [Test]
        public void Resolve_single_works_for_intercepting_decorators_similar_to_CastleDynamicProxy_based_one()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            SampleCommand.Created = false;

            // set up the "intercepting" decorator
            container.Register(typeof(LazyInterceptor<>), setup: Setup.Wrapper);
            container.Register<ICommand, CastleGeneratedProxy>(
                setup: Setup.Decorator,
                made: Made.Of(parameters: Parameters.Of.Type<Interceptor[]>(typeof(LazyInterceptor<ICommand>[]))));

            // everything resolves and executes fine as single value
            var command = container.Resolve<ICommand>();
            Assert.IsInstanceOf<CastleGeneratedProxy>(command);
            Assert.IsFalse(SampleCommand.Created);
            var res = command.Execute(1);
            Assert.AreEqual(2, res);
            Assert.IsTrue(SampleCommand.Created);

            // also, it resolves and executes fine as array
            var commands = container.Resolve<ICommand[]>();
            Assert.AreEqual(1, commands.Length);
            Assert.IsInstanceOf<CastleGeneratedProxy>(commands[0]);
            res = commands[0].Execute(2);
            Assert.AreEqual(4, res);

            // also, it resolves as many
            commands = container.ResolveMany<ICommand>().ToArray();
            Assert.AreEqual(1, commands.Length);
            Assert.IsInstanceOf<CastleGeneratedProxy>(commands[0]);
            res = commands[0].Execute(3);
            Assert.AreEqual(6, res);
        }

        [Test]
        public void Resolve_array_works_for_intercepting_decorators_similar_to_CastleDynamicProxy_based_one()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            SampleCommand.Created = false;
            AnotherCommand.Created = false;

            // set up the "intercepting" decorator
            container.Register(typeof(LazyInterceptor<>), setup: Setup.Wrapper);
            container.Register<ICommand, CastleGeneratedProxy>(
                setup: Setup.Decorator,
                made: Made.Of(parameters: Parameters.Of.Type<Interceptor[]>(typeof(LazyInterceptor<ICommand>[]))));

            // everything resolves fine
            var commands = container.Resolve<ICommand[]>();
            Assert.AreEqual(2, commands.Length);
            Assert.IsInstanceOf<CastleGeneratedProxy>(commands[0]);
            Assert.IsInstanceOf<CastleGeneratedProxy>(commands[1]);
            Assert.IsFalse(SampleCommand.Created);
            Assert.IsFalse(AnotherCommand.Created);

            // and executes as well
            var res1 = commands[0].Execute(10);
            var res2 = commands[1].Execute(10);
            Assert.AreEqual(50, res1 + res2);
            Assert.IsTrue(SampleCommand.Created);
            Assert.IsTrue(AnotherCommand.Created);
        }

        [Test]
        public void Resolve_array_works_with_lazy_proxy()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            container.ResolveAsLazy<ICommand>();
            SampleCommand.Created = false;
            AnotherCommand.Created = false;

            // everything resolves fine
            var commands = container.Resolve<ICommand[]>();
            Assert.AreEqual(2, commands.Length);
            Assert.IsNotNull(commands[0]);
            Assert.IsNotNull(commands[1]);
            Assert.IsNotInstanceOf<SampleCommand>(commands[0]);
            Assert.IsNotInstanceOf<SampleCommand>(commands[1]);
            Assert.IsNotInstanceOf<AnotherCommand>(commands[0]);
            Assert.IsNotInstanceOf<AnotherCommand>(commands[1]);
            Assert.IsFalse(SampleCommand.Created);
            Assert.IsFalse(AnotherCommand.Created);

            // and executes as well
            var res1 = commands[0].Execute(10);
            var res2 = commands[1].Execute(10);
            Assert.AreEqual(50, res1 + res2);
            Assert.IsTrue(SampleCommand.Created);
            Assert.IsTrue(AnotherCommand.Created);
        }

        [Test]
        public void ResolveMany_works_with_lazy_proxy()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            container.ResolveAsLazy<ICommand>();
            SampleCommand.Created = false;
            AnotherCommand.Created = false;

            // everything resolves fine
            var commands = container.ResolveMany<ICommand>().ToArray();
            Assert.AreEqual(2, commands.Length);
            Assert.IsNotNull(commands[0]);
            Assert.IsNotNull(commands[1]);
            Assert.IsNotInstanceOf<SampleCommand>(commands[0]);
            Assert.IsNotInstanceOf<SampleCommand>(commands[1]);
            Assert.IsNotInstanceOf<AnotherCommand>(commands[0]);
            Assert.IsNotInstanceOf<AnotherCommand>(commands[1]);
            Assert.IsFalse(SampleCommand.Created);
            Assert.IsFalse(AnotherCommand.Created);

            // and executes as well
            var res1 = commands[0].Execute(10);
            var res2 = commands[1].Execute(10);
            Assert.AreEqual(50, res1 + res2);
            Assert.IsTrue(SampleCommand.Created);
            Assert.IsTrue(AnotherCommand.Created);
        }
    }

    public interface IAlwaysLazy
    {
        void Test(string x);
    }

    class LazyService : IAlwaysLazy
    {
        public LazyService() { LastValue = "LazyServiceCreated"; }
        public static string LastValue { get; set; }
        public void Test(string x) { LastValue = x; }
    }

    public interface IChicken { IEgg Egg { get; } }

    public interface IEgg { IChicken Chicken { get; } }

    public class Chicken : IChicken
    {
        public Chicken(IEgg egg) { Egg = egg; }
        public IEgg Egg { get; private set; }
    }

    public class Egg : IEgg
    {
        public Egg(IChicken chicken) { Chicken = chicken; }
        public IChicken Chicken { get; private set; }
    }

    public interface ICommand { int Execute(int param); }

    public class SampleCommand : ICommand
    {
        public static bool Created { get; set; }
        public SampleCommand() { Created = true; }
        public int Execute(int param) { return param * 2; }
    }

    public class AnotherCommand : ICommand
    {
        public static bool Created { get; set; }
        public AnotherCommand() { Created = true; }
        public int Execute(int param) { return param * 3; }
    }

    public class TrivialDecorator : ICommand
    {
        public TrivialDecorator(Lazy<ICommand> lazyCommand) { LazyCommand = lazyCommand; }
        private Lazy<ICommand> LazyCommand { get; }
        public int Execute(int param) { return LazyCommand.Value.Execute(param); }
    }

    public class CastleGeneratedProxy : ICommand
    {
        public CastleGeneratedProxy(Interceptor[] interceptors) { Interceptors = interceptors; }
        private Interceptor[] Interceptors { get; }
        public int Execute(int param) { return ((ICommand)Interceptors.First().Target).Execute(param); }
    }

    public interface Interceptor { object Target { get; } }

    public class LazyInterceptor<T> : Interceptor
    {
        public LazyInterceptor(Lazy<T> value) { LazyValue = value; }
        private Lazy<T> LazyValue { get; }
        public object Target { get { return LazyValue.Value; } }
    }
}
