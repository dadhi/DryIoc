using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue180_Option_nullable_int_argument_with_not_null_default_value : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            using var c = new Container();

            c.Register<IOtherDependency, MyOtherDependency>();
            c.Register<MyService_IntDefaultArg>();
            c.Register<MyService_NullableIntArgWithNullValue>();
            c.Register<MyService_NullableIntArgWithIntValue>();

            // var m1 = c.Resolve<MyService_IntDefaultArg>();
            // Assert.AreEqual(15, m1.OptionalArgument);

            // var m2 = c.Resolve<MyService_NullableIntArgWithNullValue>();
            // Assert.AreEqual(15, m2.OptionalArgument);

            var m3 = c.Resolve<MyService_NullableIntArgWithIntValue>();
            Assert.AreEqual(15, m3.OptionalArgument);

            // var m1_compiled = c.Resolve<MyService_IntDefaultArg>();
            // Assert.AreEqual(15, m1_compiled.OptionalArgument);

            // var m2_compiled = c.Resolve<MyService_NullableIntArgWithNullValue>();
            // Assert.AreEqual(15, m2_compiled.OptionalArgument);

            var m3_compiled = c.Resolve<MyService_NullableIntArgWithIntValue>();
            Assert.AreEqual(15, m3_compiled.OptionalArgument);
        }

        interface IOtherDependency { }
        class MyOtherDependency : IOtherDependency { }

        class MyService_IntDefaultArg
        {
            public IOtherDependency OtherDependency;
            public int OptionalArgument;
            public MyService_IntDefaultArg(IOtherDependency otherDependency, int optionalArgument = 15)
            {
                OtherDependency = otherDependency;
                OptionalArgument = optionalArgument;
            }
        }

        class MyService_NullableIntArgWithNullValue
        {
            public IOtherDependency OtherDependency;
            public int OptionalArgument;
            public MyService_NullableIntArgWithNullValue(IOtherDependency otherDependency, int? optionalArgument = null)
            {
                OtherDependency = otherDependency;
                OptionalArgument = optionalArgument ?? 15;
            }
        }

        class MyService_NullableIntArgWithIntValue
        {
            public IOtherDependency OtherDependency;
            public int OptionalArgument;
            public MyService_NullableIntArgWithIntValue(IOtherDependency otherDependency, int? optionalArgument = 15)
            {
                OtherDependency = otherDependency;
                OptionalArgument = optionalArgument ?? 42;
            }
        }
    }
}