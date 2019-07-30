using System.Linq;
using System.Reflection;
using Mail.Service;
using Conn;
using Conn.Adapter;
using RM;
using CurrencyRounding;
using Data;
using Databases;
using DryIoc;
using Entities;
using Framework;
using Logging;
using Logic;
using Organizations;
using Shared;
using Shop;
using Users.Repositories;
using Utilities;
using Web.Rest.API;
using CustomerDatabase = Databases.CustomerDatabase;
using IUserService = Conn.IUserService;

namespace LoadTest
{
    class Registrations
    {
        public static void RegisterTypes(IContainer container, bool singletonDecorators)
        {
            // These would normally be its own assemblies, but I filter types by namespace to mimic it
            RegUtilities(container);
            RegFramework(container);
            RegDatabase(container);
            RegShared(container);
            RegOrganizations(container);
            RegConnLoad(container);
            RegConnAdapterLoad(container);

            UsersIocModule.Load(container, singletonDecorators);
            RegUsers(container, singletonDecorators);
            RegC(container);
            CUsersIocModule.Load(container);
            RestApiIocModule.Load(container, singletonDecorators);
            RegisterLogging(container);
        }

        static void RegUtilities(IContainer builder)
        {
            builder.Register<IHttpUtility, HttpUtilityImpl>(Reuse.Singleton);
        }

        static void RegFramework(IContainer container)
        {
            // a default validator for types that do not have their own validator
            container.Register(typeof(IValidator<>), typeof(Validator<>), Reuse.Singleton);

            var ns = typeof(ITimeService).Namespace;

            var types = typeof(ITimeService).GetAssembly().GetLoadedTypes()
                .Where(i => i.Namespace == ns && !i.IsInterface && !i.IsAbstract && (i.Name.EndsWith("Service") || i.Name.EndsWith("Authorization")));

            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            container.Register<IMailClient, MailClient>(Reuse.Singleton);
            container.Register<IConfiguration, Configuration>(Reuse.Singleton);
            container.Register<IPasswordGenerator, PasswordGenerator>(Reuse.Singleton);
            container.Register(typeof(System.Data.Common.DbProviderFactory),
                made: Made.Of(() => System.Data.Common.DbProviderFactories.GetFactory("System.Data.SqlClient")),
                reuse: Reuse.Singleton
            );
        }

        static void RegDatabase(IContainer container)
        {
            container.Register<IMasterDatabase, MasterDatabase>(Reuse.Singleton);
            container.Register<ICustomerDatabase, CustomerDatabase>(Reuse.Singleton);
        }

