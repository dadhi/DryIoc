using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class ResolutionRulesPlayTests
    {
        [Test]
        public void Immutable_ResolutionRules()
        {
            var rules = Ref.Of(ResolutionRules.Empty);
            rules.Swap(r => r.With(r.TryResolveUnregisteredService.AppendOrUpdate((request, registry) => null)));
        }

        [Test]
        public void Test()
        {
            var rules = ResolutionRules.Empty;
            rules.TryResolveUnregisteredService.GetFirstNonDefault(r => r(null, null));
        }
    }

    public sealed class ResolutionRules
    {
        public static readonly ResolutionRules Empty = new ResolutionRules();

        public delegate Factory SelectFactory(Type serviceType, IEnumerable<Factory> factories);
        public SelectFactory SelectSingleRegisteredFactory { get; private set; }
        public ResolutionRules With(SelectFactory selectRegisteredFactory)
        {
            return new ResolutionRules(this) { SelectSingleRegisteredFactory = selectRegisteredFactory };
        }

        public delegate Factory ResolveFactory(Request request, IRegistry registry);
        public ResolveFactory[] TryResolveUnregisteredService { get; private set; }
        public ResolutionRules With(ResolveFactory[] tryResolveUnregisteredService)
        {
            return new ResolutionRules(this) { TryResolveUnregisteredService = tryResolveUnregisteredService };
        }

        public delegate object ResolveParameterServiceKey(ParameterInfo parameter, Request parent, IRegistry registry);
        public ResolveParameterServiceKey[] TryResolveConstructorParameterServiceKey { get; private set; }
        public ResolutionRules With(ResolveParameterServiceKey[] tryResolveConstructorParameterServiceKey)
        {
            return new ResolutionRules(this) { TryResolveConstructorParameterServiceKey = tryResolveConstructorParameterServiceKey };
        }

        public static readonly BindingFlags EligiblePropertyOrField = BindingFlags.Public | BindingFlags.Instance;

        public delegate bool CanResolveMemberWithServiceKey(out object key, MemberInfo member, Request parent, IRegistry registry);
        public CanResolveMemberWithServiceKey[] CanResolvePropertyOrFieldWithServiceKey { get; private set; }
        public ResolutionRules With(CanResolveMemberWithServiceKey[] canResolvePropertyOrFieldWithServiceKey)
        {
            return new ResolutionRules(this) { CanResolvePropertyOrFieldWithServiceKey = canResolvePropertyOrFieldWithServiceKey };
        }

        #region Implementation

        private ResolutionRules() { }

        private ResolutionRules(ResolutionRules rules)
        {
            SelectSingleRegisteredFactory = rules.SelectSingleRegisteredFactory;
            TryResolveUnregisteredService = rules.TryResolveUnregisteredService;
            TryResolveConstructorParameterServiceKey = rules.TryResolveConstructorParameterServiceKey;
            CanResolvePropertyOrFieldWithServiceKey = rules.CanResolvePropertyOrFieldWithServiceKey;
        }

        #endregion
    }
}
