using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue41_RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<Factory>();
            container.Register<string>(
                made: Made.Of(
                    f => ServiceInfo.Of<Factory>(),
                    f => f.Create("Hello World")),
                serviceKey: "foo");

            container.Resolve<string>("foo");
        }

        public object Resolve() =>
            (string)new Getter<string>("abc");

        class Getter<TValue>
        {
            public static implicit operator TValue(Getter<TValue> aGetter) => aGetter.Value;

            public TValue Value { get; }

            public Getter(TValue aValue)
            {
                Value = aValue;
            }
        }

        class Factory
        {
            public Getter<TValue> Create<TValue>(TValue aValue) => 
                new Getter<TValue>(aValue);
        }
    }
}
