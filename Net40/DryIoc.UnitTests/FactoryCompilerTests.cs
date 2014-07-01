using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FactoryCompilerTests
    {
        [Test]
        public void Compile_delegate_with_nested_lambda()
        {
            Expression<FactoryDelegate> factory = 
                (_, __) => new Func<Scope, object>(x => x.GetOrAdd(0, () => "a")).Invoke(new Scope());

            var factoryDelegate = factory.Body.CompileToDelegate();
            var result = (string)factoryDelegate.Invoke(null, null);

            Assert.That(result, Is.EqualTo("a"));
        }
    }
}
