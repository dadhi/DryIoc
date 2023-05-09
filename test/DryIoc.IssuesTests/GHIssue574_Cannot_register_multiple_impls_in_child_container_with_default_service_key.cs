
public class DryExamples
{
    [Fact] // This works ok.
    public void TransferMultipleThenResolveEnumerable()
    {
        var services = new ServiceCollection();

        services.AddScoped<IPrinter, Printer>();
        services.AddScoped<IPrinter, PrinterA>();
        services.AddScoped<IPrinter, PrinterB>();
        services.AddScoped<IPrinter, NeighborPrinter>();

        var spf = new DryIocServiceProviderFactory();
        var dryContainer = spf.CreateBuilder(services);
        var msContainer = dryContainer.GetServiceProvider();

        Assert.Equal(
            dryContainer.Resolve<IEnumerable<IPrinter>>().Count(),
            msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count());
    }

    [Fact] // I have not been able to get this to work.
    public void TransferMultipleThenResolveEnumerableFromChild()
    {
        var services = new ServiceCollection();

        services.AddScoped<IPrinter, Printer>();
        services.AddScoped<IPrinter, PrinterA>();
        services.AddScoped<IPrinter, PrinterB>();
        services.AddScoped<IPrinter, NeighborPrinter>();

        var spf = new DryIocServiceProviderFactory();
        var rootContainer = spf.CreateBuilder(new ServiceCollection());
        var childContainer = rootContainer.CreateChild(RegistrySharing.Share, "child-stamp", IfAlreadyRegistered.AppendNewImplementation);
        //childContainer.Populate(services);
        foreach (var service in services)
        {
            childContainer.RegisterDescriptor(service, IfAlreadyRegistered.AppendNewImplementation, "child-stamp");
        }
        var msContainer = childContainer.GetServiceProvider();

        Assert.Equal(
            childContainer.Resolve<IEnumerable<IPrinter>>().Count(),
            msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count());
    }

    private interface IPrinter{}

    private class Printer : IPrinter{}

    private class PrinterA : IPrinter{}

    private class PrinterB : IPrinter{}

    private class NeighborPrinter : IPrinter{}
}
