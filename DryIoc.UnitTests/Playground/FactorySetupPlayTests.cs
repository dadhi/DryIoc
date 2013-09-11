using System;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class FactorySetupPlay
    {
        [Test]
        public void How_to_access_properties_not_defining_in_base_class()
        {
            //FactorySetup factory = new FactorySetup.Service();
            FactorySetup factory = new FactorySetup.GenericWrapper(args => args[0]);

            if (factory is FactorySetup.GenericWrapper)
                ((FactorySetup.GenericWrapper)factory).SelectGenericTypeArg(new[] { typeof(int) });
        }
    }

    public abstract class FactorySetup2
    {
        public abstract FactoryType Type { get; }
        public abstract bool SkipCache { get; }
        public abstract Init Init { get; }
        public abstract object Metadata { get; }

        public class Service : FactorySetup2
        {
            public override FactoryType Type { get { return FactoryType.Service; } }

            public override bool SkipCache { get { return _skipCache; } }
            public override object Metadata { get { return _metadata; } }
            public override Init Init { get { return _init; } }

            public Service(bool skipCache = false, Init init = null, object metadata = null)
            {
                _skipCache = skipCache;
                _init = init;
                _metadata = metadata;
            }

            private readonly bool _skipCache;
            private readonly object _metadata;
            private readonly Init _init;
        }

        public class GenericWrapper : FactorySetup2
        {
            public override FactoryType Type { get { return FactoryType.GenericWrapper; } }
            public override bool SkipCache { get { return false; } }
            public override Init Init { get { return null; } }
            public override object Metadata { get { return null; } }

            public readonly SelectGenericTypeArg SelectGenericTypeArg;

            public GenericWrapper(SelectGenericTypeArg selectGenericTypeArg)
            {
                SelectGenericTypeArg = selectGenericTypeArg;
            }
        }
    }

    public abstract class FactorySetup
    {
        public static readonly FactorySetup Default = new Service();

        public abstract FactoryType Type { get; }
        public virtual bool SkipCache { get { return false; } }
        public virtual Init Init { get { return null; } }
        public virtual object Metadata { get { return null; } }

        public class Service : FactorySetup
        {
            public override FactoryType Type { get { return FactoryType.Service; } }

            public override bool SkipCache { get { return _skipCache; } }
            public override object Metadata { get { return _metadata; } }
            public override Init Init { get { return _init; } }

            public Service(bool skipCache = false, Init init = null, object metadata = null)
            {
                _skipCache = skipCache;
                _init = init;
                _metadata = metadata;
            }

            private readonly bool _skipCache;
            private readonly object _metadata;
            private readonly Init _init;
        }

        public class GenericWrapper : FactorySetup
        {
            public override FactoryType Type { get { return FactoryType.GenericWrapper; } }
            public readonly SelectGenericTypeArg SelectGenericTypeArg;

            public GenericWrapper(SelectGenericTypeArg selectGenericTypeArg = null)
            {
                SelectGenericTypeArg = selectGenericTypeArg ?? SelectSingleOrThrow;
            }

            private static Type SelectSingleOrThrow(Type[] typeArgs)
            {
                return typeArgs.ThrowIf(typeArgs.Length != 1)[0];
            }
        }

        public class Decorator : FactorySetup
        {
            public override FactoryType Type { get { return FactoryType.Decorator; } }
            public override bool SkipCache { get { return true; } }
            public readonly Func<Request, bool> IsApplicable;

            public Decorator(Func<Request, bool> isApplicable = null)
            {
                IsApplicable = isApplicable ?? ApplicableIndeed;
            }

            private static bool ApplicableIndeed(Request _)
            {
                return true;
            }
        }
    }
}