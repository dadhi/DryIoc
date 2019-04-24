using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue116_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution
    {
        [Test]
        public async Task DryIoc_Resolve_parallel_execution()
        {
            var container = new Container();
            container.Register(typeof(IQuery<MyQuery.Request, MyQuery.Response>), typeof(MyQuery));
            container.Register(typeof(IQuery<,>), typeof(QueryDecorator<,>), setup: Setup.Decorator);

            var tasks = new List<Task>
            {
                Task.Run(() => container.Resolve<IQuery<MyQuery.Request, MyQuery.Response>>()),
                Task.Run(() => container.Resolve<IQuery<MyQuery.Request, MyQuery.Response>>())
            };
            await Task.WhenAll(tasks);
        }
    }

    public interface IQuery<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
    }

    public class MyQuery : IQuery<MyQuery.Request, MyQuery.Response>
    {
        public class Request { }
        public class Response { }
    }

    public class QueryDecorator<TRequest, TResponse> : IQuery<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        readonly IQuery<TRequest, TResponse> _query;

        public QueryDecorator(IQuery<TRequest, TResponse> query)
        {
            if (query.GetType() == GetType())
                throw new Exception("Decorator resolved itself"); // We should never get here

            _query = query;
        }
    }
}
