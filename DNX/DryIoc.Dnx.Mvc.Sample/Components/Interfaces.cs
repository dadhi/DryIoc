namespace Web.Components
{
    public interface IServiceBase { int InstanceId { get; } }
    public interface ISingletonService : IServiceBase { }
    public interface ITransientService : IServiceBase { }
    public interface IPerRequestService : IServiceBase { }
}
