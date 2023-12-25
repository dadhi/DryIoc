using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue41_RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType : ITest
    {
        public int Run()
        {
            Test_with_source_conversion_operator();
            Test_with_target_conversion_operator();
            return 2;
        }

        [Test]
        public void Test_with_source_conversion_operator()
        {
            var container = new Container();
            container.Register<Factory>();
            container.Register<string>(
                made: Made.Of(
                    f => ServiceInfo.Of<Factory>(),
                    f => f.Create("Hello World")),
                serviceKey: "foo");

            container.Resolve<string>("foo"); // Interpreter
            container.Resolve<string>("foo"); // Compiled expression delegate
            container.Resolve<string>("foo"); // Cached delegate
        }

        public object Resolve() =>
            (string)new Getter<string>("abc");

        [Test]
        public void Test_with_target_conversion_operator()
        {
            var container = new Container();

            container.Register<Factory>();

            container.Register(
                Made.Of(
                    f => ServiceInfo.Of<Factory>(),
                    f => f.Create2((Getter<string>)"Hello World")),
                serviceKey: "foo");

            container.Resolve<string>("foo"); // Interpreter
            container.Resolve<string>("foo"); // Compiled expression delegate
            container.Resolve<string>("foo"); // Cached delegate
        }

        class Getter<TValue>
        {
            public static implicit operator TValue(Getter<TValue> aGetter) => aGetter.Value;

            public static explicit operator Getter<TValue>(TValue x) => new Getter<TValue>(x);

            public TValue Value { get; }

            public Getter(TValue aValue)
            {
                Value = aValue;
            }
        }

        class Factory
        {
            public Getter<TValue> Create<TValue>(TValue aValue) => (Getter<TValue>)aValue;

            public TValue Create2<TValue>(Getter<TValue> aValue) => new Getter<TValue>(aValue);

        }
    }
}
