using System;
using System.Collections.Generic;
using NUnit.Framework;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter : ITest
    {
        public int Run()
        {
            Test1();
            Test2();
            return 2;
        }

        [Test]
        public void Test1()
        {
            var c = new Container(Rules.Default.WithUseInterpretation());

            c.Register<IServiceB, ServiceB>();
            c.Register<IDep, Dep>();
            c.Register<ServiceA>();

            var a = c.Resolve<ServiceA>();
            var b = ((IServiceB[])a.Services)[0] as ServiceB;
            Assert.IsNotNull(b);
            Assert.AreSame(c, b.R);
            Assert.IsInstanceOf<Dep>(b.D);
        }

        class ServiceA
        {
            public IEnumerable<IServiceB> Services;
            public ServiceA(Func<ServiceA, string, int, bool, IEnumerable<IServiceB>> onStart)
            {
                Services = onStart(this, "!", 42, true);
            }
        }

        interface IDep { }
        class Dep : IDep { }

        interface IServiceB { }
        class ServiceB : IServiceB
        {
            public IResolver R;
            public IDep D;
            public ServiceB(IResolver r, IDep d)
            {
                R = r;
                D = d;
            }
        }

        [Test]
        public void Test2()
        {
            using var c = new Container();

            c.Register<IOtherDependency, MyOtherDependency>();
            c.Register<MyService_IntDefaultArg>();
            c.Register<MyService_NullableIntArgWithNullValue>();
            c.Register<MyService_NullableIntArgWithIntValue>();

            var m1 = c.Resolve<MyService_IntDefaultArg>();
            Assert.AreEqual(15, m1.OptionalArgument);

            var m2 = c.Resolve<MyService_NullableIntArgWithNullValue>();
            Assert.AreEqual(15, m2.OptionalArgument);

            var m3 = c.Resolve<MyService_NullableIntArgWithIntValue>();
            Assert.AreEqual(15, m3.OptionalArgument);

            var m1_compiled = c.Resolve<MyService_IntDefaultArg>();
            Assert.AreEqual(15, m1_compiled.OptionalArgument);

            var m2_compiled = c.Resolve<MyService_NullableIntArgWithNullValue>();
            Assert.AreEqual(15, m2_compiled.OptionalArgument);

            var m3_expr = c.Resolve<LambdaExpression>(typeof(MyService_NullableIntArgWithIntValue));
            var m3_expr_str = m3_expr.ToExpressionString();
            StringAssert.Contains("int?", m3_expr_str);

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