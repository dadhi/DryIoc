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
            var rules = Ref.Of(Rules.Empty);
            rules.Swap(r => r.With(r.ToResolveUnregisteredService.AppendOrUpdate((request, registry) => null)));
        }
    }

    public sealed class Rules
    {
        public static readonly Rules Empty = new Rules();

        public delegate Factory SelectFactory(Type serviceType, IEnumerable<Factory> factories);
        public SelectFactory ToSelectFactory { get; private set; }

        public delegate Factory ResolveFactory(Request request, IRegistry registry);
        public ResolveFactory[] ToResolveUnregisteredService { get; private set; }

        public delegate object ResolveParameterServiceKey(ParameterInfo parameter, Request parent, IRegistry registry);
        public ResolveParameterServiceKey[] ToResolveConstructorParameterKey { get; private set; }

        public delegate bool ResolveMemberServiceKey(out object key, MemberInfo member, Request parent, IRegistry registry);

        public ResolveMemberServiceKey[] ToResolvePropertyOrFieldKey { get; private set; }

        public Rules With(ResolveFactory[] forUnregisteredService)
        {
            return new Rules(this) { ToResolveUnregisteredService = forUnregisteredService };
        }

        private Rules() { }

        private Rules(Rules rules)
        {
            ToSelectFactory = rules.ToSelectFactory;
            ToResolveUnregisteredService = rules.ToResolveUnregisteredService;
            ToResolveConstructorParameterKey = rules.ToResolveConstructorParameterKey;
            ToResolvePropertyOrFieldKey = rules.ToResolvePropertyOrFieldKey;
        }
    }
}
