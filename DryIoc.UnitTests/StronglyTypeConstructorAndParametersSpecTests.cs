using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class StronglyTypeConstructorAndParametersSpecTests
    {
        [Test]
        public void Specify_default_constructor_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(() => new Burger()));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_constructor_with_params_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(() => new Burger(default(ICheese))));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Not.Null);
        }

        [Test]
        public void Specify_parameter_ifUnresolved_bahavior_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(() => new Burger(Param.Of<ICheese>(IfUnresolved.ReturnDefault))));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test, Ignore]
        public void Specify_primitive_parameter_value_directly()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(() => new Burger("King", Param.Of<ICheese>())));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        internal interface ICheese { }

        internal class BlueCheese : ICheese { }

        internal class Burger
        {
            public readonly string Name;

            public ICheese Cheese { get; private set; }

            public Burger() {}

            public Burger(ICheese cheese)
            {
                Cheese = cheese;
            }

            public Burger(string name, ICheese cheese)
            {
                Name = name;
                Cheese = cheese;
            }
        }
    }

    public static class Construct
    {
        public static InjectionRules Of<TImpl>(Expression<Func<TImpl>> newImpl)
        {
            var newExpr = (newImpl.Body as NewExpression).ThrowIfNull();
            var ctor = newExpr.Constructor;
            var pars = ctor.GetParameters();
            var args = newExpr.Arguments;
            var parameters = Parameters.Of;
            if (args.Count != 0)
            {
                for (var i = 0; i < args.Count; i++)
                {
                    var par = pars[i];
                    var arg = args[i] as MethodCallExpression;
                    if (arg != null && arg.Method.DeclaringType == typeof(Param))
                    {
                        var ifUnresolved = IfUnresolved.Throw;

                        var settings = arg.Arguments;
                        for (var j = 0; j < settings.Count; j++)
                        {
                            var setting = settings[i] as ConstantExpression;
                            if (setting != null)
                            {
                                if (setting.Type == typeof(IfUnresolved))
                                    ifUnresolved = (IfUnresolved)setting.Value;
                            }
                        }

                        parameters = parameters.Condition(par.Equals, ifUnresolved: ifUnresolved);
                    }
                    else
                    {
                        var argValue = args[i] as ConstantExpression;
                        if (argValue != null && argValue.Type.IsPrimitive())
                        {

                        }
                    }
                }
            }

            return InjectionRules.With(request => FactoryMethod.Of(ctor), parameters);
        }
    }

    public static class Param
    {
        public static T Of<T>()
        {
            return default(T);
        }

        public static T Of<T>(IfUnresolved ifUnresolved)
        {
            return default(T);
        }

        public static T AllowDefault<T>()
        {
            return default(T);
        }
    }
}
