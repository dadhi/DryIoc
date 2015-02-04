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

            container.Register<Burger>(with: Construct.Of(_ => new Burger()));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_constructor_with_params_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(_ => new Burger(default(ICheese))));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Not.Null);
        }

        [Test]
        public void Specify_parameter_ifUnresolved_bahavior_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(p => new Burger(p.Of<ICheese>(IfUnresolved.ReturnDefault))));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_primitive_parameter_value_directly()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(p => new Burger("King", p.Of<ICheese>())));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Specify_allow_default_for_unresovled_service()
        {
            var container = new Container();

            container.Register<Burger>(with: Construct.Of(p => new Burger("King", p.AllowDefault<ICheese>())));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        internal interface ICheese { }

        internal class BlueCheese : ICheese { }

        internal class Burger
        {
            public readonly string Name;

            public ICheese Cheese { get; private set; }

            public Burger() { }

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
        public static InjectionRules Of<TImpl>(Expression<Func<Params, TImpl>> newImpl)
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
                    if (arg != null && arg.Method.DeclaringType == typeof(Params))
                    {
                        var ifUnresolved = arg.Method.Name == Params.ALLOW_DEFAULT 
                            ? IfUnresolved.ReturnDefault : IfUnresolved.Throw;

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
                            parameters = parameters.Name(par.Name, argValue.Value);
                        }
                    }
                }
            }

            return InjectionRules.With(request => FactoryMethod.Of(ctor), parameters);
        }
    }

    public sealed class Params
    {
        public static readonly Params Default = new Params();

        public T Of<T>() { return default(T); }

        public T Of<T>(IfUnresolved ifUnresolved) { return default(T); }

        public static readonly string ALLOW_DEFAULT = "AllowDefault";
        public T AllowDefault<T>() { return default(T); }

        private Params() { }
    }
}
