using System;
using System.Web.Http;
using Conn.Adapter;
using Logging;
using Shared;
using Users;

namespace Conn.Service
{
    public interface IWebHookRegistrationService
    {
    }


    public class WebHookRegistrationService
        : IWebHookRegistrationService
    {
        public WebHookRegistrationService(
            IConnClient arg0,
            IWebHookService arg1,
            ILogger arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IConnClient field0;
        public readonly IWebHookService field1;
        public readonly ILogger field2;
    }


    public interface IWebHookEventHandler
    {
    }


    public class WebHookEventHandler
        : IWebHookEventHandler
    {
        public WebHookEventHandler(
            IEventHandlerBackgroundService arg0,
            IUniqueUserService arg1,
            IConnClient arg2,
            ICountryService arg3,
            ILanguageService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IEventHandlerBackgroundService field0;
        public readonly IUniqueUserService field1;
        public readonly IConnClient field2;
        public readonly ICountryService field3;
        public readonly ILanguageService field4;
    }


    public interface IWebHookEventValidator
    {
    }


    public class WebHookEventValidator
        : IWebHookEventValidator
    {
        public WebHookEventValidator(
            IWebHookSettingsService arg0,
            IConnClient arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IWebHookSettingsService field0;
        public readonly IConnClient field1;
    }


    public class EventModel
    {
        public EventModel(
        )
        {
        }
    }


    public class UserModifiedEventPayload
    {
        public UserModifiedEventPayload(
        )
        {
        }
    }


    public class UserFromConnBase
    {
        public UserFromConnBase(
        )
        {
        }
    }


    public class UserFromConnOld : UserFromConnBase
    {
        public UserFromConnOld(
        )
        {
        }
    }


    public class UserFromConnCurrent : UserFromConnBase
    {
        public UserFromConnCurrent(
        )
        {
        }
    }


    public class WebHookGeneralConfiguration
    {
    }


    public class WebHookSettings
    {
        public WebHookSettings(
        )
        {
        }
    }


    public class EventHandlerBackgroundService
        : IEventHandlerBackgroundService, IDisposable
    {
        public EventHandlerBackgroundService(
            ILogger arg0
        )
        {
            field0 = arg0;
        }

        public readonly ILogger field0;

        public void Dispose()
        {
        }
    }


    public interface IEventHandlerBackgroundService
    {
    }


    public interface IWebHookService
    {
    }


    public class WebHookService
        : IWebHookService
    {
        public WebHookService(
            IWebHookSettingsService arg0,
            IConnWebHookService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IWebHookSettingsService field0;
        public readonly IConnWebHookService field1;
    }


    public interface IWebHookSettingsService
    {
    }


    public class WebHookSettingsService
        : IWebHookSettingsService
    {
        public WebHookSettingsService(
            IGlobalSettingsRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGlobalSettingsRepository field0;
    }


    public class ConnWebHooksController : ApiController
    {
        public ConnWebHooksController(
            IWebHookEventHandler arg0,
            IWebHookEventValidator arg1,
            ILogger arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IWebHookEventHandler field0;
        public readonly IWebHookEventValidator field1;
        public readonly ILogger field2;
    }


    public class UserModifiedState
    {
        public UserModifiedState(
            IUniqueUserService arg0,
            string arg1,
            UserFromConnOld arg2,
            UserFromConnCurrent arg3,
            string[] arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IUniqueUserService field0;
        public readonly string field1;
        public readonly UserFromConnOld field2;
        public readonly UserFromConnCurrent field3;
        public readonly string[] field4;
    }
}