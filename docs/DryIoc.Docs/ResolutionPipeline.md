# Resolution Pipeline

- [Resolution Pipeline](#resolution-pipeline)
  - [Overview](#overview)
  - [Relative Performance](#relative-performance)
  - [Rules.WithUseInterpretation](#ruleswithuseinterpretation)
  - [Rules.WithoutUseInterpretationForFirstResolution](#ruleswithoutuseinterpretationforfirstresolution)
  - [Rules.WithoutFastExpressionCompiler](#ruleswithoutfastexpressioncompiler)

## Overview

What happens when you call `container.Resolve<X>();` for the same service `X` multiple times in a sequence assuming that the `X` is not a singleton*:

1. The first call will **discover and construct the expression tree** of `X` object graph, **interpret** the expression to get the service, and if succeeded will **cache the expression** in resolution cache.
2. The second call will find cached expression, will **compile** it to delegate, then it will **replace the cached expression with the delegate** and will invoke the delegate to get the service.
3. The third call will find the cached delegate and invoke it.

singleton* - those are always interpreted (cannot be changed via rules) and injected in object graph as `ConstantExpression` unless wrapped in Func or Lazy wrappers (which create singleton when consumer demand). 
This is because they need to be created once and one-time interpretation is faster than compilation+invocation.


## Relative Performance

- Expression interpretation is 10x times slower than the delegate invocation
- Delegate compilation is 100x slower than interpretation.

Now, spot the problem for the multi-threaded resolution of the same service (#208)

## Rules.WithUseInterpretation

The compilation (essentially `System.Reflection.Emit`) is not supported by all targets, e.g. Xamarin iOS. In this case, you may specify to always use interpretation via 

```cs
var c = new Container(rules => rules.WithUseInterpretation());
```

DryIoc uses its own interpretation mechanism which is faster than `System.Linq.Expressions.Expression.Compile(preferInterpretation: true)` because DryIoc can recognize its own internal methods in the resolved expression tree, and call them directly without reflection. It has other optimizations as well.

## Rules.WithoutUseInterpretationForFirstResolution

On the contrary, if you want to Compile on the first resolution, maybe to "warm-up" the container before the actual usage you can do that via 

```cs
var c = new Container(rules => rules.WithoutUseInterpretationForFirstResolution());
```

## Rules.WithoutFastExpressionCompiler

By default, DryIoc relies on [its own Expression Tree compiler](https://github.com/dadhi/FastExpressionCompiler). But it doesn't support certain old platforms, e.g. PCL, <= .NET 4.0, <= .NET Standard 1.2 - so the default `Expression.Compile` is used for them. If you want for some reason to fully switch to `Expression.Compile` you may do so via:

```cs
var c = new Container(rules => rules.WithoutFastExpressionCompiler());
```

But I would not recommend using it, maybe just for the last resort troubleshooting only.
It is very likely that the option will be removed in future DryIoc versions.
