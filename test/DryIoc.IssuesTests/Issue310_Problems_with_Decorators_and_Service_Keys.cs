using System.Collections.Generic;
using Castle.DynamicProxy;
using DryIoc.Interception;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class Issue310_Problems_with_Decorators_and_Service_Keys : ITest
    {
        public int Run()
        {
            Decorator_dependency_should_not_require_key();
            Test_class_interception_with_decorators();
            return 2;
        }

        [Test]
        public void Decorator_dependency_should_not_require_key()
        {
            var container = new Container();

            container.Register<A>(serviceKey: 1);
            container.Register<B>();
            container.Register<A, D>(setup: Setup.Decorator);

            container.Resolve<A>(1);
        }

        public class B { }
        public class A { }
        public class D : A
        {
            public D(A a, B b)
            {
            }
        }

        [Test]
        public void Test_class_interception_with_decorators()
        {
            using (var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient()))
            {
                container.Register<AspectInterceptor>();

                container.Register<MyView>();
                container.Intercept<MyView, AspectInterceptor>();

                container.Register<MyForm>(serviceKey: 1);
                container.Intercept<MyForm, AspectInterceptor>(1);

                container.Register<MyForm>(serviceKey: 2);
                container.Intercept<MyForm, AspectInterceptor>(2);

                container.Resolve<MyForm>(1);
            }
        }

        public class AspectInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
            }
        }

        public class View { }

        public class Form
        {
            public List<View> Controls = new List<View>();

            public void InitializeComponent()
            {
            }
        }

        public class MyView : View { }

        public partial class MyForm : Form
        {

            public MyForm(MyView view)
            {
                InitializeComponent();
                Controls.Add(view);
            }
        }

    }
}
