using DryIoc;

// End-to-end smoke test: DryIoc runtime container resolves correctly.
// For compile-time DI setup see CompileTimeDI\CompileTimeRegistrations.ttinclude.
var container = new Container();
container.Register<IMyTestService, MyTestService>();
var service = container.Resolve<IMyTestService>();
Console.WriteLine($"[DryIoc.CompileTimeDI.Tests] Resolved: {service.GetType().Name} -> OK");

interface IMyTestService { }
class MyTestService : IMyTestService { }
