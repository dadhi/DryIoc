using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue486_CustomDynamicRegistrationProvider
    {
        [Test]
        public void Test()
        {
            // prepare registrations for the GetDynamicRegistrations method
            RegisterCommands();
            Assert.AreEqual(1, DynamicRegistrations.Count()); // one interface: ICommand
            Assert.AreEqual(2, DynamicRegistrations.First().Value.Count()); // two implementations: Command1, Command2

            // make sure that dynamic registration provider works as expected
            var registrations = GetDynamicRegistrations(typeof(ICommand), serviceKey: null);
            Assert.AreEqual(2, registrations.Count());

            // attach the dynamic registration provider and try resolving the services
            var container = new Container().WithMef()
                .With(r => r.WithDynamicRegistrations(GetDynamicRegistrations));

            // resolve the commands lazily
            var commands = container.Resolve<Lazy<ICommand, IScriptMetadata>[]>();
            Assert.NotNull(commands);
            Assert.AreEqual(2, commands.Length);

            // these tests work fine:
            Assert.NotNull(commands[0]);
            Assert.NotNull(commands[1]);
            Assert.NotNull(commands[0].Metadata);
            Assert.NotNull(commands[1].Metadata);
            Assert.AreEqual(3, commands[0].Metadata.ScriptID + commands[1].Metadata.ScriptID); // should be 1 and 2, in any order

            // and instantiation also works fine:
            Assert.IsNotNull(commands[0].Value);
        }

        // index: ServiceTypeFullName -> list of ServiceRegistrations
        private ConcurrentDictionary<string, List<DynamicRegistration>> DynamicRegistrations { get; } =
            new ConcurrentDictionary<string, List<DynamicRegistration>>();

        private void RegisterCommands()
        {
            // index only registrations related to this issue
            var lazyRegistrations = AttributedModel.Scan(new[] { typeof(Command1).Assembly })
                .MakeLazyAndEnsureUniqueServiceKeys()
                .Where(r => r.ImplementationTypeFullName.IndexOf("Issue486") >= 0)
                .ToArray();

            // Command1 and Command2
            Assert.AreEqual(2, lazyRegistrations.Length);

            var typeProvider = new Func<string, Type>(t => typeof(Command1).Assembly.GetType(t));

            // index export registrations by exported service type
            foreach (var reg in lazyRegistrations)
            {
                foreach (var export in reg.Exports)
                {
                    var regs = DynamicRegistrations.GetOrAdd(export.ServiceTypeFullName, _ => new List<DynamicRegistration>());
                    regs.Add(new DynamicRegistration(reg.CreateFactory(typeProvider), serviceKey: export.ServiceKey));
                }
            }
        }

        private IEnumerable<DynamicRegistration> GetDynamicRegistrations(Type serviceType, object serviceKey)
        {
            List<DynamicRegistration> regs;
            if (!DynamicRegistrations.TryGetValue(serviceType.FullName, out regs))
            {
                return null;
            }

            // NOTE: 
            // 1. You may rely on DryIoc to find the dynamic registration by key. 
            // Therefore the below code can be removed and replaced by `return regs;`
            // 
            // 2. You should know that to for default (null) keys the `DefaultDynamicKey` will be generated. 
            // So the `serviceKey` parameter will be generated `DefaultDynamicKey` instead of `null`. 
            // The thing is `DefaultDynamicKey.Equals(object obj)` will return true for null `obj`. 
            // BUT the `Object.Equals(object a, object b)` will fail, because it explicitly check for null and returns false.

            var registrations = regs.Where(reg => serviceKey == null || serviceKey.Equals(reg.ServiceKey)).ToArray();
            if (!registrations.Any())
            {
                return null;
            }

            return registrations.ToArray();

        }

        public interface IScriptMetadata
        {
            long ScriptID { get; }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false), MetadataAttribute]
        public class ScriptAttribute : Attribute, IScriptMetadata
        {
            public ScriptAttribute(long id) { ScriptID = id; }
            public long ScriptID { get; private set; }
        }

        public interface ICommand
        {
        }

        [Export(typeof(ICommand)), Script(1)]
        public class Command1 : ICommand
        {
        }

        [Export(typeof(ICommand)), Script(2)]
        public class Command2 : ICommand
        {
        }
    }
}
