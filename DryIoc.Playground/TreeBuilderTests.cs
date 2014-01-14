using System.Collections;

namespace DryIoc.Playground
{
    public class TreeBuilderTests
    {
    }

    public sealed class ImmTree
    {
        public ImmTree Left { get; private set; }

        public class Builder
        {
            private ImmTree _tree;

            public Builder()
            {
                _tree = new ImmTree();
            }

            public ImmTree Build()
            {
                var tree = _tree;
                _tree = null;
                return tree;
            }

            public Builder From(IDictionary source)
            {
                return this;
            }

            public Builder WithLeft(ImmTree left)
            {
                _tree.Left = left;
                return this;
            }
        }
    }
}
