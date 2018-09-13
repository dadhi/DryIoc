/*cs
Contains classes and interfaces used in examples
 */

public interface IService { }

public class Service : IService { }

public class TestService : IService { }

public class Client
{
    public IService Service { get; }

    public Client(IService service)
    {
        Service = service;
    }
}
