using System;
using System.Collections.Generic;
using System.Linq;
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
			container.Register(typeof(IShape), typeof(PolygonOfEnumerable), named: "composite");

			var composite = (PolygonOfEnumerable)container.Resolve<IShape>("composite");

            Assert.That(composite.Shapes.Count(), Is.EqualTo(2));
		}

        [Test]
        public void I_should_be_able_to_resolve_enumerable_of_funcs_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfEnumerable));

            var shapes = container.Resolve<IEnumerable<Func<IShape>>>();

            Assert.That(shapes.Count(), Is.EqualTo(3));
        }

        [Test]
        public void I_should_be_able_to_resolve_composite_of_array_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfArray), named: "composite");

            var composite = (PolygonOfArray)container.Resolve<IShape>("composite");

            Assert.That(composite.Shapes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void I_should_be_able_to_resolve_default_composite_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle), named: "circle");
            container.Register(typeof(IShape), typeof(Square), named: "square");
            container.Register(typeof(IShape), typeof(PolygonOfArray));

            var composite = (PolygonOfArray)container.Resolve<IShape>();

            Assert.That(composite.Shapes.Count(), Is.EqualTo(2));
        }

		[Test]
		public void I_should_be_able_to_resolve_composite_as_item_without_exception()
		{
			var container = new Container();
			container.Register(typeof(IShape), typeof(Circle));
			container.Register(typeof(IShape), typeof(Square));
			container.Register(typeof(IShape), typeof(PolygonOfEnumerable));

			var shapes = container.Resolve<IShape[]>();

			Assert.That(shapes.Count(), Is.EqualTo(3));
		}

        [Test]
        public void I_should_be_able_to_resolve_composite_of_many_as_item_without_exception()
        {
            var container = new Container();
            container.Register(typeof(IShape), typeof(Circle));
            container.Register(typeof(IShape), typeof(Square));
            container.Register(typeof(IShape), typeof(PolygonOfMany), named: "composite");

            var polygon = (PolygonOfMany)container.Resolve<IShape>("composite");
            Assert.That(polygon.Shapes.Count(), Is.EqualTo(2));

            var shapes = container.Resolve<LazyEnumerable<IShape>>().Items;
            Assert.That(shapes.Count(), Is.EqualTo(3));
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
