# Thread Safety

[TOC]

## DryIoc is Thread-Safe

DryIoc ensures that:

- Registrations and resolutions could be invoked concurrently on the same container (possibly from a different threads) without corrupting container state.
- Registrations do not stop-the-world for resolutions. __There is no locking associated with registrations__.
- Resolutions do not block registrations either. __No locking except for reused instances as explained below__.
- Registration won't be visible for resolutions until it is completed. If it is incompleted (due exception) all its changes will be ignored.

The above guaranties are possible because of Container structure design. In pseudo code and simplifying things the DryIoc Container may be represented as following:

    class Ref<T> { T Value; } // represents CAS (compare-and-swap) box for the referenced value

    class Container 
    { 
        Ref<Registry> registry; 
    }

    class Registry 
    {
        ImmutableMap registrations;
        Ref<ResolutionCache> resolutionCache;
    }

    class ResolutionCache 
    {
        ImmutableMap cache;
    }

Given the structure Registration of new service will produce new `registry`, then will swap old `registry` with new one inside the container. If some concurrent registration will intervene then the Registration will be retried with new `registry`.

Resolution on the other hand will produce new `resolutionCache` and will swap new cache inside the same registry object. This ensures that resolution cache is bound to the specific registry, and does no affected by new registrations (which produce new registries). 

__Note:__ In addition to providing thread-safety the described Container structure allows to produce new Container very fast, at O(1) cost. This makes creation of ["Child" containers and Open Scope](KindsOfChildContainer) a cheap operation.


## Locking is only used for reused service creation

Lock is required to ensure that creation of reused service happens only once. For instance for singleton service A:

    public class A 
    {
        public static int InstanceCount;

        public A() { ++InstanceCount; }
    }

    container.Register<A>(Reuse.Singleton);

    //thread1:
    container.Resolve<A>();

    //thread2:
    container.Resolve<A>();

    // A.InstanceCount == 1

The lock boundaries are minimized to cover service creation only - the rest of resolution bits are handled outside of lock. 