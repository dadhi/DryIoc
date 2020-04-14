using System.Diagnostics;

namespace DryIoc.AspNetCore.Sample.Services
{
    public class SingletonService : ISingletonService { }

    public class TransientService : ITransientService
    {
        public ISingletonService Singleton { get; }

        public TransientService(ISingletonService singleton)
        {
            Singleton = singleton;
        }
    }

    public class ScopedService : IScopedService
    {
        public ISingletonService Singleton { get; }
        public ITransientService Transient { get; }

        public ScopedService(ISingletonService singleton, ITransientService transient)
        {
            Singleton = singleton;
            Transient = transient;

            Debug.Assert(Singleton == transient.Singleton);
        }
    }
}
