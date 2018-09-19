<!--Auto-generated from .cs file, the edits here will be lost! -->

# Interception

[TOC]

## Decorator with Castle DynamicProxy

Decorator pattern is a good fit for implementing [cross-cutting concerns](https://en.wikipedia.org/wiki/Cross-cutting_concern).

But we can extend it further to implement [AOP Interception](https://en.wikipedia.org/wiki/Aspect-oriented_programming) 
with help of [Castle DynamicProxy](http://www.castleproject.org/projects/dynamicproxy/).

Let's define an extension method for intercepting interfaces and classes:

```cs 

using DryIoc;
using ImTools;
using Castle.DynamicProxy;
using NUnit.Framework;

using System;
using System.Collections.Generic;

public static class DryIocInterception
{
    static readonly DefaultProxyBuilder _proxyBuilder = new DefaultProxyBuilder();

    public static void Intercept<TService, TInterceptor>(this IRegistrator registrator, object serviceKey = null) 
        where TInterceptor : class, IInterceptor
    {
        var serviceType = typeof(TService);

        Type proxyType;
        if (serviceType.IsInterface())
            proxyType = _proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(
                serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
        else if (serviceType.IsClass())
            proxyType = _proxyBuilder.CreateClassProxyType(
                serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
        else
            throw new ArgumentException(
                $"Intercepted service type {serviceType} is not a supported, cause it is nor a class nor an interface");

        var decoratorSetup = serviceKey == null
            ? Setup.DecoratorWith(useDecorateeReuse: true)
            : Setup.DecoratorWith(r => serviceKey.Equals(r.ServiceKey), useDecorateeReuse: true);

        registrator.Register(serviceType, proxyType,
            made: Made.Of(pType => pType.PublicConstructors().FindFirst(ctor => ctor.GetParameters().Length != 0),
                Parameters.Of.Type<IInterceptor[]>(typeof(TInterceptor[]))),
            setup: decoratorSetup);
    }
} 
```

Now define a method interceptor:
```cs 
public class FooLoggingInterceptor : IInterceptor
{
    public List<string> LogLines = new List<string>();
    private void Log(string line) => LogLines.Add(line);

    public void Intercept(IInvocation invocation)
    {
        Log($"Invoking method: {invocation.GetConcreteMethod().Name}");
        invocation.Proceed();
    }
}
```

Register service and its interceptor as a normal services, then link them together via Intercept method:
```cs 

public class Register_and_use_interceptor
{
    public interface IFoo
    {
        void Greet();
    }
    public class Foo : IFoo
    {
        public void Greet() { }
    }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<IFoo, Foo>();
        container.Register<FooLoggingInterceptor>(Reuse.Singleton);
        container.Intercept<IFoo, FooLoggingInterceptor>();

        var foo = container.Resolve<IFoo>();
        foo.Greet();

        // examine that logging indeed was hooked up
        var logger = container.Resolve<FooLoggingInterceptor>();
        Assert.AreEqual("Invoking method: Greet", logger.LogLines[0]);
    }
}
```
