using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportMetadataTests
    {
        [Test]
        public void I_can_specify_metadata()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(MyCode));

            var code = container.Resolve<MyCode>();

            Assert.That(code.Tool, Is.Not.Null);
            Assert.That(code.ToolMeta, Is.EqualTo(MineMeta.Green));
        }

        [Test]
        public void I_should_be_able_to_import_single_service_based_on_specified_metadata()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FooConsumer), typeof(FooHey), typeof(FooBlah));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
        }

        [Test]
        public void Possible_to_use_key_value_metadata()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(X), typeof(A), typeof(XX));

            var xx = container.Resolve<XX>();
            Assert.IsNotNull(xx);
        }

        [Export]
        [WithMetadata("a", 1)]
        [WithMetadata("b")]
        public class X
        {
        }

        [ExportMany]
        public class A : X
        {
        }

        [Export]
        public class XX
        {
            public X X { get; private set; }

            public XX([Import, WithMetadata("a", 1)]X x)
            {
                X = x;
            }
        }
    }

    public enum MineMeta
    {
        Red,
        Green
    };

    [ExportMany]
    public class MyCode
    {
        public ExternalTool Tool { get; set; }
        public MineMeta ToolMeta { get; set; }

        public MyCode(
            [ImportExternal(Metadata = MineMeta.Green, ConstructorSignature = new Type[0])] 
            Tuple<Lazy<ExternalTool>, MineMeta> tool)
        {
            Tool = tool.Item1.Value;
            ToolMeta = tool.Item2;
        }
    }

    [ExportMany]
    public class FooConsumer
    {
        public Lazy<IFooService> Foo { get; set; }

        public FooConsumer([Import, WithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
        {
            Foo = foo;
        }
    }
}
