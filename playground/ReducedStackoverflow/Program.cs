using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using DryIoc;
using DryIoc.WebApi;
using Logic;
using Web.Rest.API;

namespace LoadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpConfiguration();

            /*
             * Following container setup will cause process to fail to Stack Overflow exception
             *
             * Make Release build and run .exe file from command line,
             * if it does not fail you can increase threadCount and iterations  integers to increase pressure
             *
             * Reproduces https://github.com/dadhi/DryIoc/issues/139
             */
            var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments)).WithWebApi(config);

            Registrations.RegisterTypes(container, true);

            Console.WriteLine("-- Run Stack overflow test --");

            for (int i = 0; i < 10; i++)
            {
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                {
                    scope.Resolve(typeof(EmailController));
                }
            }

            Console.WriteLine("-- Finished --");
        }
    }
}