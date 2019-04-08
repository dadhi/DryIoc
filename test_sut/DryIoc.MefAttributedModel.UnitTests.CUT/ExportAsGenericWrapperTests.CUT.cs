using System;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class FactoryConsumer
    {
        public FactoryConsumer(IFactory<One>[] ones)
        {
            if (ones.Length == 0) throw new InvalidOperationException("Empty factory list");
            One = ones[0].Create();
        }

        public One One { get; set; }
    }

    [Export("one"), Export("two")]
    public class One
    {
    }

    public interface IFactory<TService>
    {
        TService Create();
    }

    [ExportMany, AsWrapper]
    public class DryFactory<TService> : IFactory<TService>
    {
        public DryFactory(Func<TService> create)
        {
            _create = create;
        }

        public TService Create()
        {
            return _create();
        }

        private readonly Func<TService> _create;
    }

    [Export]
    public class FactoryWithArgsConsumer
    {
        public FactoryWithArgsConsumer(IFactory<string, Two>[] twos, string message)
        {
            Two = twos[0].Create(message);
        }

        public Two Two { get; set; }
    }

    [Export]
    public class Two
    {
        public string Message { get; set; }

        public Two(string message)
        {
            Message = message;
        }
    }

    public interface IFactory<TArg0, TService>
    {
        TService Create(TArg0 arg0);
    }

    [ExportMany, AsWrapper(1)]
    public class DryFactory<TArg0, TService> : IFactory<TArg0, TService>
    {
        public DryFactory(Func<TArg0, TService> create)
        {
            _create = create;
        }

        public TService Create(TArg0 arg0)
        {
            return _create(arg0);
        }

        private readonly Func<TArg0, TService> _create;
    }

}
