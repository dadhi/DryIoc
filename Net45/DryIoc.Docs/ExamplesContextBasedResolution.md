# Examples of context based resolution

[TOC]

## log4net logger

Here is [the discussion](https://github.com/autofac/Autofac/issues/644) and example  [how to inject log4net](https://gist.github.com/piers7/81724d51a7ca158d721e) in Autofac.

In DryIoc we may use [strongly-typed Factory Method](https://bitbucket.org/dadhi/dryioc/wiki/SelectConstructorOrFactoryMethod#markdown-header-factory-method-instead-of-constructor) specification to register log4net.ILog:

```
#!c#
	// Test setup:
    public class A	
    {
        public log4net.ILog Log { get; private set; }

        public A(log4net.ILog log)
        {
            Log = log;
        }
    }

    [Test]
    public void Can_register_ILog_with_factory_method()
    {
        var container = new Container();
        container.Register<A>();

        container.Register<log4net.ILog>(Made.Of(
            () => log4net.LogManager.GetLogger(Arg.Index<Type>(0)), 
            request => request.Parent.ImplementationType));

        var a = container.Resolve<A>();
    }
```

`Arg.Index<Type>(0)` references to the argument after factory method: `request => request.Parent.ImplementationType`, which evaluates to typeof(A) in test setup.


## Serilog logger

The code is similar to the __log4net__ with using dependency parent type as context for instantiating `ILogger`.
In addition the condition allows to use default logger where context is not available, e.g. at resolution root.

```
#!c#
    [Test]
    public void Test()
    {
        var c = new Container();

        // default logger
        c.Register(Made.Of(() => Serilog.Log.Logger), 
            setup: Setup.With(condition: r => r.Parent.ImplementationType == null));
        
        // contextual logger
        c.Register(Made.Of(() => Serilog.Log.ForContext(Arg.Index<Type>(0)), r => r.Parent.ImplementationType),
            setup: Setup.With(condition: r => r.Parent.ImplementationType != null));

        c.Resolve<Serilog.ILogger>();

        c.Register<LogSubject>();
        c.Resolve<LogSubject>();
    }

    public class LogSubject
    {
        public ILogger Logger { get; private set; }
        public LogSubject(Serilog.ILogger logger)
        {
            Logger = logger;
        }
    }
```