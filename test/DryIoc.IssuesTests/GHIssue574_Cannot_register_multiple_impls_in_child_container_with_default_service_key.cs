using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests
{
    public class GHIssue574_Cannot_register_multiple_impls_in_child_container_with_default_service_key
    {
        [Ignore("fixme")]
        public void ResolveEnumerableFromChild()
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

            Assert.That(
                childContainer.Resolve<IEnumerable<IPrinter>>().Count(),
                Is.EqualTo(msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count()));

            Assert.That(
                msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count(),
                Is.EqualTo(4));
        }

        private interface IPrinter { }

        private class Printer : IPrinter { }

        private class PrinterA : IPrinter { }

        private class PrinterB : IPrinter { }

        private class NeighborPrinter : IPrinter { }
    }
}
