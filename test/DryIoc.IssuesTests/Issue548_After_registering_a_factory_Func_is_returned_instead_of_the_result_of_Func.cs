using NUnit.Framework;
using static FastExpressionCompiler.LightExpression.Expression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func
    {
        [Test]
        public void Test()
        {
            var c = new Container(rules => rules.
                WithUnknownServiceResolvers(request =>
                {
                    var optsType = typeof(IOptions<>).MakeGenericType(request.ServiceType);
                    if (!request.Container.IsRegistered(optsType))
                        return null;

                    var opts = request.Container.Resolve(optsType);
                    return new ExpressionFactory(_ => Property(Constant(opts), optsType.Property("Value")));
                }));

            c.RegisterInstance<IOptions<Foo>>(new Options<Foo> { Value = new Foo() });
            c.Register<FooUser>();

            var fooUser = c.Resolve<FooUser>();
            Assert.IsNotNull(fooUser.Foo);
        }

        public interface IOptions<T> { T Value { get; } }

        class Options<T> : IOptions<T>
        {
            public T Value { get; set; }
        }

        public class Foo { }

        public class FooUser
        {
            public Foo Foo { get; }
            public FooUser(Foo foo)
            {
                Foo = foo;
            }
        }
    }
}
