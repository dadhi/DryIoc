using System;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    using Request = DryIocAttributes.Request;

    public interface IHandler
    {
    }

    [ExportMany(ContractName = "fast"), WithMetadata(2)]
    public class FastHandler : IHandler
    {
    }

    [ExportMany(ContractName = "slow"), WithMetadata(1)]
    public class SlowHandler : IHandler
    {
    }

    [ExportMany(ContractName = "transact"), WithMetadata(1)]
    public class TransactHandler : IHandler
    {
    }

    [ExportMany, AsDecorator(ContractName = "slow")]
    public class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportMany, AsDecorator]
    public class RetryHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public RetryHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportMany, AsDecorator("transact")]
    public class TransactHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public TransactHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [Export(typeof(IHandler)), AsDecorator, ForSlowHandler]
    public class CustomHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public CustomHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }

        public sealed class ForSlowHandler : ExportConditionAttribute
        {
            public override bool Evaluate(Request request)
            {
                return request.ImplementationType == typeof(SlowHandler);
            }
        }
    }

    [ExportMany, AsDecorator]
    public class DecoratorWithFastHandlerImport : IHandler
    {
        public IHandler Handler { get; set; }

        public DecoratorWithFastHandlerImport([Import("fast")]IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportEx(BlahFooh.Blah, typeof(IHandler))]
    public class BlahHandler : IHandler { }

    [ExportMany(ContractKey = BlahFooh.Fooh)]
    public class FoohHandler : IHandler { }

    public enum BlahFooh { Blah, Fooh }

    [ExportMany, AsDecorator(ContractKey = BlahFooh.Fooh)]
    public class FoohDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public FoohDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    public interface IDecoratedResult
    {
        int GetResult();
    }

    [Export(typeof(IDecoratedResult)), TransientReuse]
    public class DecoratedResult : IDecoratedResult
    {
        public int GetResult()
        {
            return 1;
        }
    }

    [Export(typeof(IDecoratedResult)), AsDecorator]
    public class FuncDecorator : IDecoratedResult
    {
        private readonly Func<IDecoratedResult> _decorated;

        public FuncDecorator(Func<IDecoratedResult> decorated)
        {
            _decorated = decorated;
        }

        public int GetResult()
        {
            return _decorated().GetResult() + 1;
        }
    }

    [Export(typeof(IDecoratedResult)), TransientReuse, AsDecorator, AsResolutionCall]
    public class DynamicDecorator : IDecoratedResult
    {
        private readonly IDecoratedResult _decorated;

        public DynamicDecorator(IDecoratedResult decorated)
        {
            _decorated = decorated;
        }

        public int GetResult()
        {
            return _decorated.GetResult() + 1;
        }
    }

    public interface IServiceA
    {
        int GetResult();
    }

    [Export(typeof(IServiceA))]
    public class ServiceAReal : IServiceA
    {
        public int GetResult()
        {
            return 1;
        }
    }

    [Export(typeof(IServiceA)), AsDecorator(Order = 1)]
    public class ServiceADecoratorInner : IServiceA
    {
        private readonly IServiceA decorated;

        public ServiceADecoratorInner(IServiceA decorated)
        {
            this.decorated = decorated;
        }

        public int GetResult()
        {
            return this.decorated.GetResult() + 1;
        }
    }

    [Export(typeof(IServiceA)), AsDecorator(Order = 2)]
    public class ServiceADecoratorOuter : IServiceA
    {
        private readonly IServiceA decorated;

        public ServiceADecoratorOuter(IServiceA decorated)
        {
            this.decorated = decorated;
        }

        public int GetResult()
        {
            return this.decorated.GetResult() + 1;
        }
    }
}
