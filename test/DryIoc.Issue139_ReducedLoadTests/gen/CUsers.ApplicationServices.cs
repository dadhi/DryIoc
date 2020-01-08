using CUsers.Domain;
using Organizations;
using IUserRepository = CUsers.Domain.IUserRepository;

namespace CUsers.ApplicationServices
{
    public class EventReader
        : IEventReader
    {
        public EventReader(
            IUserEventRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IUserEventRepository field0;
    }


    public interface IEventReader
    {
    }


    public interface IOrganizationEventHandler
    {
    }


    public class OrganizationEventHandler
        : IOrganizationEventHandler
    {
        public OrganizationEventHandler(
            IOrganizationChangeResolver arg0,
            IUserEventRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationChangeResolver field0;
        public readonly IUserEventRepository field1;
    }


    public interface IOrganizationService
    {
    }


    public class OrganizationService
        : IOrganizationService
    {
        public OrganizationService(
            IOrganizationRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IOrganizationRepository field0;
    }


    public interface IUserEventHandler
    {
    }


    public class UserEventHandler
        : IUserEventHandler
    {
        public UserEventHandler(
            IUserChangeResolver arg0,
            IUserEventRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IUserChangeResolver field0;
        public readonly IUserEventRepository field1;
    }


    public interface IUserService
    {
    }


    public class UserService
        : IUserService
    {
        public UserService(
            IUserRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IUserRepository field0;
    }
}