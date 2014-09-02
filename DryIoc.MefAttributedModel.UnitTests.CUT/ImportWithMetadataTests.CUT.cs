using System;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public enum FooMetadata { Hey, Blah, NotFound }

    [ExportAll, WithMetadata(FooMetadata.Hey)]
    public class FooHey : IFooService
    {
    }

    [ExportAll, WithMetadata(FooMetadata.Blah)]
    public class FooBlah : IFooService
    {
    }

    [ExportAll]
    public class FooConsumer
    {
        public Lazy<IFooService> Foo { get; set; }

        public FooConsumer([WithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
        {
            Foo = foo;
        }
    }

    [ExportAll]
    public class FooConsumerNotFound
    {
        public IFooService Foo { get; set; }

        public FooConsumerNotFound([WithMetadata(FooMetadata.NotFound)]IFooService foo)
        {
            Foo = foo;
        }
    }
}
