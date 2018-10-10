namespace DryIoc.AspNetCore.Sample.Services
{
    public interface ISingletonService { }

    public interface IScopedService
    {
        ISingletonService Singleton { get; }
        ITransientService Transient { get; }
    }

    public interface ITransientService
    {
        ISingletonService Singleton { get; }
    }

    public interface IExportedService { }
}
