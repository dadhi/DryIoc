# Extensions and Companions

## Extensions in this repository

### [MEF](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx)

- DryIoc.MefAttributedModel.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.MefAttributedModel.dll)](https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll)
- DryIoc.MefAttributedModel (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.MefAttributedModel)](https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll)

### ASP .NET Core

- DryIoc.Microsoft.DependencyInjection [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Microsoft.DependencyInjection)](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection)
- DryIoc.Microsoft.DependencyInjection.src (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Microsoft.DependencyInjection.src)](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection.src)
- DryIoc.Microsoft.DependencyInjection.AspNetCore2_1 (targeting ASP .NET Core 2.1.1) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Microsoft.DependencyInjection.AspNetCore2_1)](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection.AspNetCore2_1)


### ASP .NET

- DryIoc.Web.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Web.dll)](https://www.nuget.org/packages/DryIoc.Web.dll)
- DryIoc.Web (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Web)](https://www.nuget.org/packages/DryIoc.Web)
- DryIoc.Mvc.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Mvc.dll)](https://www.nuget.org/packages/DryIoc.Mvc.dll)
- DryIoc.Mvc (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Mvc)](https://www.nuget.org/packages/DryIoc.Mvc)
- DryIoc.WepApi.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.WebApi.dll)](https://www.nuget.org/packages/DryIoc.WebApi.dll)
- DryIoc.WepApi (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.WebApi.dll)](https://www.nuget.org/packages/DryIoc.WebApi)
- DryIoc.SignalR.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.SignalR.dll)](https://www.nuget.org/packages/DryIoc.SignalR.dll)
- DryIoc.SignalR (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.SignalR)](https://www.nuget.org/packages/DryIoc.SignalR)
- DryIoc.Owin.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Owin.dll)](https://www.nuget.org/packages/DryIoc.Owin.dll)
- DryIoc.Owin (source code)[![NuGet Badge](https://buildstats.info/nuget/DryIoc.Owin)](https://www.nuget.org/packages/DryIoc.Owin)
- DryIoc.WebApi.Owin.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.WebApi.Owin.dll)](https://www.nuget.org/packages/DryIoc.WebApi.Owin.dll)
- DryIoc.WebApi.Owin (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.WebApi.Owin)](https://www.nuget.org/packages/DryIoc.WebApi.Owin)


### Other

- DryIoc.CommonServiceLocator.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.CommonServiceLocator.dll)](https://www.nuget.org/packages/DryIoc.CommonServiceLocator.dll)
- DryIoc.CommonServiceLocator (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.CommonServiceLocator)](https://www.nuget.org/packages/DryIoc.CommonServiceLocator)
- DryIoc.Syntax.Autofac.dll [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Syntax.Autofac.dll)](https://www.nuget.org/packages/DryIoc.Syntax.Autofac.dll)



## Companions in this repository

Companion packages do not depend on DryIoc package.

### DryIocAttributes

- DryIocAttributes.dll [![NuGet Badge](https://buildstats.info/nuget/DryIocAttributes.dll)](https://www.nuget.org/packages/DryIocAttributes.dll)  
- DryIocAttributes (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIocAttributes)](https://www.nuget.org/packages/DryIocAttributes)

Extends [MEF](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) attributes to cover DryIoc features: metadata, advanced reuses, context based registration, decorators, etc. 


### DryIocZero

DryIocZero [![NuGet Badge](https://buildstats.info/nuget/DryIocZero)](https://www.nuget.org/packages/DryIocZero)

Slim IoC Container based on service factory delegates __generated at compile-time__ by DryIoc.

- __Does not depend on DryIoc at run-time.__
- Ensures _zero_ application bootstrapping time associated with IoC registrations.
- Provides verification of DryIoc registration setup at compile-time by generating service factory delegates. Basically you can see how DryIoc is creating things.
- Supports everything registered in DryIoc: reuses, decorators, wrappers, etc.
- Much smaller and simpler than DryIoc itself. Works standalone without any run-time dependencies.
- Allows run-time registrations too. You may register instances and delegates at run-time.
