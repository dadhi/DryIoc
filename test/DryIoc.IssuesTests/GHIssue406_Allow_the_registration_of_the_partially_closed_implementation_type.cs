using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container();

            container.Register(
                typeof(ISuperWrapper<,>).MakeGenericType(typeof(IWrappedType<>), typeof(IResult)),
                typeof(SuperWrapper<>));

            var x = container.Resolve<ISuperWrapper<IWrappedType<string>, IResult>>();

            Assert.IsInstanceOf<SuperWrapper<string>>(x);
        }

        interface IResult {}
        interface IWrappedType<T> {}
        interface ISuperWrapper<T, R> {}
        class SuperWrapper<X> : ISuperWrapper<IWrappedType<X>, IResult> {}
    }
}