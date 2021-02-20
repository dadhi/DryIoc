using DryIoc.MefAttributedModel;
using NUnit.Framework;
using System;
using System.ComponentModel.Composition;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue376_MessedUpStackTrace
    {
        [Test]
        public void Test()
        {
            var c = new Container().WithMef();
            c.RegisterExports(typeof(GHIssue376_MessedUpStackTrace));

            // this line should be the top stack frame
            const string expectedCallFrame = "DryIoc.IssuesTests.GHIssue376_MessedUpStackTrace.ThrowMyExportException()";

            // normal call
            try
            {
                ThrowMyExportException();
            }
            catch (MyExportException ex)
            {
                Assert.IsNotNull(ex.StackTrace);
                Assert.IsTrue(ex.StackTrace.Trim().Contains(expectedCallFrame));
            }

            // imported action call
            try
            {
                var action = c.Resolve<Action>(nameof(MyExportAttribute));
                action();
            }
            catch (MyExportException ex)
            {
                Assert.IsNotNull(ex.StackTrace);
                Assert.IsTrue(ex.StackTrace.Trim().Contains(expectedCallFrame),
                    "Stack trace should have this stack frame:\n{0},\nbut it doesnt. Stack trace:\n{1}",
                    expectedCallFrame, ex.StackTrace);
            }
        }

        [MetadataAttribute, AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class MyExportAttribute : ExportAttribute
        {
            public MyExportAttribute() : base(nameof(MyExportAttribute))
            {
            }
        }

        class MyExportException : Exception
        { 
        }

        [MyExport]
        public void ThrowMyExportException()
        {
            throw new MyExportException();
        }
    }
}
