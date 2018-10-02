using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue7_1_Context_based_injection
    {
        [Test]
        public void Request_based_impl_type()
        {
            var container = new Container();

            container.Register<SignInViewModel>();
            container.Register<SignUpViewModel>();

            container.Register<IValidator>(
                Reuse.Singleton,
                Made.Of(r => typeof(AbstractValidator<>).MakeGenericType(r.Parent.ImplementationType)));

            var signInViewModel = container.Resolve<SignInViewModel>();
            Assert.IsInstanceOf<AbstractValidator<SignInViewModel>>(signInViewModel.Validator);

            var signUpViewModel = container.Resolve<SignUpViewModel>();
            Assert.IsInstanceOf<AbstractValidator<SignUpViewModel>>(signUpViewModel.Validator);
        }

        public interface IValidator { }

        public interface IValidator<T> : IValidator { }

        public class AbstractValidator<T> : IValidator<T> { }

        public class SignInViewModel
        {
            public IValidator Validator { get; }
            public SignInViewModel(IValidator validator)
            {
                Validator = validator;
            }
        }

        public class SignUpViewModel
        {
            public IValidator Validator { get; }
            public SignUpViewModel(IValidator validator)
            {
                Validator = validator;
            }
        }
    }
}
