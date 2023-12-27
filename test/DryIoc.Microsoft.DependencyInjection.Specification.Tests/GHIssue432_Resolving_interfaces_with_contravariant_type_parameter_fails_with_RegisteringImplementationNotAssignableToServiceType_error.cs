using System;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue432_Resolving_interfaces_with_contravariant_type_parameter_fails_with_RegisteringImplementationNotAssignableToServiceType_error : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var type = typeof(IValidator<>).MakeGenericType(typeof(ExtraSettings));
            var services = new ServiceCollection(); // Microsoft.Extensions.DependencyInjection

            services.AddSingleton<IValidator<ExtraSettings>, SettingsValidator>();

            var container = new Container();
            var adaptedContainer = container.WithDependencyInjectionAdapter(services); // this throws
            IServiceProvider serviceProvider = adaptedContainer;

            var validator = serviceProvider.GetService(type);
            Assert.NotNull(validator);
        }

        public class Settings
        {
            public string Address { get; set; }
        }

        public class ExtraSettings : Settings
        {
            public string Key { get; set; }
        }

        public interface IValidator<in T>
        {
            bool IsValid(T settings);
        }

        public abstract class AbstractValidator<T> : IValidator<T>
        {
            public bool IsValid(T settings)
            {
                return true;
            }
        }

        public class SettingsValidator : AbstractValidator<Settings>
        {

        }

    }
}
