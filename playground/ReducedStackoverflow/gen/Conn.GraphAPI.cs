using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Conn.GraphAPI
{
    public class AuthenticationInputModel
        : IValidatableObject
    {
        public AuthenticationInputModel(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class ConnResourceError
        : IValidatableObject
    {
        public ConnResourceError(
        )
        {
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class IPhoneRecord
        : IValidatableObject
    {
        public IPhoneRecord(
            Guid? arg0,
            string arg1,
            DateTime? arg2,
            bool? arg3,
            bool? arg4,
            bool? arg5,
            string arg6
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly Guid? field0;
        public readonly string field1;
        public readonly DateTime? field2;
        public readonly bool? field3;
        public readonly bool? field4;
        public readonly bool? field5;
        public readonly string field6;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class PhoneAddInputModel
        : IValidatableObject
    {
        public PhoneAddInputModel(
            string arg0,
            string arg1,
            bool? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly bool? field2;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class PhoneModel
        : IValidatableObject
    {
        public PhoneModel(
            Guid? arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool? arg4,
            DateTime? arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly Guid? field0;
        public readonly string field1;
        public readonly bool? field2;
        public readonly bool? field3;
        public readonly bool? field4;
        public readonly DateTime? field5;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class PhoneVerificationInputModel
        : IValidatableObject
    {
        public PhoneVerificationInputModel(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class RequestChangeEmailInputModel
        : IValidatableObject
    {
        public RequestChangeEmailInputModel(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            throw new NotImplementedException();
    }


    public class UserClientModel
        : IValidatableObject
    {
        public UserClientModel(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            DateTime? arg4,
            DateTime? arg5,
            int? arg6
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
        public readonly string field3;
        public readonly DateTime? field4;
        public readonly DateTime? field5;
        public readonly int? field6;

        public bool Equals()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}