using System;
using System.Collections.Generic;
using System.Linq;
using ImTools;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class CompositePatternTests
    {
        [Test]
        public void I_should_be_able_to_resolve_composite_of_enumerable_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfEnumerable), serviceKey: "composite");

            var composite = (PolygonOfEnumerable)container.Resolve<IShape>("composite");

            Assert.AreEqual(2, composite.Shapes.Count());
        }

        [Test]
        public void I_should_be_able_to_resolve_enumerable_of_funcs_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfEnumerable));

            var shapes = container.Resolve<IEnumerable<Func<IShape>>>();

            Assert.AreEqual(3, shapes.Count());
        }

        [Test]
        public void I_should_be_able_to_resolve_composite_of_array_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfArray), serviceKey: "composite");

            var composite = (PolygonOfArray)container.Resolve<IShape>("composite");

            Assert.AreEqual(2, composite.Shapes.Count());
        }

        [Test]
        public void I_should_be_able_to_resolve_default_composite_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle), serviceKey: "circle");
            container.Register(typeof(IShape), typeof(Square), serviceKey: "square");
            container.Register(typeof(IShape), typeof(PolygonOfArray));

            var composite = (PolygonOfArray)container.Resolve<IShape>();

            Assert.AreEqual(2, composite.Shapes.Count());
        }

        [Test]
        public void I_should_be_able_to_resolve_composite_as_item_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfEnumerable));

            var shapes = container.Resolve<IShape[]>();

            Assert.AreEqual(3, shapes.Count());
        }

        [Test]
        public void I_should_be_able_to_resolve_composite_of_many_as_item_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfMany), serviceKey: "composite");

            var polygon = (PolygonOfMany)container.Resolve<IShape>("composite");
            Assert.AreEqual(2, polygon.Shapes.Count());

            var shapes = container.Resolve<LazyEnumerable<IShape>>().Items;
            Assert.AreEqual(3, shapes.Count());
        }

        [Test]
        public void No_recursion_with_required_type()
        {
            var container = new Container();
            container.Register<IOperation, OperationOne>(serviceKey: nameof(OperationOne));
            container.Register<IOperation, OperationTwo>(serviceKey: nameof(OperationTwo));

            container.RegisterMany(typeof(CompositeOperation).One(), _ => new[]{ typeof(IOperation), typeof(IAction) });
            container.Register<Service>(made: Parameters.Of.Type<IAction>(typeof(IOperation)));

            var service = container.Resolve<Service>();
            Assert.IsNotNull(service);
            Assert.IsInstanceOf<CompositeOperation>(service.Action);
        }

        [Test]
        public void No_recursion_with_required_type_in_lazy_enumerable()
        {
            var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());
            container.Register<IOperation, OperationOne>(serviceKey: nameof(OperationOne));
            container.Register<IOperation, OperationTwo>(serviceKey: nameof(OperationTwo));

            container.RegisterMany(typeof(CompositeOperation).One(), _ => new[] { typeof(IOperation), typeof(IAction) });
            container.Register<Service>(made: Parameters.Of.Type<IAction>(typeof(IOperation)));

            var service = container.Resolve<Service>();
            Assert.IsNotNull(service);
            Assert.IsInstanceOf<CompositeOperation>(service.Action);
        }

        public interface IAction
        {
        }

        public interface IOperation : IAction
        {
        }

        public class OperationOne : IOperation
        {
        }

        public class OperationTwo : IOperation
        {
        }

        public class CompositeOperation : IOperation
        {
            public readonly IEnumerable<IOperation> Operations;

            public CompositeOperation(IEnumerable<IOperation> operations)
            {
                Operations = operations.ToArray();
            }
        }

        public class Service
        {
            public readonly IAction Action;

            public Service(IAction action)
            {
                Action = action;
            }
        }

        #region Composite pattern CUT

        public interface IShape
        {
        }

        public class Circle : IShape
        {
        }

        public class Square : IShape
        {
        }

        public class PolygonOfEnumerable : IShape
        {
            public IEnumerable<IShape> Shapes { get; set; }

            public PolygonOfEnumerable(IEnumerable<IShape> shapes)
            {
                Shapes = shapes;
            }
        }

        public class PolygonOfMany : IShape
        {
            public IEnumerable<IShape> Shapes { get; set; }

            public PolygonOfMany(LazyEnumerable<IShape> shapes)
            {
                Shapes = shapes;
            }
        }

        public class PolygonOfArray : IShape
        {
            public IShape[] Shapes { get; set; }

            public PolygonOfArray(IShape[] shapes)
            {
                Shapes = shapes;
            }
        }

        #endregion
    }
}
