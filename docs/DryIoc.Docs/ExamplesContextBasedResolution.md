<!--Auto-generated from .cs file, the edits here will be lost! -->

# Examples of context based resolution


- [Examples of context based resolution](#examples-of-context-based-resolution)
  - [log4net logger](#log4net-logger)
  - [Serilog logger](#serilog-logger)


## log4net logger

Here is [the discussion](https://github.com/autofac/Autofac/issues/644) and example  [how to inject log4net](https://gist.github.com/piers7/81724d51a7ca158d721e) in Autofac.

In DryIoc we may use [strongly-typed Factory Method](SelectConstructorOrFactoryMethod.md#factory-method-instead-of-constructor) specification to register log4net.ILog:

```cs 
namespace DryIoc.Docs;
using DryIoc;
using NUnit.Framework;
using System;
using log4net;

public class Log4net_logger_example
{
    public class A
    {
        public ILog Logger { get; }
        public A(ILog logger)
        {
            Logger = logger;
        }
    }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<A>();

        container.Register(Made.Of<ILog>(() => 
                LogManager.GetLogger(Arg.Index<string>(0)),
                request => request.Parent.ImplementationType.Name.ToString()));

        var a = container.Resolve<A>();
        Assert.IsNotNull(a.Logger);
    }
} 
```

`Made.Of` `Arg.Index<Type>(0)` argument references to the value: `request => request.Parent.ImplementationType`, 
which evaluates to the `typeof(A)` in the example.


## Serilog logger

The code is similar to the __log4net__ with using dependency parent type as context for instantiating of `ILogger`.
In addition, the condition allows to use default logger where context is not available, e.g. at resolution root.

```cs 

public class Serilog_logger_example
{
    public class LogSubject
    {
        public Serilog.ILogger Logger { get; }
        public LogSubject(Serilog.ILogger logger)
        {
            Logger = logger;
        }
    }

    [Test]
    public void Example()
    {
        var container = new Container();

        // default logger
        container.Register(Made.Of(() => Serilog.Log.Logger),
            setup: Setup.With(condition: r => r.Parent.ImplementationType == null));

        // type dependent logger
        container.Register(
            Made.Of(() => Serilog.Log.ForContext(Arg.Index<Type>(0)), r => r.Parent.ImplementationType),
            setup: Setup.With(condition: r => r.Parent.ImplementationType != null));

        var defaultLogger = container.Resolve<Serilog.ILogger>();
        Assert.AreSame(Serilog.Log.Logger, defaultLogger);

        container.Register<LogSubject>();
        var s = container.Resolve<LogSubject>();

        Assert.AreSame(Serilog.Log.ForContext<LogSubject>(), s.Logger);
    }
} 
```
