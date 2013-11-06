using System;

namespace DryIoc.AttributedRegistration.UnitTests.CUT
{
    public enum FooMetadata { Hey, Blah, NotFound }

    [ExportAll, ExportWithMetadata(FooMetadata.Hey)]
    public class FooHey : IFooService
    {
    }

    [ExportAll, ExportWithMetadata(FooMetadata.Blah)]
    public class FooBlah : IFooService
    {
    }

    [ExportAll]
    public class FooConsumer
    {
        public Lazy<IFooService> Foo { get; set; }

        public FooConsumer([ImportWithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
        {
            Foo = foo;
        }
    }

    [ExportAll]
    public class FooConsumerNotFound
    {
        public IFooService Foo { get; set; }

        public FooConsumerNotFound([ImportWithMetadata(FooMetadata.NotFound)]IFooService foo)
        {
            Foo = foo;
        }
    }
}
