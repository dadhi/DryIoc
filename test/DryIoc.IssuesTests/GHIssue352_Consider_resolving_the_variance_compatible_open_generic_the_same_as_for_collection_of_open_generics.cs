using NUnit.Framework;
using DryIoc.Messages;
using System.Threading.Tasks;
using System.Threading;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue352_Consider_resolving_the_variance_compatible_open_generic_the_same_as_for_collection_of_open_generics : ITest
    {
        public int Run()
        {
            Contravariant_handler_can_be_Resolved_with_a_single_Resolve();
            Contravariant_handler_can_be_Resolved_with_a_single_Resolve_even_with_MS_DI_rules();
            Contravariant_handler_should_be_Resolved_in_collection();
            Contravariant_handler_should_be_Resolved_in_collection_and_the_variant_resolve_should_not_affect_it();
            return 4;
        }

        [Test]
        public void Contravariant_handler_can_be_Resolved_with_a_single_Resolve()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithVariantGenericTypesInResolve()
                .WithoutThrowOnRegisteringDisposableTransient());

            container.RegisterMany(new[] { typeof(GetInformationHandler).Assembly }, Registrator.Interfaces, made: PropertiesAndFields.Auto);
            container.Register<MessageMediator>();

            // But this does not work!
            var handler = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>>();
            var result = handler.Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);

            // and this does not work!
            var m = container.Resolve<MessageMediator>();
            var task = m.Send<PermissionedGetInformationRequest, DataIWantView>(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(task);
        }

        [Test]
        public void Contravariant_handler_can_be_Resolved_with_a_single_Resolve_even_with_MS_DI_rules()
        {
            var container = new Container(rules =>
                DryIocAdapter.WithMicrosoftDependencyInjectionRules(rules)
                .WithVariantGenericTypesInResolve());

            container.RegisterMany(new[] { typeof(GetInformationHandler).Assembly }, Registrator.Interfaces, made: PropertiesAndFields.Auto);
            container.Register<MessageMediator>();

            // But this does not work!
            var handler = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>>();
            var result = handler.Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);

            // and this does not work!
            var m = container.Resolve<MessageMediator>();
            var task = m.Send<PermissionedGetInformationRequest, DataIWantView>(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(task);
        }

        [Test]
        public void Contravariant_handler_should_be_Resolved_in_collection()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowOnRegisteringDisposableTransient());

            container.RegisterMany(new[] { typeof(GetInformationHandler).Assembly }, Registrator.Interfaces, made: PropertiesAndFields.Auto);

            // This works.
            var handlers = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>[]>();
            Assert.AreEqual(1, handlers.Length);
            var result = handlers[0].Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);
        }

        [Test]
        public void Contravariant_handler_should_be_Resolved_in_collection_and_the_variant_resolve_should_not_affect_it()
        {
            var container = new Container(rules => rules
                .WithVariantGenericTypesInResolve()
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowOnRegisteringDisposableTransient());

            container.RegisterMany(new[] { typeof(GetInformationHandler).Assembly }, Registrator.Interfaces, made: PropertiesAndFields.Auto);

            // This works.
            var handlers = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>[]>();
            Assert.AreEqual(1, handlers.Length);
            var result = handlers[0].Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);
        }

        public class DataIWantView { }
        public class GetInformationRequest : IMessage<DataIWantView>
        {
        }

        public class PermissionedGetInformationRequest : GetInformationRequest
        {
            public int RequesterId { get; set; }
        }

        public class GetInformationHandler : IMessageHandler<GetInformationRequest, DataIWantView>
        {
            public Task<DataIWantView> Handle(GetInformationRequest request, CancellationToken cancellationToken)
            {
                if (request is PermissionedGetInformationRequest permissionedRequest)
                {
                    return Task.FromResult(new DataIWantView());
                }

                return null;
            }
        }
    }
}
