using System;
using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export, AsFactory]
    public class AppleFactory
    {
        public Apple Create()
        {
            return new Apple();
        }
    }

    [Export, AsFactory]
    public class OrangeFactory
    {
        [Export]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [ExportMany, AsFactory]
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

    [Export, AsFactory]
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

    [Export, AsFactory]
    public class TransientOrangeFactory
    {
        [Export, TransientReuse]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [Export, AsFactory]
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
}
