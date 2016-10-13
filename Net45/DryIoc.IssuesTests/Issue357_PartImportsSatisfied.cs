using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    /// <summary>
    /// Issue #357: IPartImportsSatisfiedNotification support fails for interfaces
    /// </summary>
    [TestFixture]
    public class Issue357_PartImportsSatisfied
    {
        private IContainer Container { get; } = CreateContainer();

        private static IContainer CreateContainer()
        {
            var c = new Container().WithMefAttributedModel();

            c.RegisterExports(new[] { typeof(Issue357_PartImportsSatisfied).GetAssembly() });
            return c;
        }

        [Test]
        public void PartImportsSatisfiedNotification_is_supported_for_classes()
        {
            var myClass = Container.Resolve<MyClass>();
            Assert.IsTrue(myClass.ImportsSatisfied);

            var notMyClass = Container.Resolve<NotMyClass>();
            Assert.IsTrue(notMyClass.ImportsSatisfied);
        }

        [Test]
        public void PartImportsSatisfiedNotification_is_supported_for_interfaces()
        {
            var notMyClass = Container.Resolve<ICheckImportsSatisfied>();
            Assert.IsTrue(notMyClass.ImportsSatisfied);
        }

        public interface ICheckImportsSatisfied
        {
            bool ImportsSatisfied { get; }
        }

        [Export]
        public class MyClass : IPartImportsSatisfiedNotification
        {
            public void OnImportsSatisfied()
            {
                ImportsSatisfied = true;
            }

            public bool ImportsSatisfied { get; private set; }
        }

        [Export, Export(typeof(ICheckImportsSatisfied))]
        public class NotMyClass : IPartImportsSatisfiedNotification, ICheckImportsSatisfied
        {
            public void OnImportsSatisfied()
            {
                ImportsSatisfied = true;
            }

            public bool ImportsSatisfied { get; private set; }
        }
    }
}
