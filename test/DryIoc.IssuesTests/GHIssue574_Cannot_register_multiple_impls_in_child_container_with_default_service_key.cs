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
            // ResolveEnumerableFromChild();
            // ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys();
            ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys_2();
            return 1;
        }

        //[Ignore("fixme")]
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

        // [Test]
        public void ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys()
        {
            var container = new Container(Rules.MicrosoftDependencyInjectionRules)
                .WithMef(); // <-- this is the key, LOL ;-)

            var spf = new DryIocServiceProviderFactory(container);
            var rootContainer = spf.CreateBuilder(new ServiceCollection());
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

            Assert.That(
                childContainer.Resolve<IEnumerable<IPrinter>>().Count(),
                Is.EqualTo(msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count()));

            Assert.That(
                msContainer.GetRequiredService<IEnumerable<IPrinter>>().Count(),
                Is.EqualTo(4));
        }

        [Test]
        public void ResolveEnumerableFromChild_MefAttributedModel_SupportsMultipleServiceKeys_2()
        {
            var container = new Container(Rules.MicrosoftDependencyInjectionRules)
                .WithMef(); // <-- this is the key, LOL ;-)

            // here use RegisterExport instead of the RegisterDescriptor
            container.RegisterExports(
                typeof(Printer),
                typeof(PrinterA),
                typeof(PrinterB),
                typeof(NeighborPrinter)
            );

            // all printers with and without the name, this is the default behavior of the Collection wrapper
            var ps = container.Resolve<IPrinter[]>();  
            CollectionAssert.AreEquivalent(
                new[] { typeof(Printer), typeof(PrinterA), typeof(PrinterB), 
                    typeof(NeighborPrinter) }, 
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
