using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using DryIoc.MefAttributedModel;
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
        public void Interface_can_be_resolved_as_always_lazy_via_ProxyGenerator()
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
        public void Interface_can_be_resolved_as_always_lazy_via_DefaultProxyBuilder()
        {
            var c = new Container();
            c.ResolveAsLazyViaProxyBuilder(typeof(IAlwaysLazy));
            c.Register<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Interface_not_available_at_compile_time_can_be_resolved_as_always_lazy_via_ProxyGenerator()
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
        public void Interface_not_available_at_compile_time_can_be_resolved_as_always_lazy_via_DefaultProxyBuilder()
        {
            var c = new Container();
            var typeLoadedFromExternalAssembly = Assembly.GetExecutingAssembly().GetType("DryIoc.IssuesTests.Interception.IAlwaysLazy");
            c.ResolveAsLazyViaProxyBuilder(typeLoadedFromExternalAssembly);

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

        [Test]
        public void Circular_dependency_handling_doesnt_actually_work_for_the_deeper_levels()
        {
            var container = new Container();
            container.RegisterAsLazy<IChicken, Chicken>();
            container.RegisterAsLazy<IEgg, Egg>();

            var e = container.Resolve<IEgg>();
            Assert.NotNull(e);
            Assert.NotNull(e.Chicken);
            Assert.NotNull(e.Chicken.Egg);
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
        public void Resolve_array_works_with_lazy_proxy_via_ProxyGenerator()
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
        public void Resolve_array_works_with_lazy_proxy_via_DefaultProxyBuilder()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            container.ResolveAsLazyViaProxyBuilder(typeof(ICommand));
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
        public void ResolveMany_works_with_lazy_proxy_via_ProxyGenerator()
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

        [Test]
        public void ResolveMany_works_with_lazy_proxy_via_DefaultProxyBuilder()
        {
            var container = new Container();
            container.Register<ICommand, SampleCommand>();
            container.Register<ICommand, AnotherCommand>();
            container.ResolveAsLazyViaProxyBuilder(typeof(ICommand));
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

        [Test]
        public void Resolve_array_works_for_command_with_dependency_lazy_proxy_via_ProxyGenerator()
        {
            var container = new Container();
            container.Register<ICommand, CommandWithDependency>();
            container.Register<ICommandDependency, CommandDependency>();
            container.ResolveAsLazy<ICommandDependency>();

            // everything resolves fine
            var commands = container.Resolve<ICommand[]>();
            Assert.AreEqual(1, commands.Length);
            Assert.IsNotNull(commands[0]);
            Assert.IsInstanceOf<CommandWithDependency>(commands[0]);
            Assert.IsNotInstanceOf<CommandDependency>((commands[0] as CommandWithDependency).Dep);

            // and executes as well
            var res = commands[0].Execute(10);
            Assert.AreEqual(13, res);
        }

        [Test]
        public void Resolve_array_works_with_metadata()
        {
            var container = new Container().WithMef();
            container.Register<IBusinessLogicHelper, Helper1>(setup: Setup.With(new BusinessLogicAttribute(1)));
            container.Register<IBusinessLogicHelper, Helper2>(setup: Setup.With(new BusinessLogicAttribute(2)));
            container.Register<IBusinessLogicHelper, Helper3>(setup: Setup.With(new BusinessLogicAttribute(3)));

            var helpers = container.Resolve<Meta<IBusinessLogicHelper, IBusinessLogicForZone>[]>();

            Assert.AreEqual(3, helpers.Length);
            var h1 = helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
        }

        [Test, Ignore("Fails")]
        public void Resolve_array_works_with_metadata_and_trivial_decorator()
        {
            var container = new Container().WithMef();
            container.Register<IBusinessLogicHelper, Helper1>(setup: Setup.With(new BusinessLogicAttribute(1)));
            container.Register<IBusinessLogicHelper, Helper2>(setup: Setup.With(new BusinessLogicAttribute(2)));
            container.Register<IBusinessLogicHelper, Helper3>(setup: Setup.With(new BusinessLogicAttribute(3)));
            container.Register<IBusinessLogicHelper, TrivialBusinessLogicDecorator>(setup: Setup.Decorator);

            var helpers = container.Resolve<Meta<IBusinessLogicHelper, IBusinessLogicForZone>[]>();

            Assert.AreEqual(3, helpers.Length);
            var h1 = helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
        }

        [Test, Ignore("Fails")]
        public void Resolve_array_with_metadata_works_with_LazyProxy()
        {
            var container = new Container().WithMef();
            container.Register<IBusinessLogicHelper, Helper1>(setup: Setup.With(new BusinessLogicAttribute(1)));
            container.Register<IBusinessLogicHelper, Helper2>(setup: Setup.With(new BusinessLogicAttribute(2)));
            container.Register<IBusinessLogicHelper, Helper3>(setup: Setup.With(new BusinessLogicAttribute(3)));
            container.ResolveAsLazy(typeof(IBusinessLogicHelper));

            var helpers = container.Resolve<Lazy<IBusinessLogicHelper, IBusinessLogicForZone>[]>();

            Assert.AreEqual(3, helpers.Length);
            var h1 = helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
        }

        [Test]
        public void ImportMany_with_metadata_works_without_lazy_proxies()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Helper1), typeof(Helper2), typeof(Helper3));

            var consumer = new BusinessLogicConsumer();
            container.InjectPropertiesAndFields(consumer);

            Assert.AreEqual(3, consumer.Helpers.Length);
            var h1 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
        }

        [Test, Ignore("Fails")]
        public void ImportMany_with_metadata_works_with_lazy_proxies_via_ProxyGenerator()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Helper1), typeof(Helper2), typeof(Helper3));
            container.ResolveAsLazy(typeof(IBusinessLogicHelper));

            var consumer = new BusinessLogicConsumer();
            container.InjectPropertiesAndFields(consumer);

            Assert.AreEqual(3, consumer.Helpers.Length);
            var h1 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
        }

        [Test, Ignore("Fails")]
        public void ImportMany_with_metadata_works_with_lazy_proxies_via_DefaultProxyBuilder()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Helper1), typeof(Helper2), typeof(Helper3));
            container.ResolveAsLazyViaProxyBuilder(typeof(IBusinessLogicHelper));

            var consumer = new BusinessLogicConsumer();
            container.InjectPropertiesAndFields(consumer);

            Assert.AreEqual(3, consumer.Helpers.Length);
            var h1 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 1);
            h1.Value.ProcessZone(1);

            var h2 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 2);
            h2.Value.ProcessZone(2);

            var h3 = consumer.Helpers.Single(h => h.Metadata.ZoneId == 3);
            h3.Value.ProcessZone(3);
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

    public interface ICommandDependency { }

    public class CommandDependency : ICommandDependency { }

    public class CommandWithDependency : ICommand
    {
        public CommandWithDependency(ICommandDependency dep) { Dep = dep; }
        public ICommandDependency Dep { get; }
        public int Execute(int param) { return param + 3; }
    }

    public interface IBusinessLogicHelper { void ProcessZone(int a); }

    public interface IBusinessLogicForZone { int ZoneId { get; } }

    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BusinessLogicAttribute : ExportAttribute, IBusinessLogicForZone
    {
        public BusinessLogicAttribute(int zone) : base(typeof(IBusinessLogicHelper)) { ZoneId = zone; }
        public int ZoneId { get; }
    }

    [BusinessLogic(1)]
    public class Helper1 : IBusinessLogicHelper { public void ProcessZone(int a) { Assert.AreEqual(1, a); } }

    [BusinessLogic(2)]
    public class Helper2 : IBusinessLogicHelper { public void ProcessZone(int a) { Assert.AreEqual(2, a); } }

    [BusinessLogic(3)]
    public class Helper3 : IBusinessLogicHelper { public void ProcessZone(int a) { Assert.AreEqual(3, a); } }

    public class BusinessLogicConsumer
    {
        [ImportMany]
        public Lazy<IBusinessLogicHelper, IBusinessLogicForZone>[] Helpers { get; set; }
    }

    public class TrivialBusinessLogicDecorator : IBusinessLogicHelper
    {
        public TrivialBusinessLogicDecorator(Lazy<IBusinessLogicHelper> lazyHelper) { LazyHelper = lazyHelper; }
        private Lazy<IBusinessLogicHelper> LazyHelper { get; }
        public void ProcessZone(int a) { LazyHelper.Value.ProcessZone(a); }
    }
}
