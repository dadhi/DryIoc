using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public interface IHandler
    {
    }

    [ExportAll(ContractName = "fast"), WithMetadata(2)]
    public class FastHandler : IHandler
    {
    }

    [ExportAll(ContractName = "slow"), WithMetadata(1)]
    public class SlowHandler : IHandler
    {
    }

    [ExportAll(ContractName = "transact"), WithMetadata(1)]
    public class TransactHandler : IHandler
    {
    }

    [ExportAll, AsDecoratorFor(ContractName = "slow")]
    public class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, AsDecoratorFor, WithMetadata(2)]
    public class RetryHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public RetryHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, AsDecoratorFor(ContractName = "transact"), WithMetadata(1)]
    public class TransactHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public TransactHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [Export(typeof(IHandler)), AsDecoratorFor(ConditionType = typeof(Condition))]
    public class CustomHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public CustomHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }

        public class Condition : IDecoratorCondition
        {
            public bool CanApply(Request request)
            {
                return request.ImplementationType == typeof(SlowHandler);
            }
        }
    }

    [ExportAll, AsDecoratorFor]
    public class DecoratorWithFastHandlerImport : IHandler
    {
        public IHandler Handler { get; set; }

        public DecoratorWithFastHandlerImport([Import("fast")]IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportWithKey(BlahFooh.Blah, typeof(IHandler))]
    public class BlahHandler : IHandler { }

    [ExportAll(ContractKey = BlahFooh.Fooh)]
    public class FoohHandler : IHandler { }

    public enum BlahFooh { Blah, Fooh }

    [ExportAll, AsDecoratorFor(ContractKey = BlahFooh.Fooh)]
    public class FoohDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public FoohDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }
}
