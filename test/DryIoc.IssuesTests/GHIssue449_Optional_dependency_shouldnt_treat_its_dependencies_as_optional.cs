using NUnit.Framework;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue449_Optional_dependency_shouldnt_treat_its_dependencies_as_optional
    {
        [Test]
        public void Import_AllowDefault_DoesntImportUnregisteredDependency()
        {
            var container = new Container().WithMef();

            var x = new Computer();
            container.InjectPropertiesAndFields(x);

            // no hard drive registered
            Assert.IsNull(x.HardDrive);
        }

        [Test]
        public void Import_AllowDefault_DoesntImportServiceWithoutDependencies_without_MEF()
        {
            var container = new Container();
            container.Register<Computer2>();
            container.Register<IHardDrive, SamsungHardDrive2>();

            var x = container.Resolve<Computer2>();

            Assert.IsNull(x.HardDrive);
        }

        [Test] // doesn't work as expected
        public void Import_AllowDefault_DoesntImportServiceWithoutDependencies()
        {
            var container = new Container().WithMef();
            container.Register<IHardDrive, SamsungHardDrive>();

            var x = new Computer();
            container.InjectPropertiesAndFields(x);

            // should be null: couldn't assemble a computer with a hard drive
            // because logic board for the hard drive is not registered
            Assert.IsNull(x.HardDrive);
        }

        [Test]
        public void Import_AllowDefault_ImportsServiceWithDependencies()
        {
            var container = new Container().WithMef();
            container.Register<IHardDrive, SamsungHardDrive>();
            container.Register<ILogicBoard, CirrusLogicBoard>();

            var x = new Computer();
            container.InjectPropertiesAndFields(x);

            // both the hard drive and the logic board are registered
            Assert.IsNotNull(x.HardDrive);
            Assert.IsNotNull(x.HardDrive.LogicBoard);
        }

        public class Computer
        {
            [Import(AllowDefault = true)]
            public IHardDrive HardDrive { get; set; }
        }

        public class Computer2
        {
            public IHardDrive HardDrive { get; }
            public Computer2(IHardDrive hardDrive = null) => HardDrive = hardDrive;
        }

        public interface IHardDrive
        {
            ILogicBoard LogicBoard { get; }
        }

        public class SamsungHardDrive : IHardDrive
        {
            [Import]
            public ILogicBoard LogicBoard { get; set; }
        }

        public class SamsungHardDrive2 : IHardDrive
        {
            public ILogicBoard LogicBoard { get; }
            public SamsungHardDrive2(ILogicBoard logicBoard) => LogicBoard = logicBoard;
        }

        public interface ILogicBoard { }

        public class CirrusLogicBoard : ILogicBoard { }
    }
}
