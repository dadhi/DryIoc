using System;
using System.Linq;
using System.Web.Http.Controllers;
using DryIoc;
using Mega;

namespace LoadTest
{
    public static class TestHelper
    {
        public static Type[] GetAllControllers()
        {
            var typesToResolve = typeof(Program).Assembly.GetLoadedTypes()
                .Where((t) =>
                    !t.IsAbstract && !t.IsInterface && !t.Name.Contains("Base") &&
                    typeof(IHttpController).IsAssignableFrom(t))
                .ToList();

            typesToResolve.Add(typeof(IMegaClass));

            return typesToResolve.ToArray();
        }
    }
}
