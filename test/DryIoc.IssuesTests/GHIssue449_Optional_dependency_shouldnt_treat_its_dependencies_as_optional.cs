using NUnit.Framework;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using System;

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

        [Test]
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

        // [Test] // doesn't work as expected
        public void Import_LazyAllowDefault_DoesntImportServiceWithoutDependencies()
        {
            var container = new Container().WithMef();
            // .With(r => r.WithoutFuncAndLazyWithoutRegistration());

            // container.Register<IHardDrive, HitachiHardDrive>();
            container.Register<IHardDrive, SamsungHardDrive>();

            var x = new Computer3();
            container.InjectPropertiesAndFields(x);

            //var h = x.HardDrive;
            // should be null: couldn't assemble a computer with a hard drive
            // because logic board for the hard drive is not registered
            Assert.IsNull(x.HardDrive);

            // instead, we currently have this situation:
            //Assert.That(x.HardDrive, Is.Not.Null);
            //Assert.That(x.HardDrive.Value, Is.Not.Null);
            //Assert.That(() => x.HardDrive.Value.LogicBoard, Throws.Exception.TypeOf<NullReferenceException>());
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

        public class Computer3
        {
            [Import(AllowDefault = true)]
            public Lazy<IHardDrive> HardDrive { get; set; }
        }

        public class HitachiHardDrive : IHardDrive
        {
            [Import] // this dependency is missing
            public Lazy<ILogicBoard> LazyLogicBoard { get; set; }

            public ILogicBoard LogicBoard
            {
                get
                {
                    return LazyLogicBoard.Value;
                }
            }
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
