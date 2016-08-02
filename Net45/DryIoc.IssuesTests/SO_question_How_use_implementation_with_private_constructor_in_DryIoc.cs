using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_question_How_use_implementation_with_private_constructor_in_DryIoc
    {
        public class A { internal A() { } }
        public class B { internal B(A a) { } }

        [Test]
        public void Test()
        {
            var c = new Container();
            c.RegisterMany(new[] {typeof(A), typeof(B)},
                made: Made.Of(SelectMostResolvableConstructor(includeNonPublic: true)));

            c.Resolve<B>();
        }

        private FactoryMethodSelector SelectMostResolvableConstructor(bool includeNonPublic = false)
        {
            return request =>
            {
                var implementationType = request.ImplementationType.ThrowIfNull();
                var ctors = implementationType.GetAllConstructors(includeNonPublic: true).ToArrayOrSelf();
                if (ctors.Length == 0)
                    return null; // Delegate handling of constructor absence to the Caller code.
                if (ctors.Length == 1)
                    return FactoryMethod.Of(ctors[0]);

                var ctorsWithMoreParamsFirst = ctors
                    .Select(c => new {Ctor = c, Params = c.GetParameters()})
                    .OrderByDescending(x => x.Params.Length);

                var rules = request.Container.Rules;
                var parameterSelector = rules.Parameters.And(request.Made.Parameters)(request);

                var matchedCtor = ctorsWithMoreParamsFirst.FirstOrDefault(x =>
                        x.Params.All(p => IsResolvableParameter(p, parameterSelector, request)));

                var ctor = matchedCtor.ThrowIfNull(Error.UnableToFindCtorWithAllResolvableArgs, request).Ctor;

                return FactoryMethod.Of(ctor);
            };
        }

        private static bool IsResolvableParameter(ParameterInfo parameter,
            Func<ParameterInfo, ParameterServiceInfo> parameterSelector, Request request)
        {
            var parameterServiceInfo = parameterSelector(parameter) ?? ParameterServiceInfo.Of(parameter);
            var parameterRequest = request.Push(parameterServiceInfo.WithDetails(ServiceDetails.IfUnresolvedReturnDefault, request));

            if (parameterServiceInfo.Details.HasCustomValue)
            {
                var customValue = parameterServiceInfo.Details.CustomValue;
                return customValue == null
                    || customValue.GetType().IsAssignableTo(parameterRequest.ServiceType);
            }

            var parameterFactory = request.Container.ResolveFactory(parameterRequest);
            return parameterFactory != null && parameterFactory.GetExpressionOrDefault(parameterRequest) != null;
        }
    }
}
