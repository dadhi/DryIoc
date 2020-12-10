using NUnit.Framework;
using DryIoc.Messages;
using System.Threading.Tasks;
using System.Threading;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue____DryIoc_Message_SO
    {
        [Test, Ignore("fixme")]
        public void Contravariant_handler_should_be_Resolved_the_same_as_for_ResolveMany()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowOnRegisteringDisposableTransient());
            container.RegisterMany(new[] { typeof(GetInformationHandler).Assembly }, Registrator.Interfaces, made: PropertiesAndFields.Auto);
            container.Register<MessageMediator>();

            var handlers = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>[]>();
            Assert.AreEqual(1, handlers.Length);

            var result = handlers[0].Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);

            var handler = container.Resolve<IMessageHandler<PermissionedGetInformationRequest, DataIWantView>>();
            result = handler.Handle(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(result);

            var m = container.Resolve<MessageMediator>();
            var task = m.Send<PermissionedGetInformationRequest, DataIWantView>(new PermissionedGetInformationRequest(), default);
            Assert.IsNotNull(task);
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
