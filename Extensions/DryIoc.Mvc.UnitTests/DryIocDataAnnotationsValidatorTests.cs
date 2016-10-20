using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using NUnit.Framework;

namespace DryIoc.Mvc.UnitTests
{
    public class DryIocDataAnnotationsValidatorTests
    {
        private Container _container;
        private DryIocServiceProvider _serviceProvider;

        [SetUp]
        public void Initialize()
        {
            _container = new Container();
            _serviceProvider = new DryIocServiceProvider(_container);
        }

        [Test]
        public void Can_register_validators()
        {
            Assert.DoesNotThrow(() => _container.WithDataAnnotationsValidator());
        }

        private class UserModel : IValidatableObject
        {
            public string Login { get; set; }
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var results = new List<ValidationResult>();

                if (string.IsNullOrEmpty(Login))
                {
                    results.Add(new ValidationResult("Field Login is required!"));
                }

                return results;
            }
        }

        [Test]
        public void User_model_login_is_valid()
        {
            var user = new UserModel
            {
                Login = "arabasso"
            };

            var results = ValidateUserModel(user);

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void User_model_login_not_is_valid()
        {
            var user = new UserModel();

            var results = ValidateUserModel(user);

            Assert.That(results, Is.Not.Empty);
        }

        private IEnumerable<ModelValidationResult> ValidateUserModel(UserModel user)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => user.Login, typeof(UserModel), "Login");

            var dataAnnorationValidator = new DryIocDataAnnotationsModelValidator(_serviceProvider, metadata,
                new ControllerContext(),
                new RequiredAttribute());

            return dataAnnorationValidator.Validate(user);
        }

        [Test]
        public void User_model_validatable_object_is_valid()
        {
            var user = new UserModel
            {
                Login = "arabasso"
            };

            var results = ValidateUserObjectAdapter(user);

            Assert.That(results, Is.Empty);
        }

        private IEnumerable<ModelValidationResult> ValidateUserObjectAdapter(UserModel user)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => user, typeof(UserModel));

            var validatableObjectAdapter = new DryIocValidatableObjectAdapter(_serviceProvider, metadata,
                new ControllerContext());

            return validatableObjectAdapter.Validate(user);
        }

        [Test]
        public void User_model_validatable_object_not_is_valid()
        {
            var user = new UserModel();

            var results = ValidateUserObjectAdapter(user);

            Assert.That(results, Is.Not.Empty);
        }

        private class VerifyLoginValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var login = (LoginModel)value;

                var otherLogin = validationContext.GetService(typeof(LoginModel));

                return login.Equals(otherLogin)
                    ? ValidationResult.Success
                    : new ValidationResult("Invalid Login!");
            }
        }

        private class LoginModel : IValidatableObject
        {
            public string Username { get; set; }
            public string Password { get; set; }

            private bool Equals(LoginModel other)
            {
                return string.Equals(Username, other.Username) && string.Equals(Password, other.Password);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((LoginModel)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Username?.GetHashCode() ?? 0)*397) ^ (Password?.GetHashCode() ?? 0);
                }
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var results = new List<ValidationResult>();

                var otherLogin = validationContext.GetService(typeof(LoginModel));

                if (!Equals(otherLogin))
                {
                    results.Add(new ValidationResult("Invalid Login!"));
                }

                return results;
            }
        }

        [Test]
        public void Login_model_login_is_valid()
        {
            var login = new LoginModel
            {
                Username = "arabasso",
                Password = "123"
            };

            _container.RegisterDelegate(c => login);

            var results = ValidateLoginModel(login);

            Assert.That(results, Is.Empty);
        }

        private IEnumerable<ModelValidationResult> ValidateLoginModel(LoginModel login)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => login, typeof(LoginModel));

            var dataAnnorationValidator = new DryIocDataAnnotationsModelValidator(_serviceProvider, metadata,
                new ControllerContext(),
                new VerifyLoginValidationAttribute());

            return dataAnnorationValidator.Validate(login);
        }

        [Test]
        public void Login_model_login_not_is_valid()
        {
            var login = new LoginModel
            {
                Username = "arabasso",
                Password = "123"
            };

            _container.RegisterDelegate(c => new LoginModel());

            var results = ValidateLoginModel(login);

            Assert.That(results, Is.Not.Empty);
        }

        [Test]
        public void Login_model_validatable_object_is_valid()
        {
            var login = new LoginModel
            {
                Username = "arabasso",
                Password = "123"
            };

            _container.RegisterDelegate(c => login);

            var results = ValidateLoginObjectAdapter(login);

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void Login_model_validatable_object_not_is_valid()
        {
            var login = new LoginModel
            {
                Username = "arabasso",
                Password = "123"
            };

            _container.RegisterDelegate(c => new LoginModel());

            var results = ValidateLoginObjectAdapter(login);

            Assert.That(results, Is.Not.Empty);
        }

        private IEnumerable<ModelValidationResult> ValidateLoginObjectAdapter(LoginModel login)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => login, typeof(LoginModel));

            var validatableObjectAdapter = new DryIocValidatableObjectAdapter(_serviceProvider, metadata,
                new ControllerContext());

            return validatableObjectAdapter.Validate(login);
        }
    }
}
