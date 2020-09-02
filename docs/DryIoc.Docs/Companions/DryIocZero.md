# DryIocZero

- [DryIocZero](#dryioczero)
  - [At Glance](#at-glance)
  - [How to use](#how-to-use)

## At Glance

NuGet: `PM> Install-Package DryIocZero`

Standalone container based on compile-time generated factories by DryIoc. 

- __Works standalone without any run-time dependencies.__
- Ensures _zero_ application bootstrapping time associated with IoC registrations.
- Provides verification of DryIoc registrations at compile-time by generating service factory delegates. 
Basically you can see how DryIoc is creating things.
- Supports everything registered in DryIoc: reuses, decorators, wrappers, etc.
- Much smaller and simpler than DryIoc itself. 
- Additionally supports run-time registrations: you may register instances and factories at run-time.

## How to use

Here I will speak about v3.

1. Install nuget package [DryIocZero](https://www.nuget.org/packages/DryIocZero/3.0.0) into the project wich plays role of Composition Root of your application. The project to make registrations into IoC container.

2. DIZero is intalled not as dll, but as source files and tt ([T4 Text Template](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates)) files.

- _Container.cs_, _ImTools.cs_, _Container.Generated.tt_ are not supposed to be edited by user, and contain functionality of container, and template for registrations.
- _Registrations.ttinclude_ on the other hand is the place where you put your registrations.
- _Container.Generated.cs_ will contain generated  methods to resolve your registrations. When package is installed, the file will be generated for the first time. It will contain empty methods, because no registrations in _Registrations.ttinclude_ yet.

3. Pit your registrations into _Registrations.ttinclude_ file. The registrations are just a normal DryIoc `Register..` methods. 

- Here is [example](https://bitbucket.org/dadhi/dryioc/src/892bea22352d8a62bd287b00447f0399fbe10fb4/Net45/DryIocZero.UnitTests/DryIocZero/Registrations.ttinclude?at=default&fileviewer=file-view-default) with DryIoc.MefAttributeModel

4. You may use type-based `Register`, `RegisterMany` and `RegisterPlaceholder`, but not `RegisterDelagate` and `UseInstance`, cause latter are operate with run-time state.