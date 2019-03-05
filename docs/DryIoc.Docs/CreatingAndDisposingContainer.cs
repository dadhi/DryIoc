/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Creating and Disposing Container

[TOC]

## Creating Container

In most cases all you need is: 
```cs md*/
using System;
using DryIoc;
using NUnit.Framework;

// ReSharper disable UnusedVariable
class Creating_container
{
    [Test] public void Example()
    {
        var container = new Container();
        // start using the container..
    }
} /*md
``` 

Actually, the above example is equivalent to:
```cs md*/
class Creating_container_with_explicit_defaults
{
    [Test] public void Example()
    {
        var container = new Container(rules: Rules.Default, scopeContext: null);
        // start using the container..
    }
} /*md
```

As you see, it is possible to additionally specify the `rules` and 
the [`scopeContext`](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes#markdown-header-what-scopecontext-is).


### Rules

Rules will define the container behavior and conventions.
Rules are immutable, you can create the new rules from the old ones via `With..` methods and 
get the rules without some setting via the `Without..` methods.

```cs md*/
class Adding_some_rules
{
    [Test] public void Example()
    {
        var container1 = new Container(
            Rules.Default.With(FactoryMethod.ConstructorWithResolvableArguments));

        var container2 = new Container(
            Rules.Default.WithoutThrowIfDependencyHasShorterReuseLifespan());
    }
} /*md
```

There is an alternate way to specify the rules with existing container rules as a starting point (without explicit mention of `Rules.Default`):
```cs md*/
class Adding_some_rules_with_action
{
    [Test]
    public void Example()
    {
        var container1 = new Container(rules => rules.WithDefaultReuse(Reuse.Singleton));

        var container2 = new Container(rules => rules.WithAutoConcreteTypeResolution());
    }
} /*md
```

### ScopeContext

ScopeContext is an __optional__ ambient context for the new open scopes to be bound to. 
It is described in details [here](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes#markdown-header-scopecontext).


## Disposing Container

`Container` class implements `IDisposable` interface and should be disposed when no longer required.
Disposing container will:

- Dispose resolved Singletons.
- Remove all registrations.
- Set Rules to `Rules.Empty`.

Usage of disposed Container will end-up with exception.

```cs md*/
class Disposing_container
{
    public class MyService : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    [Test]
    public void Example()
    {
        MyService myService;
        using (var container = new Container())
        {
            container.Register<MyService>(Reuse.Singleton); // MyService is registered with singleton lifetime
            myService = container.Resolve<MyService>();
        }

        Assert.IsTrue(myService.IsDisposed);
    }
}
/*md
```
md*/
