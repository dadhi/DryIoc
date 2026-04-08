using DryIoc;

// This is a simple end-to-end test for the DryIoc.dll NuGet package CompileTime DI install experience.
//
// After `dotnet build`, the DryIoc.dll.targets file automatically copies T4 templates and example files
// to this project's CompileTimeDI\ folder. The project compiles and runs successfully.
//
// To use compile-time DI:
// 1. Edit CompileTimeDI\CompileTimeRegistrations.ttinclude with your service registrations
// 2. Build in Debug mode to trigger T4 generation of Container.Generated.cs
// 3. Reference CompileTimeContainer in your code (with runtime fallback as shown below)

Console.WriteLine("[DryIoc.CompileTimeDI.Tests] Testing basic DryIoc runtime container...");

// Test that DryIoc runtime container works as expected
var container = new Container();
container.Register<IMyTestService, MyTestService>();

var service = container.Resolve<IMyTestService>();
Console.WriteLine($"[DryIoc.CompileTimeDI.Tests] Resolved: {service.GetType().Name} -> OK");
Console.WriteLine("[DryIoc.CompileTimeDI.Tests] All tests passed!");

// Simple service definitions for testing
interface IMyTestService { }
class MyTestService : IMyTestService { }
