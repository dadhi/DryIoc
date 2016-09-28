using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DryIoc.Mvc
{
    public static class DryIocDataAnnotationsValidator
    {
        public static IContainer WithDataAnnotationsValidator(this IContainer container)
        {
            container.ThrowIfNull();

            DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(
                (metadata, context, attribute) =>
                new DryIocDataAnnotationsModelValidator(new DryIocServiceProvider(container), metadata, context, attribute)
            );

            DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(
                (metadata, context) =>
                new DryIocValidatableObjectAdapter(new DryIocServiceProvider(container), metadata, context)
            );

            return container;
        }
    }

    public class DryIocServiceProvider : IServiceProvider
    {
        readonly IResolver _container;

        public DryIocServiceProvider(IResolver container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }

    public class DryIocDataAnnotationsModelValidator : DataAnnotationsModelValidator
    {
        readonly IServiceProvider _serviceProvider;

        public DryIocDataAnnotationsModelValidator(IServiceProvider serviceProvider, ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute)
        {
            _serviceProvider = serviceProvider;
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            var context = new ValidationContext(container ?? Metadata.Model, _serviceProvider, null)
            {
                DisplayName = Metadata.GetDisplayName()
            };

            var result = Attribute.GetValidationResult(Metadata.Model, context);

            if (result != ValidationResult.Success)
            {
                yield return new ModelValidationResult
                {
                    Message = result?.ErrorMessage
                };
            }
        }
    }

    public class DryIocValidatableObjectAdapter : ValidatableObjectAdapter
    {
        readonly IServiceProvider _serviceProvider;

        public DryIocValidatableObjectAdapter(IServiceProvider serviceProvider, ModelMetadata metadata, ControllerContext context) :
            base(metadata, context)
        {
            _serviceProvider = serviceProvider;
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            var model = Metadata.Model;

            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            var validatable = model as IValidatableObject;

            if (validatable == null)
            {
                return base.Validate(container);
            }

            var validationContext = new ValidationContext(validatable, _serviceProvider, null);

            return ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result == ValidationResult.Success) continue;

                if (result.MemberNames == null || !result.MemberNames.Any())
                {
                    yield return new ModelValidationResult { Message = result.ErrorMessage };
                }

                else
                {
                    foreach (var memberName in result.MemberNames)
                    {
                        yield return new ModelValidationResult { Message = result.ErrorMessage, MemberName = memberName };
                    }
                }
            }
        }
    }
}
