using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public enum FooMetadata { Hey, Blah, NotFound }

    [ExportMany, WithMetadata(FooMetadata.Hey)]
    public class FooHey : IFooService
    {
    }

    [ExportMany, WithMetadata(FooMetadata.Blah)]
    public class FooBlah : IFooService
    {
    }

    [ExportMany]
    public class FooConsumerNotFound
    {
        public IFooService Foo { get; set; }

        public FooConsumerNotFound([Import,WithMetadata(FooMetadata.NotFound)]IFooService foo)
        {
            Foo = foo;
        }
    }
}
