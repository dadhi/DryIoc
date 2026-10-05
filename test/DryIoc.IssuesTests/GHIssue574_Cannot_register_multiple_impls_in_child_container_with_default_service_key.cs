using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;
using DryIoc.MefAttributedModel;
using System.ComponentModel.Composition;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue574_Cannot_register_multiple_impls_in_child_container_with_default_service_key : ITest
    {
        public int Run()
        {
            ResolveEnumerableFromChild();
            ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys();
            ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys_2();
            return 3;
        }

        [Test]
        public void ResolveEnumerableFromChild()
        {
            var services = new ServiceCollection();

            services.AddScoped<IPrinter, Printer>();
            services.AddScoped<IPrinter, PrinterA>();
            services.AddScoped<IPrinter, PrinterB>();
            services.AddScoped<IPrinter, NeighborPrinter>();

            var rootContainer = new ServiceCollection().BuildDryIocServiceProvider().Container;
            var childContainer = rootContainer.CreateChild(RegistrySharing.Share, "child-stamp", IfAlreadyRegistered.AppendNewImplementation);

            foreach (var service in services)
            {
                childContainer.RegisterDescriptor(service, IfAlreadyRegistered.AppendNewImplementation, "child-stamp");
            }

            var msContainer = childContainer.GetServiceProvider();

            Assert.That(
                childContainer.Resolve<IEnumerable<IPrinter>>().Count(),
                Is.EqualTo(msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count()));

            // keyed services are excluded from the IEnumerable, as in MS.DI RC1
            Assert.That(msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count(), Is.EqualTo(0));
            Assert.That(msContainer.GetKeyedServices<IPrinter>("child-stamp").Count(), Is.EqualTo(4));
        }

        [Test]
        public void ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys()
        {
            // now with MS.DI keyed services it works fine
            var container = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);

            var spf = new DryIocServiceProviderFactory(container);
            var rootContainer = spf.CreateBuilder(new ServiceCollection()).Container;
            var childContainer = rootContainer
                .CreateChild(RegistrySharing.Share, "child-stamp", IfAlreadyRegistered.AppendNewImplementation);

            // here use RegisterExport instead of the RegisterDescriptor
            childContainer.RegisterExports(
                typeof(Printer),
                typeof(PrinterA),
                typeof(PrinterB),
                typeof(NeighborPrinter)
            );

            var msContainer = childContainer.GetServiceProvider();
            Assert.That(msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count(), Is.EqualTo(0));
            Assert.That(msContainer.GetKeyedServices<IPrinter>("child-stamp").Count(), Is.EqualTo(4));
        }

        [Test]
        public void ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys_2()
        {
            // now with MS.DI keyed services it works fine
            var container = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);

            // here use RegisterExport instead of the RegisterDescriptor
            container.RegisterExports(
                typeof(Printer),
                typeof(PrinterA),
                typeof(PrinterB),
                typeof(NeighborPrinter)
            );

            // only the printers without the name, keyed ones are excluded as in MS.DI RC1
            var ps = container.Resolve<IPrinter[]>();  
            CollectionAssert.AreEquivalent(
                new[] { typeof(NeighborPrinter) },
                ps.Select(p => p.GetType()));

            // only printers with the specific StampName
            var psStamped = container.Resolve<IPrinter[]>(serviceKey: StampName);
            CollectionAssert.AreEquivalent(
                new[] { typeof(Printer), typeof(PrinterA), typeof(PrinterB) },
                psStamped.Select(p => p.GetType()));
        }

        private const string StampName = "child-stamp";

        private interface IPrinter { }

        [Export(StampName, typeof(IPrinter))]
        private class Printer : IPrinter { }

        [Export(StampName, typeof(IPrinter))]
        private class PrinterA : IPrinter { }

        [Export(StampName, typeof(IPrinter))]
        private class PrinterB : IPrinter { }

        [Export(typeof(IPrinter))] // No name
        private class NeighborPrinter : IPrinter { }
    }
}
