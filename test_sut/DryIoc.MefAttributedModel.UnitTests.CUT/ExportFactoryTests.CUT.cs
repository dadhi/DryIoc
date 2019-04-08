using System;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class OrangeFactory
    {
        [Export]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [ExportMany]
    public class FruitFactory
    {
        [Export]
        public Orange CreateOrange()
        {
            return new Orange();
        }

        [Export]
        public Apple CreateApple()
        {
            return new Apple();
        }
    }

    [Export]
    public class NamedFruitFactory
    {
        [Export("orange")]
        public Orange CreateOrange()
        {
            return new Orange();
        }

        [Export("apple")]
        public Apple CreateApple()
        {
            return new Apple();
        }
    }

    [Export]
    public class TransientOrangeFactory
    {
        [Export, TransientReuse]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [Export]
    public class FuncFactory
    {
        [Export]
        public Func<string, Orange> Create()
        {
            return CreateOrange;
        }

        public Orange CreateOrange(string s)
        {
            return new Orange();
        }
    }

    public class Orange { }
    public class Apple { }

    public class Duck {}
    public class Chicken {}

    [Export]
    public class BirdFactory
    {
        [Export]
        public static Duck GetDuck()
        {
            return new Duck();
        }

        [Export]
        public static Chicken Chicken { get { return new Chicken(); } }
    }

    public static class StaticBirdFactory
    {
        [Export]
        public static readonly Duck Duck = new Duck();

        [Export]
        public static Chicken Chicken { get { return new Chicken(); } }
    }

    [Export("hey")]
    public class KeyedFactoryWithString
    {
        [Export]
        public string GetValue()
        {
            return "blah!";
        }
    }

}
