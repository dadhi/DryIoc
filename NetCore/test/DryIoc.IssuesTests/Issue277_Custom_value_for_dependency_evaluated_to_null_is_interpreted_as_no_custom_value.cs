using System;
using DryIoc.IssuesTests;
using NUnit.Framework;

namespace DryIoc
{
    [TestFixture]
    public class Issue277_Custom_value_for_dependency_evaluated_to_null_is_interpreted_as_no_custom_value
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register(Made.Of(() => Issue_InjectingSerilogLogger.Log.ForContext(Arg.Index<Type>(0)),
                r => r.Parent.ImplementationType));

            // When ImplementationType is null fails trying to resolve the Type parameter
            c.Resolve<Issue_InjectingSerilogLogger.ILogger>();
        }
    }
}
