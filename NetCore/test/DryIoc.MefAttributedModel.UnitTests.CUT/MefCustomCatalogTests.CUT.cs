using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public interface IDummyService
    {
    	int GetValue();

    	bool GetFlag();

    	string GetString();
    }

    public interface IDummyServiceConsumer
    {
        IDummyService Single { get; }

        IDummyService[] Multiple { get; }
    }

    [Export(typeof(IDummyServiceConsumer))]
    internal class PrivateConsumer : IDummyServiceConsumer
    {
        [Import]
        public IDummyService Single { get; private set; }

        [ImportMany]
        public IDummyService[] Multiple { get; private set; }
    }
}
