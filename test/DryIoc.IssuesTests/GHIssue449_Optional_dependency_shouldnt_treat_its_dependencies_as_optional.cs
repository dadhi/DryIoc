using NUnit.Framework;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue449_Optional_dependency_shouldnt_treat_its_dependencies_as_optional : ITest
    {
        public int Run()
        {
            Import_AllowDefault_DoesntImportUnregisteredDependency();
            Import_AllowDefault_DoesntImportServiceWithoutDependencies_without_MEF();
            Import_AllowDefault_DoesntImportServiceWithoutDependencies();
            Import_AllowDefault_ImportsServiceWithDependencies();
            Import_Lazy_with_AllowDefault_Should_not_check_into_dependencies();
            Import_Lazy_with_AllowDefault_Should_not_check_into_Lazy_dependencies_as_well();
            return 6;
        }

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

        [Test]
        public void Import_Lazy_with_AllowDefault_Should_not_check_into_dependencies()
        {
            var container = new Container().WithMef()
                .With(r => r.WithoutFuncAndLazyWithoutRegistration());

            container.Register<IHardDrive, SamsungHardDrive>();

            var x = new Computer3();
            container.InjectPropertiesAndFields(x);

            Assert.That(x.HardDrive, Is.Not.Null);   // it is not null because the registration of HardDrive is present but its dependencies are not checked yet (cause laziness)
            Assert.That(x.HardDrive.Value, Is.Null); // it is null because of allow default and we failing to get the dependency
        }

        [Test]
        public void Import_Lazy_with_AllowDefault_Should_not_check_into_Lazy_dependencies_as_well()
        {
            var container = new Container().WithMef();

            container.Register<IHardDrive, HitachiHardDrive>();

            var x = new Computer3();
            container.InjectPropertiesAndFields(x);

            Assert.That(x.HardDrive, Is.Not.Null); // it is not null because the registration of HardDrive is present but its dependencies are not checked yet (cause laziness)
            Assert.That(x.HardDrive.Value, Is.Null); // it is null because of allow default and we failing to get the dependency - even if dependency is lazy, we still check the registration
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
