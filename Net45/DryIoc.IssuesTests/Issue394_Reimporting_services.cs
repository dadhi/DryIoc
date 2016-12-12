using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue394_Reimporting_services
    {
        [Test]
        public void InjectPropertiesAndFields_imports_new_services_into_resolved_singleton()
        {
            // registered on application startup
            var container = new Container().WithMef();

            container.Register<Aggregator>(Reuse.Singleton);
            container.Register<IAggregatee, Agg1>(Reuse.Transient);

            // used later
            var aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(1, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg2>(Reuse.Transient);
            container.InjectPropertiesAndFields(aggregator);
            Assert.AreEqual(2, aggregator.Aggregatees.Length);
        }

        [Test, Ignore("fails")]
        public void InjectPropertiesAndFields_imports_new_services_into_an_instance()
        {
            // registered on application startup
            var container = new Container().WithMef()
                .With(rules => rules.WithDefaultReuseInsteadOfTransient(Reuse.Transient));

            container.Register<IAggregatee, Agg1>();

            // used later
            var aggregator = new Aggregator();
            container.InjectPropertiesAndFields(aggregator);
            Assert.AreEqual(1, aggregator.Aggregatees.Length);

            var expr = container.Resolve<LambdaExpression>(typeof(IEnumerable<IAggregatee>));

            // registered new service and re-imported
            container.Register<IAggregatee, Agg2>();

            // should make test pass
            container.ClearCache<IAggregatee[]>(FactoryType.Wrapper);
            container.ClearCache<IAggregatee[]>(FactoryType.Decorator);
            container.ClearCache<LambdaExpression>(FactoryType.Wrapper);

            var expr2 = container.Resolve<LambdaExpression>(typeof(IEnumerable<IAggregatee>));

            container.InjectPropertiesAndFields(aggregator);
            Assert.AreEqual(2, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg3>();
            container.InjectPropertiesAndFields(aggregator);
            Assert.AreEqual(3, aggregator.Aggregatees.Length);
        }

        [Test, Ignore("fails")]
        public void Resolve_imports_new_services()
        {
            // registered on application startup
            var container = new Container().WithMef();

            container.Register<Aggregator>(Reuse.Transient);
            container.Register<IAggregatee, Agg1>();

            // used later
            var aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(1, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg2>();
            aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(2, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg3>();
            aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(3, aggregator.Aggregatees.Length);
        }

        [Test, Ignore("fails")]
        public void Resolve_imports_new_services_WithoutCache()
        {
            // registered on application startup
            var container = new Container().WithMef();
            container.Register<Aggregator>();
            container.Register<IAggregatee, Agg1>();

            // used later
            var aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(1, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg2>();
            container = container.WithoutCache();
            aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(2, aggregator.Aggregatees.Length);

            // registered new service and re-imported
            container.Register<IAggregatee, Agg3>();
            container = container.WithoutCache();
            aggregator = container.Resolve<Aggregator>();
            Assert.AreEqual(3, aggregator.Aggregatees.Length);
        }

        public class Aggregator
        {
            [ImportMany]
            public IAggregatee[] Aggregatees { get; set; }
        }

        public interface IAggregatee { }

        public class Agg1 : IAggregatee { }

        public class Agg2 : IAggregatee { }

        public class Agg3 : IAggregatee { }
    }
}
