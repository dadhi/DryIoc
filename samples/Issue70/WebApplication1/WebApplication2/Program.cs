using System;
using System.Net.Http;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication2
{
    class Program
    {
        static void Main()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddSingleton<ITest, Test>();

            //var provider = services.BuildServiceProvider();
            //provider.GetService();

            var container = new Container().WithDependencyInjectionAdapter(services).BuildServiceProvider();

            var test = container.GetService<ITest>();

            Console.WriteLine(test.HttpClientFactory);
        }
    }

    public interface ITest
    {
        IHttpClientFactory HttpClientFactory { get; }
    }

    public class Test : ITest
    {
        public IHttpClientFactory HttpClientFactory { get; }

        public Test(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }
    }
}
