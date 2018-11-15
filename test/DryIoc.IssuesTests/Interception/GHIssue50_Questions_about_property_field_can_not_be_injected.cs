using Castle.DynamicProxy;
using NUnit.Framework;
using DryIoc.Interception;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class GHIssue50_Questions_about_property_field_can_not_be_injected
    {
        [Test]
        public void Test()
        {
            var c = new Container(r => r.WithTrackingDisposableTransients());

            c.Register<ITool, Tool>();

            c.RegisterMany(
                new[] { typeof(IApplicationImpl), typeof(ApplicationImpl) }, 
                typeof(ApplicationImpl), 
                Reuse.Transient, 
                PropertiesAndFields.Auto);

            c.Register<TestInterceptor>();

            c.Intercept<IApplicationImpl, TestInterceptor>();
            c.Intercept<ApplicationImpl, TestInterceptor>();

            var iApp = c.Resolve<IApplicationImpl>();
            Assert.AreEqual("I am Tool (intercepted)", iApp.UseTool());

            var app = c.Resolve<ApplicationImpl>();
            Assert.AreEqual("I am Tool (intercepted)", app.UseTool());
        }

        public interface ITool
        {
            string Print();
        }

        public class Tool : ITool
        {
            public string Print() => "I am Tool";
        }

        public class DefaultTool : ITool
        {
            public string Print() => "I am Default Tool";
        }

        public class TestInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                invocation.ReturnValue += " (intercepted)";
            }
        }

        public interface IApplicationImpl
        {
            string UseTool();
        }

        public class ApplicationImpl : IApplicationImpl
        {
            public ITool Tool { get; set; }

            public ApplicationImpl()
            {
                Tool = new DefaultTool();
            }

            public virtual string UseTool() => Tool.Print();
        }
    }
}
