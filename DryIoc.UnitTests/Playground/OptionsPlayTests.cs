using System;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class OptionsPlayTests
    {
        [Test]
        [Ignore]
        public void Test()
        {
            var b = new B("b", new B.Options { Metadata = 3 });

            b = new B("b", options => { options.Metadata = 3; options.Flag = true; });

            Assert.That(b.Count(), Is.EqualTo(1));
        }
    }

    public abstract class A
    {
        public class Options
        {
            public bool Flag;
            public object Metadata;

            public Options() { }

            protected Options(Options options)
            {
                Flag = options.Flag;
                Metadata = options.Metadata;
            }

            public virtual Options Copy()
            {
                return new Options(this);
            }
        }

        public readonly string Name;

        public bool Flag { get { return _options.Flag; } }

        public object Metadata { get { return _options.Metadata; } }

        protected A(string name, Options options)
        {
            Name = name;
            _options = options.Copy();
        }

        protected readonly Options _options;
    }

    public class B : A
    {
        public new class Options : A.Options
        {
            internal static readonly Options Default = new Options();

            public Func<int> Count;

            public Options() { }

            protected Options(Options options) : base(options)
            {
                Count = options.Count;
            }

            public override A.Options Copy()
            {
                return this == Default ? Default : new Options(this);
            }
        }

        public Func<int> Count { get { return ((Options)_options).Count; } }

        public B(string name, Options options = null)
            : base(name, options ?? Options.Default) { }

        public B(string name, Action<Options> setOptions) :
            base(name, new Func<Options>(() =>
            {
                var options = new Options();
                setOptions(options);
                return options;
            })()) { }
    }

    public class C : B
    {
        public C(string name, Options options = null)
            : base(name, options) { }
    }
}
