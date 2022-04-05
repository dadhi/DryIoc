using System;
using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    internal class GHIssue188_Custom_delegate_wrapper_resolving
    {
        [Test]
        public void Resolve_custom_delegate_wrapper()
        {
            var container = new Container();
            container.Register<Simple>();
            container.RegisterFactoryDelegateType<SimpleFactory>();

            // Using generic factory wrapper should work
            var genericFactoryWrapper = container.Resolve<Func<string, Simple>>();
            var simple1 = genericFactoryWrapper("Simple1");
            Assert.AreEqual("Simple1", simple1.Text);

            // Using custom delegate factory wrapper should work, too
            var customDelegateFactoryWrapper = container.Resolve<SimpleFactory>();
            var simple2 = customDelegateFactoryWrapper("Simple2");
            Assert.AreEqual("Simple2", simple2.Text);
        }

        [Test]
        public void Resolve_only_one_dependency_instance()
        {
            var container = new Container();
            container.Register<Simple>();
            container.RegisterFactoryDelegateType<SimpleFactory>();

            container.Register<Countable>();
            container.Register<Complex>();

            Countable.Counter = 0;
            container.Resolve<Complex>();
            Assert.AreEqual(1, Countable.Counter, "The container resolved more than one instance of Countable.");
        }

        [Test]
        public void Resolve_only_one_dependency_instance_without_UseInterpretation_should_work()
        {
            var container = new Container(rules => rules.WithoutInterpretationForTheFirstResolution());
            container.Register<Simple>();
            container.RegisterFactoryDelegateType<SimpleFactory>();

            container.Register<Countable>();
            container.Register<Complex>();

            Countable.Counter = 0;
            container.Resolve<Complex>();
            Assert.AreEqual(1, Countable.Counter, "The container resolved more than one instance of Countable.");
        }

        [Test]
        public void Resolve_only_one_dependency_instance_with_RegisterDelegate()
        {
            var container = new Container();
            container.Register<Simple>();

            // DryIoc v4.0.5
            container.RegisterDelegate<SimpleFactory>(r => s => r.Resolve<Func<string, Simple>>()(s));

            container.Register<Countable>();
            container.Register<Complex>();

            Countable.Counter = 0;
            container.Resolve<Complex>();
            Assert.AreEqual(1, Countable.Counter, "The container resolved more than one instance of Countable.");
        }

        [Test]
        public void Resolve_only_one_dependency_instance_with_RegisterDelegate_of_func_with_args()
        {
            var container = new Container(rules => rules.WithoutInterpretationForTheFirstResolution());
            container.Register<Simple>();

            // DryIoc v4.0.5
            //container.RegisterDelegate<SimpleFactory>(r => s => r.Resolve<Func<string, Simple>>()(s));

            // DryIoc v4.1 - preferable, because it does not call Resolve (the Func<..> is injected) - so avoiding all problems with service locator
            container.RegisterDelegate<Func<string, Simple>, SimpleFactory>(f => s => f(s));

            container.Register<Countable>();
            container.Register<Complex>();

            Countable.Counter = 0;
            container.Resolve<Complex>();
            Assert.AreEqual(1, Countable.Counter, "The container resolved more than one instance of Countable.");
        }

        /// <summary>
        /// This is a custom delegate we want to use instead of <see cref="Func{Tin, TResult}"/>.
        /// </summary>
        private delegate Simple SimpleFactory(string s);

        private class Simple
        {
            public string Text { get; }

            public Simple(string text)
            {
                Text = text;
            }
        }

        private class Countable
        {
            public static int Counter;

            public Countable()
            {
                ++Counter;
            }
        }

        private class Complex
        {
            public Complex(Countable c, SimpleFactory simpleFactory)
            {
            }
        }
    }

    internal static class ContainerExtension
    {
        /// <summary>
        /// DryIoc only supports resolving generic factory delegates of type <see cref="Func{TResult}"/>.
        /// If you want to resolve a custom delegate type with or without parameters, use this method.
        /// </summary>
        /// <typeparam name="TDelegate">The custom delegate type to register.</typeparam>
        public static void RegisterFactoryDelegateType<TDelegate>(this IContainer container, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) where TDelegate : System.Delegate
        {
            container.Register(typeof(TDelegate), new CustomDelegateWrapper<TDelegate>(), ifAlreadyRegistered, serviceKey);
        }
    }

    internal class CustomDelegateWrapper<TDelegate> : Factory where TDelegate : System.Delegate
    {
        public override Expression CreateExpressionOrDefault(Request request)
        {
            var originalFactoryType = GetOriginalFactoryType();
            var originalFactoryRequest = Request.Create(request.Container, originalFactoryType, request.ServiceKey);
            var originalFactory = originalFactoryRequest.Container.ResolveFactory(originalFactoryRequest);

            var originalFactoryExpression = (LambdaExpression)originalFactory.GetExpressionOrDefault(originalFactoryRequest);

            var convertedFactoryExpression = Expression.Lambda<TDelegate>(originalFactoryExpression.Body, originalFactoryExpression.Parameters);

            return convertedFactoryExpression;
        }

        private static Type GetOriginalFactoryType()
        {
            var delegateType = typeof(TDelegate);
            var invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null)
                throw new ArgumentException("Invalid delegate type: " + delegateType.Name);
            if (invokeMethod.ReturnType == typeof(void))
                throw new ArgumentException("Only function types are supported.");

            // Construct original factory type like Func<TIn, TOut>
            var parameters = invokeMethod.GetParameters();
            var genericArguments = new Type[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                genericArguments[i] = parameters[i].ParameterType;
            }

            genericArguments[genericArguments.Length - 1] = invokeMethod.ReturnType;
            var originalFactoryType = GetFuncType(parameters.Length).MakeGenericType(genericArguments);

            return originalFactoryType;
        }

        private static Type GetFuncType(int parameterCount)
        {
            switch (parameterCount)
            {
                case 0:
                    return typeof(Func<>);
                case 1:
                    return typeof(Func<,>);
                case 2:
                    return typeof(Func<,,>);
                case 3:
                    return typeof(Func<,,,>);
                case 4:
                    return typeof(Func<,,,,>);
                case 5:
                    return typeof(Func<,,,,,>);
                case 6:
                    return typeof(Func<,,,,,,>);
                case 7:
                    return typeof(Func<,,,,,,,>);
                case 8:
                    return typeof(Func<,,,,,,,,>);
                default:
                    throw new NotSupportedException(
                        "Factory delegates with more than 8 parameters are not supported.");
            }
        }
    }
}
