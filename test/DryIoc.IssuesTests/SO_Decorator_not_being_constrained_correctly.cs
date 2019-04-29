using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_Decorator_not_being_constrained_correctly
    {
        [Test]
        public void Test()
        {
            var c = new Container();


            c.Register(typeof(IRequestHandler<,>), typeof(RetryOnConcurrencyRequestHandlerDecorator<,>), setup: Setup.Decorator);


        }
    }

    public class RetryOnConcurrencyRequestHandlerDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IRetryOnConflict
    {
        private readonly IConcurrencyRetryPolicy retryPolicy;
        private readonly IRequestHandler<TRequest, TResponse> innerHandler;

        public RetryOnConcurrencyRequestHandlerDecorator(IRequestHandler<TRequest, TResponse> innerHandler, IConcurrencyRetryPolicy retryPolicy)
        {
            this.innerHandler = innerHandler;
            this.retryPolicy = retryPolicy;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken) => 
            retryPolicy.Execute(() => innerHandler.Handle(request, cancellationToken));
    }

    public interface IRetryOnConflict
    {
    }

    public class GetSaleRegistration : IRequest<SaleRegistration>
    {
    }

    public class SaleRegistration { }

    public interface IConcurrencyRetryPolicy
    {
        Task<TResponse> Execute<TResponse>(Func<Task<TResponse>> func);
    }

    class ConcurrencyRetryPolicy : IConcurrencyRetryPolicy
    {
        public Task<TResponse> Execute<TResponse>(Func<Task<TResponse>> func) => Task.FromResult(default(TResponse));
    }
}