        static void RegShared(IContainer container)
        {
            var ns = typeof(Country).Namespace;

            var types = typeof(IPsaContext).Assembly.GetLoadedTypes()
                .Where(i => i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                           (i.Name.EndsWith("Repository") || i.Name.EndsWith("Validator") ||
                            i.Name.EndsWith("Authorization") || i.Name.EndsWith("Builder") ||
                            i.Name.EndsWith("Service")));

            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            container.Register(typeof(ISharedAuthorization<>), typeof(SharedAuthorization<>), reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IDict>(made: Made.Of(() => Dict.Current()), reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IAppSettings, AppSettings>(reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IDistributorSettingsFactory, DistributorSettingsFactory>(reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);


            // Overrides
            container.Register<IPsaContextStorage, PsaContextStorage>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.Register<IContextService, PsaContextService>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.Register<IContextService<IPsaContext>, PsaContextService>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.Register<IPsaContextService, PsaContextService>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.Register<IContextService<ISharedContext>, PsaContextService>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            container.Register<IOrganizationContextScopeService, OrganizationContextScopeService>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.Register<OrganizationContextScopeService>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        }

        static void RegisterLogging(IContainer container)
        {
            container.Register<ILoggerConfiguration, LoggerConfiguration>(Reuse.Singleton);
            container.Register<IRGWebApiClientFactory, RGWebApiClientFactory>(Reuse.Singleton);
            container.Register<ILoggerFactory, LoggerFactory>(Reuse.Singleton);

            container.RegisterDelegate<ILogger>(resolverContext =>
            {
                var factory = resolverContext.Resolve<ILoggerFactory>();
                return factory.Create();
            });
        }

        static void RegUsers(IRegistrator container, bool singletonDecorators)
        {
            container.Register<Users.IUniqueUserService, Users.UniqueUserService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw, reuse: Reuse.Singleton);
            //container.Register<Users.IUniqueUserService, UniqueUserDependencyServiceDecorator>(setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            container.Register<IPsaUserService, PsaUserService>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            container.Register<IUserRepositoryController, UserRepositoryController>(reuse: Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IUserRepository, UserRepository>(reuse: Reuse.Singleton);
            container.Register<IUserRepository, UniqueUserDecoratorForUserRepository>(setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);
            container.Register<IUserRepository, UserChangeBlockerWhenExternallyOwned>(setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);

            //container.Register<IOrganizationUserRepository, OrganizationUserRepository>(reuse: Reuse.Singleton);
            container.Register<IOrganizationUserRepository, ConnOrganizationUserRepository>(setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);

            //container.Register<ITrustedOrganizationUserRepository, TrustedOrganizationUserRepository>(Reuse.Singleton);
        }

        static void RegOrganizations(IRegistrator container)
        {
            var ns = typeof(OrganizationBase.OrganizationEntity).Namespace;

            var types = typeof(OrganizationBase.OrganizationEntity).Assembly.GetLoadedTypes().Where(i =>
            {
                return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                       (i.Name.EndsWith("Repository") || i.Name.EndsWith("Service") || i.Name.EndsWith("Builder") ||
                        i.Name.EndsWith("Validator") || i.Name.EndsWith("Authorization"));
            }
            );

            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ns1 = typeof(Organizations.Organization).Namespace;

            types = typeof(Organizations.Organization).Assembly.GetLoadedTypes().Where(i =>
            {
                return i.Namespace == ns1 && !i.IsInterface && !i.IsAbstract &&
                       (i.Name.EndsWith("Repository") || i.Name.EndsWith("Service") || i.Name.EndsWith("Builder") ||
                        i.Name.EndsWith("Validator") || i.Name.EndsWith("Authorization"))
                       && i.Name != "UserRepository"
                       && i.Name != "UniqueUserDecoratorForUserRepository"
                       && i.Name != "UserChangeBlockerWhenExternallyOwned"
                       && i.Name != "ConnOrganizationUserRepository";
            }
            );

            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        }

        static void RegConnLoad(IRegistrator container)
        {
            container.Register<IPublicUserService, PublicUserService>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IUserService, Conn.UserService>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IUserClient, UserClient>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IConnConfiguration, ConnConfiguration>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IHttpClientFactory, HttpClientFactory>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IUserHttpClientFactory, UserHttpClientFactory>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IAccessTokenCache, AccessTokenCache>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IAccessTokenClient, AccessTokenClient>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
        }

        static void RegConnAdapterLoad(IRegistrator container)
        {
            container.Register<IConnClient, ConnClient>(reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IConnErrorFactory, ConnErrorFactory>(reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IConnPublicApiClientFactory, ConnPublicApiClientFactory>(
                reuse: Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IConnCertificateStorage, ConnCertificateStorage>(reuse: Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ns = typeof(IConnClient).Namespace;
            var types = typeof(IConnClient).Assembly.GetLoadedTypes().Where(i =>
            {
                return i.Namespace == ns && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Service");
            }
            );

            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == ns,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
        }

        static class UsersIocModule
        {
            public static void Load(IContainer builder, bool singletonDecorators)
            {
                RegisterImplementations(builder);
                RegisterRepositories(builder, singletonDecorators);
            }

            private static void RegisterImplementations(IRegistrator container)
            {
                var ns = typeof(Users.IUniqueUserService).Namespace;

                container.RegisterMany(typeof(Users.IUniqueUserService).GetAssembly().GetLoadedTypes().Where(i =>
                {
                    return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                           ((i.Name.EndsWith("Service") && i.Name != "UniqueUserService") ||
                            i.Name.EndsWith("Authorization")
                           );
                }), Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == ns,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw
                );
            }

            private static void RegisterRepositories(IRegistrator container, bool singletonDecorators)
            {
                var ns = typeof(EntityVersionRepository).Namespace;

                container.RegisterMany(
                    typeof(EntityVersionRepository).GetAssembly().GetLoadedTypes().Where(i =>
                    {
                        return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                               (i.Name.EndsWith("Service") || i.Name.EndsWith("Repository")) &&
                               i.Name != "UniqueUserRepository" &&
                               i.Name != "UniqueUserConnUpdateHandlerRepository" &&
                               i.Name != "UniqueUserUuidHandlerRepository";
                    }), Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == ns,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw
                );

                container
                    .Register<IUniqueUserToConnChangeNotifier,
                        UniqueUserToConnChangeNotifier>(Reuse.Singleton);

                container.Register<IUniqueUserRepository, UniqueUserRepository>(
                    Reuse.Singleton);
                container
                    .Register<IUniqueUserRepository,
                        UniqueUserConnUpdateHandlerRepository>(setup: Setup.Decorator,
                        reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);
                container.Register<IUniqueUserRepository, UniqueUserUuidHandlerRepository>(
                    setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);
                container
                    .Register<IUniqueUserRepository, UniqueUserToUserReplicator>(
                        setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);
            }
        }


        static void RegC(IRegistrator container)
        {
            var ns = typeof(RM.AccountService).Namespace;
            var types = typeof(RM.AccountService).Assembly.GetLoadedTypes().Where(i =>
            {
                return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                       (i.Name.EndsWith("Repository") || i.Name.EndsWith("Service") ||
                        i.Name.EndsWith("Validator") || i.Name.EndsWith("Authorization")
                       );
            }
            );
            container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);
        }

        public static class CUsersIocModule
        {
            public static void Load(IContainer builder)
            {
                RegisterApplicationServices(builder);
                RegisterDomain(builder);
                RegisterRepositories(builder);
            }

            static void RegisterApplicationServices(IRegistrator container)
            {
                var ns = typeof(CUsers.ApplicationServices.IUserService).Namespace;
                var types = typeof(CUsers.ApplicationServices.IUserService).Assembly.GetLoadedTypes().Where(
                    (i) => i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                           (i.Name.EndsWith("Service") || i.Name.EndsWith("Handler") || i.Name.EndsWith("Reader"))
                );

                container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: (s) => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }

            static void RegisterDomain(IRegistrator container)
            {
                var ns = typeof(CUsers.Domain.IUserEventRepository).Namespace;
                var types = typeof(CUsers.Domain.IUserEventRepository).Assembly.GetLoadedTypes().Where(
                    (i) => i.Namespace == ns && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Resolver")
                );

                container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: (s) => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }

            static void RegisterRepositories(IRegistrator container)
            {
                var ns = typeof(CUsers.Domain.UserEventRepository).Namespace;
                var types = typeof(CUsers.Domain.UserEventRepository).Assembly.GetLoadedTypes().Where(
                    (i) => i.Namespace == ns && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Repository")
                );

                container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: (s) => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }
        }


        public static class RestApiIocModule
        {
            public static void Load(IContainer container, bool singletonDecorators)
            {
                RegisterRestObjects(container);
                RegisterPdfObjects(container);
                RegisterLogicObjects(container, singletonDecorators);
                RegisterShopObjects(container, singletonDecorators);
                RegisterDataObjects(container);
                RegIntegrations(container);
                RegFinancials(container);
                RegisterConnWebHooks(container);
                RegisterMail(container);
                RegisterBackgroundTasks(container);
                RegisterScanner(container);
            }

            private static void RegisterBackgroundTasks(IContainer container)
            {
                BackgroundIocModule.Load(container);

                ScheduledWorkIocModule.Load(container);
            }

            private static class ScheduledWorkIocModule
            {
                public static void Load(IRegistrator container)
                {
                    var ns = typeof(ScheduledWork.ScheduledWorkService).Namespace;

                    container.RegisterMany(typeof(ScheduledWork.ScheduledWorkService).Assembly
                            .GetLoadedTypes().Where(i =>
                            {
                                return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                                       i.Name.EndsWith("Service");
                            }
                            ), Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == ns,
                        ifAlreadyRegistered: IfAlreadyRegistered.Throw
                    );
                }
            }

            private static class BackgroundIocModule
            {
                public static void Load(IContainer container)
                {
                    RegisterApplicationServices(container);
                }

                private static void RegisterApplicationServices(IRegistrator container)
                {
                    var theAssembly = typeof(Background.ScopedBackgroundTask).Assembly;
                    var ns = typeof(Background.ScopedBackgroundTask).Namespace;

                    var types = theAssembly.GetLoadedTypes()
                        .Where((t) =>
                            t.Namespace == ns && !t.IsAbstract && !t.IsInterface && t.Name.EndsWith("Service"));

                    container.RegisterMany(types, reuse: Reuse.Singleton, serviceTypeCondition: (s) => s.IsInterface && s.Namespace == ns,
                        ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                    // Register the scope wrapper
                    container.Register(typeof(Background.ScopedBackgroundTask), reuse: Reuse.Transient);
                }
            }

            private static void RegisterMail(IRegistrator container)
            {
                container.Register<IIncomingEmailHandler, AppIncomingEmailHandler>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }

            private static void RegIntegrations(IRegistrator container)
            {
                var types = Assembly.GetExecutingAssembly().GetLoadedTypes()
                    .Where((i) => i.Namespace == "Integrations" && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Service"));

                container.RegisterMany(types, serviceTypeCondition: (s) => s.IsInterface, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

                // Register providers
                var providerTypes = Assembly.GetExecutingAssembly().GetLoadedTypes().Where(
                    i => i.Namespace == "Integrations" && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Provider")
                );

                container.RegisterMany(providerTypes, Reuse.Scoped, serviceTypeCondition: s => s.IsInterface && s.Name != "ICalendarSyncProvider" && s.Name != "IIntegrationProvider", ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }

            private static void RegisterConnWebHooks(IRegistrator container)
            {
                var ns = typeof(Conn.Service.ConnWebHooksController).Namespace;
                container.RegisterMany(typeof(Conn.Service.ConnWebHooksController).Assembly.GetLoadedTypes()
                        .Where(type =>
                        {
                            return type.Namespace == ns && !type.IsInterface && !type.IsAbstract &&
                                   (type.Name.EndsWith("Service") || type.Name.EndsWith("Validator") ||
                                    type.Name.EndsWith("Handler")
                                   );
                        }
                        ), Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == ns,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw
                );
            }

            private static void RegisterShopObjects(IContainer container, bool singletonDecorators)
            {
                ShopIocModule.Load<DoNothingBilledPaymentPdfBuilder>(container, singletonDecorators);
            }

            private static void RegisterRestObjects(IRegistrator container)
            {
            }

            private static void RegisterPdfObjects(IContainer container)
            {
                var apiAssembly = typeof(Pdf.PdfDocumentService).Assembly;
                var ns = typeof(Pdf.PdfDocumentService).Namespace;

                container.RegisterMany(apiAssembly.GetLoadedTypes().Where(i =>
                {
                    return i.Namespace == ns && !i.IsInterface && !i.IsAbstract &&
                           (i.Name.EndsWith("Service") || i.Name.EndsWith("Definition")
                           );
                }
                    ), Reuse.Transient, serviceTypeCondition: s => s.IsInterface && s.Name != "IPdfXmlDefinitionBase" && s.Namespace == ns,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            }

            private static void RegFinancials(IContainer container)
            {
                var financialTypes = Assembly.GetExecutingAssembly().GetLoadedTypes().Where(
                    (i) => i.Namespace == "Financials" && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Service")
                );
                container.RegisterMany(
                    financialTypes,
                    Reuse.Transient,
                    serviceTypeCondition: (s) => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw
                );
            }

            private static void RegisterDataObjects(IContainer container)
            {
                var apiAssembly = typeof(ActivityRepository).Assembly;
                var ns = typeof(ActivityRepository).Namespace;
                var types = apiAssembly.GetLoadedTypes().Where(i =>
                {
                    return i.Namespace == ns && !i.IsInterface && !i.IsAbstract && i.Name.EndsWith("Repository");
                }
                );
                container.RegisterMany(types, Reuse.Singleton,
                    serviceTypeCondition: s => s.IsInterface && !s.Name.Contains("IEntityRepository") && s.Namespace == ns,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                // Check later
                container.RegisterInstance<IAuditTrail<Case>>(CaseAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<CaseMember>>(CaseMemberAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Task>>(TaskAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<TaskMember>>(TaskMemberAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Contact>>(ContactAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<BillingPlan>>(BillingPlanAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<CostCenterRevenue>>(CostCenterRevenueAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Entities.Invoice>>(InvoiceAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<InvoiceRow>>(InvoiceRowAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<InvoiceCase>>(InvoiceCaseAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Item>>(ItemAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<IReport>>(ExportAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Activity>>(ActivityAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<CommunicatesWith>>(CommunicatesWithAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Employment>>(EmploymentAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Right>>(RightAuditTrail.Definition);
                container.RegisterInstance<IAuditTrail<Framework.User>>(UserAuditTrail.Definition);
                container.Register<IAccessRightsHelper, AccessRightsHelper>(Reuse.Singleton);
            }

            private static void RegisterLogicObjects(IContainer container, bool singletonDecorators)
            {
                IocLogicModule.Load(container, singletonDecorators);
            }
        }

        public static void RegisterScanner(IContainer container)
        {
            container.Register<Scanner.IScannerService, Scanner.ScannerService>(Reuse.Singleton);
            container.Register<Scanner.ISettings, Scanner.Settings>(Reuse.Singleton);
        }


        public static class IocLogicModule
        {
            public static void Load(IContainer container, bool singletonDecorators)
            {
                var types = Assembly.GetExecutingAssembly().GetLoadedTypes().Where(i => i.Namespace == "Logic" && !i.IsInterface && !i.IsAbstract)
                    .ToArray();

                var serviceTypes = types.Where(i =>
                {
                    return i.Namespace == "Logic" && (i.Name.EndsWith("Repository") || i.Name.EndsWith("Builder") ||
                                                      i.Name.EndsWith("Factory")
                           );
                }
                ).ToArray();

                container.RegisterMany(serviceTypes, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                serviceTypes = types.Where(i =>
                {
                    return i.Namespace == "Logic" && (i.Name.EndsWith("Service") || i.Name.EndsWith("Validator") ||
                                                      i.Name.EndsWith("Authorization")
                           ) && !i.Name.Contains("LogEntryService") && i.Name != "GuidService" &&
                           i.Name != "ContactService" && i.Name != "BillingPlanService" &&
                           i.Name != "ContactCommunicationService" && i.Name != "InvoiceTaxBreakdownService";
                }
                ).ToArray();

                container.RegisterMany(serviceTypes, Reuse.Transient, serviceTypeCondition: s => s.IsInterface,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);
                container.Register<IGuidService, GuidService>(reuse: Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                container.Register<IWorkHourOverviewReportHandler, WorkHourOverviewReportHandler>(
                    reuse: Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                container.Register<IContactAnonymizer, ContactAnonymizer>(Reuse.Singleton);
                container.Register<IUserAnonymizer, UserAnonymizer>(Reuse.Singleton);
                container.Register<IContactCommunicationService, ContactCommunicationService>(
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                container.Register<IContactService, ContactService>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                container.Register<ICurrencyRoundingFactory, CurrencyRoundingFactory>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                container.Register<IOfferTemplateService, OfferTemplateService>(Reuse.Transient,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);
                container.Register<IFlextimeAdjustmentService, FlextimeAdjustmentService>(Reuse.Transient,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);

                container.Register<IGoogleDriveSettings, GoogleDriveSettings>(Reuse.Singleton);

                container.Register<IContactService, ContactAuditTrailServiceDecorator>(setup: Setup.Decorator,
                    reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                container.Register<IBillingPlanService, BillingPlanService>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                //container.Register<IBillingPlanService, BillingPlanAuditTrailServiceDecorator>(setup: Setup.Decorator,
                //    reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                //container.Register<IContactCommunicationService, ContactCommunicationAuditTrailServiceDecorator>(
                //    setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
                container.Register<IImageResizer, ImageResizer>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);
                container.Register<Logic.IUserService, Logic.UserService>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);
                container.Register<IInvoiceTaxBreakdownService, InvoiceTaxBreakdownService>(Reuse.Singleton,
                    ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            }
        }

        public static class ShopIocModule
        {
            public static void Load<BilledPaymentPdfBuilderImpl>(IContainer container, bool singletonDecorators)
                where BilledPaymentPdfBuilderImpl : IBilledPaymentPdfBuilder
            {
                var types = typeof(IOrganizationAddonService).Assembly.GetLoadedTypes().Where(i =>
                {
                    return i.Namespace == "Shop" && !i.IsInterface && !i.IsAbstract &&
                           (i.Name.EndsWith("Repository") || i.Name.EndsWith("Service") ||
                            i.Name.EndsWith("Resolver") || i.Name.EndsWith("Builder")
                           ) && i.Name != "PsaShopUserService" && i.Name != "DoNothingBilledPaymentPdfBuilder";
                }
                );

                container.RegisterMany(types, Reuse.Singleton, serviceTypeCondition: s => s.IsInterface && s.Namespace == "Shop",
                    ifAlreadyRegistered: IfAlreadyRegistered.Throw);

                container.Register<IBilledPaymentPdfBuilder, BilledPaymentPdfBuilderImpl>(Reuse.Singleton);
                //container.Register<IAddonActivationRepository, AddonActivationAppApiGuidRuleDecorator>(
                //    setup: Setup.Decorator, reuse: singletonDecorators ? Reuse.Singleton : Reuse.Transient);
            }
        }
    }
}