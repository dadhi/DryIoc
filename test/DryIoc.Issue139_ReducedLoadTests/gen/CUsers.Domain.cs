using System;
using Databases;

namespace CUsers.Domain
{
    public interface IOrganizationChangeResolver
    {
    }


    public class OrganizationChangeResolver
        : IOrganizationChangeResolver
    {
        public OrganizationChangeResolver(
            IUserChangeResolver arg0
        )
        {
            field0 = arg0;
        }

        public readonly IUserChangeResolver field0;
    }

    public interface IUserChangeResolver
    {
    }


    public class UserChangeResolver
        : IUserChangeResolver
    {
        public UserChangeResolver(
            IUserRepository arg0,
            IUserEventRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IUserRepository field0;
        public readonly IUserEventRepository field1;
    }


    public interface IUserEventRepository
    {
    }


    public class UserEventRepository
        : IUserEventRepository
    {
        public UserEventRepository(
            IMasterDatabase arg0
        )
        {
            field0 = arg0;
        }

        public readonly IMasterDatabase field0;
    }


    public interface IUserRepository
    {
    }


    public class UserRepository
        : IUserRepository
    {
        public UserRepository(
            IMasterDatabase arg0
        )
        {
            field0 = arg0;
        }

        public readonly IMasterDatabase field0;
    }


    public class DistributorEvent
    {
        public DistributorEvent(
            int arg0,
            User arg1,
            Event arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly User field1;
        public readonly Event field2;
    }


    public class Event
    {
    }


    public class Organization
    {
        public Organization(
            int arg0,
            int? arg1,
            bool arg2,
            bool arg3,
            bool arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly int field0;
        public readonly int? field1;
        public readonly bool field2;
        public readonly bool field3;
        public readonly bool field4;
    }


    public class User
    {
        public User(
            int arg0,
            bool arg1,
            string arg2,
            string arg3,
            string arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly int field0;
        public readonly bool field1;
        public readonly string field2;
        public readonly string field3;
        public readonly string field4;
    }


    public class UserEvent
    {
        public UserEvent(
            Int64 arg0,
            string arg1,
            string arg2,
            string arg3,
            Event arg4,
            DateTime arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly Int64 field0;
        public readonly string field1;
        public readonly string field2;
        public readonly string field3;
        public readonly Event field4;
        public readonly DateTime field5;
    }
}