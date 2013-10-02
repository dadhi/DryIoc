namespace DryIoc.CompileTimeOperationTests
{
    using DryIoc;
    using AR = AttributedRegistrator;

    public static class CompileTimeGeneratedRegistrator
    {
        public static void RegisterIn(IRegistrator registrator)
        {
            registrator.RegisterExported(new[]
            {
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.TransientService),
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ITransientService), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.SingletonService),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISingletonService), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.SingletonOpenGenericService<>),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IOpenGenericService<>), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.TransientOpenGenericService<>),
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.TransientOpenGenericService<>), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.DependentService),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.DependentService), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.OneTransientService),
                    IsSingleton = false,
                    MetadataAttributeIndex = 0,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherTransientService),
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.OneServiceWithMetadata),
                    IsSingleton = true,
                    MetadataAttributeIndex = 0,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherServiceWithMetadata),
                    IsSingleton = false,
                    MetadataAttributeIndex = 1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.YetAnotherServiceWithMetadata),
                    IsSingleton = false,
                    MetadataAttributeIndex = 2,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.SingleServiceWithMetadata),
                    IsSingleton = true,
                    MetadataAttributeIndex = 0,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.SingleServiceWithMetadata), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithImportedCtorParameter),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithImportedCtorParameter), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.NamedService),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.INamedService), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherNamedService),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.INamedService), ServiceName = "blah" },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.DbMan),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISomeDb), ServiceName = null },
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IAnotherDb), ServiceName = null },
                    }
                },
                new RegistrationInfo {
                    ImplementationType = typeof(DryIoc.UnitTests.CUT.DbMan<>),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    FactorySetupInfo = null,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISomeDb<>), ServiceName = null },
                    }
                },
            });
        }
    }
}