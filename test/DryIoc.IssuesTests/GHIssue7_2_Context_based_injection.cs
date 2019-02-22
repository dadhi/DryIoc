using System;
using ImTools;
using NUnit.Framework;
using Validators;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue7_2_Context_based_injection
    {
        [Test]
        public void Option_1_multiple_registrations_with_condition()
        {
            var container = new Container();

            container.Register<SignInViewModelValidationBuilder>();
            container.Register<SignInViewModel>();
            container.Register<IObjectValidator>(Made.Of(
                req => ServiceInfo.Of<SignInViewModelValidationBuilder>(), 
                builder => builder.Build(Arg.Of<SignInViewModel>())),
                setup: Setup.With(condition: req => req.Parent.ImplementationType.IsAssignableTo<SignInViewModel>()));

            container.Register<SignUpViewModelValidationBuilder>();
            container.Register<SignUpViewModel>();
            container.Register<IObjectValidator>(Made.Of(
                req => ServiceInfo.Of<SignUpViewModelValidationBuilder>(),
                builder => builder.Build(Arg.Of<SignUpViewModel>())),
                setup: Setup.With(condition: req => req.Parent.ImplementationType.IsAssignableTo<SignUpViewModel>()));

            var sin = container.Resolve<SignInViewModel>();
            Assert.NotNull(sin);

            var sup = container.Resolve<SignUpViewModel>();
            Assert.NotNull(sup);
        }

        [Test]
        public void Option_2_single_generic_registration()
        {
            var container = new Container();

            container.Register<SignInViewModel>();
            container.RegisterMany<SignInViewModelValidationBuilder>();

            container.Register<SignUpViewModel>();
            container.RegisterMany<SignUpViewModelValidationBuilder>();

            // Way 1: Works. Internally just calls the FactoryMethod.Of as in case below.
            //container.Register<IObjectValidator>(made: Made.Of(
            //    req => typeof(IObjectValidatorBuilder<>).MakeGenericType(req.Parent.ImplementationType)
            //        .SingleMethod("Build"),
            //    req => ServiceInfo.Of(
            //        typeof(IObjectValidatorBuilder<>).MakeGenericType(req.Parent.ImplementationType))));

            // Way 2: Works. 
            //container.Register<IObjectValidator>(made: Made.Of(req =>
            //{
            //    var builderType = typeof(IObjectValidatorBuilder<>).MakeGenericType(req.Parent.ImplementationType);
            //    return FactoryMethod.Of(builderType.SingleMethod("Build"), ServiceInfo.Of(builderType));
            //}));

            // Works 3: Same as above but with a bit of sugar with `Do` - most succinct variant
            container.Register<IObjectValidator>(made: Made.Of(req =>
                typeof(IObjectValidatorBuilder<>).MakeGenericType(req.Parent.ImplementationType)
                    .Map(t => FactoryMethod.Of(t.SingleMethod("Build"), ServiceInfo.Of(t)))));

            var sin = container.Resolve<SignInViewModel>();
            Assert.NotNull(sin);

            var sup = container.Resolve<SignUpViewModel>();
            Assert.NotNull(sup);
        }

        public class SignInViewModelValidationBuilder : ValidationBuilder<SignInViewModel>
        {
        }

        public class SignInViewModel : IValidatableObject
        {
            public IObjectValidator Validator { get; }

            // IObjectValidator should be build by SignInViewModelValidationBuilder
            //public SignInViewModel(IObjectValidator validator) // NOPE: don't work because of recursive dependency
            public SignInViewModel(Func<SignInViewModel, IObjectValidator> validator) //YEP!
            {
                Validator = validator(this);
            }
        }

        public class SignUpViewModelValidationBuilder : ValidationBuilder<SignUpViewModel>
        {
        }

        public class SignUpViewModel : IValidatableObject
        {
            public IObjectValidator Validator { get; }

            // IObjectValidator should be build by SignUpViewModelValidationBuilder
            //public SignUnViewModel(IObjectValidator validator) // NOPE: don't work because of recursive dependency
            public SignUpViewModel(Func<SignUpViewModel, IObjectValidator> validator) 
            {
                Validator = validator(this);
            }
        }
    }
}

namespace Validators // not my assembly
{

    internal class ObjectValidator<TObject> : IObjectValidator where TObject : IValidatableObject
    {
        public ObjectValidator(TObject instance)
        {
        }
    }

    public interface IObjectValidator
    {
    }

    public interface IValidatableObject
    {
        IObjectValidator Validator { get; }
    }

    public interface IObjectValidatorBuilder
    {
        IObjectValidator Build(IValidatableObject instance);
    }

    public interface IObjectValidatorBuilder<in TObject> where TObject : IValidatableObject
    {
        IObjectValidator Build(TObject instance);
    }

    public class ValidationBuilder<TObject> :
        IObjectValidatorBuilder<TObject>,
        IObjectValidatorBuilder
        where TObject : IValidatableObject
    {
        IObjectValidator IObjectValidatorBuilder.Build(IValidatableObject instance)
        {
            return Build((TObject)instance);
        }

        public IObjectValidator Build(TObject instance)
        {
            var validator = new ObjectValidator<TObject>(instance);
            return validator;
        }
    }
}
