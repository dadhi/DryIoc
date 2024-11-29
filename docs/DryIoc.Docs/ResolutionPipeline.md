# Resolution Pipeline

- [Resolution Pipeline](#resolution-pipeline)
  - [Overview](#overview)
  - [Relative Performance](#relative-performance)
  - [Rules.WithUseInterpretation](#ruleswithuseinterpretation)
  - [Rules.WithoutUseInterpretationForFirstResolution](#ruleswithoutuseinterpretationforfirstresolution)


## Overview

What happens when you call the `container.Resolve<X>()` for the service `X` multiple times in a sequence assuming that the `X` is not a singleton*:

1. The first call will **discover and construct the expression tree** of `X` object graph, will **interpret** the expression to get the service, and if succeeded will **cache the expression** in the resolution cache, and will return the interpreted service.
2. The second call will find the cached expression, will **compile** it to the delegate, then it will **replace the cached expression with the delegate** and will invoke the delegate to get the service.
3. The third call will find the cached delegate and invoke it.

singleton* - those are always interpreted (this cannot be changed via rules) and injected in the object graph as a `ConstantExpression` unless wrapped in `Func` or `Lazy` wrappers (which create the singleton on demand). 
The reason for that is the singletons are created once and the one-time interpretation is faster than the compilation+invocation.


## Relative Performance

- Delegate invocation is the fastest
- Expression interpretation is 10x times slower than the delegate invocation
- Delegate compilation is 100x slower than the interpretation.

That's mean you need **once** to spend 100x more time than interpretation to get 10x boost, 
and DryIoc **by default **is not paying this price for the first resolution (assuming that you may not even require the second resolution, and if the second one happens, that's fine, let's pay the price).


## Rules.WithUseInterpretation

The compilation (essentially `System.Reflection.Emit`) is not supported by all targets, e.g. by the Xamarin iOS. In this case you may specify to always use the interpretation via the rule: 

```cs
var c = new Container(rules => rules.WithUseInterpretation());
```

DryIoc uses its own interpretation mechanism which is faster than `System.Linq.Expressions.Expression.Compile(preferInterpretation: true)` because DryIoc can recognize its own internal methods in the resolved expression tree, and call them directly without reflection. It has other optimizations as well.

## Rules.WithoutUseInterpretationForFirstResolution

On the contrary, if you want to Compile on the first resolution, maybe to "warm-up" the container before the actual usage you can do that via 

```cs
var c = new Container(rules => rules.WithoutUseInterpretationForFirstResolution());
```
