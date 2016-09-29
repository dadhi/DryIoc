/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIoc.Mvc.DataAnnotationsValidator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    public static class DryIocDataAnnotationsValidator
    {
        public static IContainer WithDataAnnotationsValidator(this IContainer container)
        {
            container.ThrowIfNull();

            DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory(
                (metadata, context, attribute) =>
                new DryIocDataAnnotationsModelValidator(container, metadata, context, attribute)
            );

            DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory(
                (metadata, context) =>
                new DryIocValidatableObjectAdapter(container, metadata, context)
            );

            return container;
        }
    }

    public class DryIocDataAnnotationsModelValidator : DataAnnotationsModelValidator
    {
        private readonly IServiceProvider _serviceProvider;

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

        private static IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
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
